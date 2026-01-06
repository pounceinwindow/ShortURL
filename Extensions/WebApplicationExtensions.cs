using URLShortener.Endpoints;

namespace URLShortener.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseStaticFrontend(this WebApplication app)
    {
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            DefaultFileNames = new List<string> { "login.html" }
        });
        app.UseStaticFiles();
        return app;
    }

    public static WebApplication MapAppEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapLinkEndpoints();
        return app;
    }
}