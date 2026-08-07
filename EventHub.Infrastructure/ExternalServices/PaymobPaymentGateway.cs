using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.Application.DTOs.Payment;
using EventHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EventHub.Infrastructure.ExternalServices;

/// <summary>
/// Payment module: Paymob Accept API integration (Test/Free tier).
/// Flow: Auth token -> Order registration -> Payment key -> Iframe checkout,
/// exactly as documented at https://developers.paymob.com. All secrets come
/// from configuration (appsettings "Paymob" section / environment) — never
/// hardcoded. Re-verify field names/endpoints against Paymob's current docs
/// before going live, since gateway APIs can change without notice.
/// </summary>
public class PaymobPaymentGateway : IPaymentGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _hmacSecret;
    private readonly string _integrationId;
    private readonly string _iframeId;
    private readonly string _baseUrl;

    public PaymobPaymentGateway(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Paymob:ApiKey"] ?? string.Empty;
        _hmacSecret = config["Paymob:HmacSecret"] ?? string.Empty;
        _integrationId = config["Paymob:IntegrationId"] ?? string.Empty;
        _iframeId = config["Paymob:IframeId"] ?? string.Empty;
        _baseUrl = (config["Paymob:BaseUrl"] ?? "https://accept.paymob.com").TrimEnd('/');
    }

    public async Task<PaymentGatewayResult> CreatePaymentKeyAsync(PaymentGatewayRequest request)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_integrationId) || string.IsNullOrWhiteSpace(_iframeId))
        {
            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = "Paymob is not configured (ApiKey/IntegrationId/IframeId missing in configuration)."
            };
        }

        try
        {
            var amountCents = (long)Math.Round(request.AmountEgp * 100m, MidpointRounding.AwayFromZero);
            var merchantOrderId = $"EVH-{request.BookingId}-{request.PaymentId}";

            var authToken = await GetAuthTokenAsync();
            var orderId = await RegisterOrderAsync(authToken, amountCents, merchantOrderId);
            var paymentToken = await RequestPaymentKeyAsync(authToken, amountCents, orderId, request);

            var checkoutUrl = $"{_baseUrl}/api/acceptance/iframes/{_iframeId}?payment_token={paymentToken}";

            return new PaymentGatewayResult
            {
                Success = true,
                PaymentToken = paymentToken,
                CheckoutUrl = checkoutUrl,
                GatewayOrderId = orderId
            };
        }
        catch (Exception ex)
        {
            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>Step 1 — POST /api/auth/tokens.</summary>
    private async Task<string> GetAuthTokenAsync()
    {
        var payload = new { api_key = _apiKey };

        var response = await PostAsync("/api/auth/tokens", payload);

        var result = JsonSerializer.Deserialize<PaymobAuthResponse>(response, JsonOptions)
            ?? throw new Exception("Paymob auth response could not be parsed.");

        if (string.IsNullOrWhiteSpace(result.Token))
            throw new Exception("Paymob authentication failed: no token returned.");

        return result.Token;
    }

    /// <summary>Step 2 — POST /api/ecommerce/orders.</summary>
    private async Task<long> RegisterOrderAsync(string authToken, long amountCents, string merchantOrderId)
    {
        var payload = new
        {
            auth_token = authToken,
            delivery_needed = false,
            amount_cents = amountCents,
            currency = "EGP",
            merchant_order_id = merchantOrderId,
            items = Array.Empty<object>()
        };

        var response = await PostAsync("/api/ecommerce/orders", payload);

        var result = JsonSerializer.Deserialize<PaymobOrderResponse>(response, JsonOptions)
            ?? throw new Exception("Paymob order response could not be parsed.");

        return result.Id;
    }

    /// <summary>Step 3 — POST /api/acceptance/payment_keys.</summary>
    private async Task<string> RequestPaymentKeyAsync(
        string authToken,
        long amountCents,
        long orderId,
        PaymentGatewayRequest request)
    {
        var payload = new
        {
            auth_token = authToken,
            amount_cents = amountCents,
            expiration = 3600,
            order_id = orderId,
            currency = "EGP",
            integration_id = long.TryParse(_integrationId, out var intId) ? intId : (object)_integrationId,
            billing_data = new
            {
                first_name = request.CustomerFirstName,
                last_name = request.CustomerLastName,
                email = request.CustomerEmail,
                phone_number = string.IsNullOrWhiteSpace(request.CustomerPhone) ? "NA" : request.CustomerPhone,
                apartment = "NA",
                floor = "NA",
                street = "NA",
                building = "NA",
                shipping_method = "NA",
                postal_code = "NA",
                city = "NA",
                country = "EG",
                state = "NA"
            }
        };

        var response = await PostAsync("/api/acceptance/payment_keys", payload);

        var result = JsonSerializer.Deserialize<PaymobPaymentKeyResponse>(response, JsonOptions)
            ?? throw new Exception("Paymob payment key response could not be parsed.");

        if (string.IsNullOrWhiteSpace(result.Token))
            throw new Exception("Paymob did not return a payment key token.");

        return result.Token;
    }

    private async Task<string> PostAsync(string path, object payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}{path}", content);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Paymob request to {path} failed ({(int)response.StatusCode}): {body}");

        return body;
    }

    /// <summary>
    /// HMAC verification per https://developers.paymob.com/paymob-docs/developers/webhook-callbacks-and-hmac:
    /// concatenate the listed transaction fields (exact order below), SHA-512
    /// hash with the HMAC secret, compare as lowercase hex against the "hmac"
    /// query parameter Paymob sends with the callback.
    /// </summary>
    public bool VerifyWebhookSignature(string receivedHmac, PaymobTransactionCallbackDto callback)
    {
        if (string.IsNullOrWhiteSpace(receivedHmac) || string.IsNullOrWhiteSpace(_hmacSecret))
            return false;

        var concatenated = string.Concat(
            callback.AmountCents.ToString(),
            callback.CreatedAt,
            callback.Currency,
            BoolStr(callback.ErrorOccured),
            BoolStr(callback.HasParentTransaction),
            callback.Id.ToString(),
            callback.IntegrationId.ToString(),
            BoolStr(callback.Is3DSecure),
            BoolStr(callback.IsAuth),
            BoolStr(callback.IsCapture),
            BoolStr(callback.IsRefunded),
            BoolStr(callback.IsStandalonePayment),
            BoolStr(callback.IsVoided),
            callback.Order.Id.ToString(),
            callback.Owner.ToString(),
            BoolStr(callback.Pending),
            callback.SourceData?.Pan ?? string.Empty,
            callback.SourceData?.SubType ?? string.Empty,
            callback.SourceData?.Type ?? string.Empty,
            BoolStr(callback.Success));

        using var hmacSha512 = new HMACSHA512(Encoding.UTF8.GetBytes(_hmacSecret));

        var hash = hmacSha512.ComputeHash(Encoding.UTF8.GetBytes(concatenated));

        var computedHex = Convert.ToHexStringLower(hash);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHex),
            Encoding.UTF8.GetBytes(receivedHmac.Trim().ToLowerInvariant()));
    }

    private static string BoolStr(bool value) => value ? "true" : "false";

    // ─── Minimal response shapes (only the fields we actually need) ──────────

    private class PaymobAuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }

    private class PaymobOrderResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    private class PaymobPaymentKeyResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
