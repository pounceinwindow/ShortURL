using URLShortener.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseStaticFrontend();

app.UseAuthentication();
app.UseAuthorization();

app.MapAppEndpoints();

app.Run();