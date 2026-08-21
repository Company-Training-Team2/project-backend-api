namespace EventHub.Application.Helpers;

/// <summary>
/// Resolves the real static-files root on disk, working around a quirk on
/// the deployed MonsterASP host: its site's physical path is itself named
/// "wwwroot" (e.g. D:\Sites\site83881\wwwroot), which becomes
/// IWebHostEnvironment.ContentRootPath. ASP.NET Core's own default
/// (ContentRootPath + "\wwwroot") then computes a doubled, nonexistent
/// D:\Sites\site83881\wwwroot\wwwroot — logged at startup as "The
/// WebRootPath was not found" — so uploaded public files (vendor logos,
/// WorkPost/portfolio images) were being saved into a folder
/// StaticFileMiddleware was never actually serving from, a guaranteed 404
/// no matter how correctly the upload itself succeeded.
///
/// Used by both Program.cs (to configure UseStaticFiles' FileProvider) and
/// LocalFileStorageService (to know where it's actually writing files) so
/// the two can never disagree on the real path.
/// </summary>
public static class WebRootResolver
{
    public static string Resolve(string contentRootPath)
    {
        var trimmed = contentRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var lastSegment = Path.GetFileName(trimmed);

        return lastSegment.Equals("wwwroot", StringComparison.OrdinalIgnoreCase)
            ? contentRootPath
            : Path.Combine(contentRootPath, "wwwroot");
    }
}
