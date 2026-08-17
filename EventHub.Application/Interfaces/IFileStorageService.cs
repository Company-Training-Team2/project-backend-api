using Microsoft.AspNetCore.Http;

namespace EventHub.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file somewhere publicly servable (under wwwroot) and returns
    /// a URL path the frontend can render directly (e.g. an "img src"),
    /// like "/uploads/vendors/42/logo/ab12cd34.png".
    /// </summary>
    Task<string> SavePublicAsync(IFormFile file, string subfolder);

    /// <summary>
    /// Saves a file outside wwwroot, where the static-files middleware
    /// cannot serve it directly. For anything an anonymous visitor should
    /// never be able to open by guessing a URL - identity documents,
    /// business licenses. Returns an opaque relative path (not a URL) that
    /// only StreamPrivateAsync can resolve back into bytes, gated behind
    /// an authenticated/authorized endpoint.
    /// </summary>
    Task<string> SavePrivateAsync(IFormFile file, string subfolder);

    /// <summary>Reads a file previously saved by SavePrivateAsync. Returns
    /// null if the relative path does not resolve to a real file.</summary>
    Task<(Stream Stream, string ContentType, string FileName)?> StreamPrivateAsync(string relativePath);
}
