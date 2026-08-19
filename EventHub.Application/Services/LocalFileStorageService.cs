using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace EventHub.Application.Services;

/// <summary>
/// Saves uploads to disk on the API's own host (MonsterASP). No third-party
/// storage account needed, but note the tradeoff spelled out where this is
/// registered in Program.cs: files here only survive as long as the deploy
/// pipeline does not wipe/replace this directory on each release.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    public LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SavePublicAsync(IFormFile file, string subfolder)
    {
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            // WebRootPath is only null if wwwroot was never created. Fall
            // back to ContentRootPath/wwwroot rather than throwing.
            webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        var folder = Path.Combine(webRoot, "uploads", subfolder);
        var fileName = await SaveToDiskAsync(file, folder);
        var relativePath = $"/uploads/{subfolder}/{fileName}".Replace('\\', '/');

        // Was returned (and stored on the entity) as this bare relative path.
        // The frontend and API live on completely different domains
        // (Vercel vs MonsterASP), so an <img src="/uploads/..."> resolved
        // against the *frontend's* own origin instead of the API's — a
        // guaranteed 404/"failed to load" for every uploaded image, no
        // matter how correctly the file itself was saved and served.
        // Prefixing the API's own scheme+host (as the current request
        // actually reached it) makes the URL absolute and independent of
        // whichever origin ends up rendering it.
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return relativePath;

        return $"{request.Scheme}://{request.Host}{relativePath}";
    }

    public async Task<string> SavePrivateAsync(IFormFile file, string subfolder)
    {
        // Deliberately outside wwwroot - UseStaticFiles only ever serves
        // wwwroot, so nothing under App_Data is reachable by URL.
        var folder = Path.Combine(_env.ContentRootPath, "App_Data", "verification-uploads", subfolder);
        var fileName = await SaveToDiskAsync(file, folder);
        return Path.Combine(subfolder, fileName).Replace('\\', '/');
    }

    public Task<(Stream Stream, string ContentType, string FileName)?> StreamPrivateAsync(string relativePath)
    {
        var root = Path.Combine(_env.ContentRootPath, "App_Data", "verification-uploads");
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));

        // Defend against a path-traversal relativePath (e.g. "../../secrets.txt")
        // ever resolving outside the verification-uploads root.
        if (!fullPath.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<(Stream, string, string)?>(null);

        if (!File.Exists(fullPath))
            return Task.FromResult<(Stream, string, string)?>(null);

        if (!ContentTypeProvider.TryGetContentType(fullPath, out var contentType))
            contentType = "application/octet-stream";

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<(Stream, string, string)?>((stream, contentType, Path.GetFileName(fullPath)));
    }

    private static async Task<string> SaveToDiskAsync(IFormFile file, string folder)
    {
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }
}
