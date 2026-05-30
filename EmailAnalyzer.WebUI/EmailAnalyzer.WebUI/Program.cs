using EmailAnalyzer.WebUI.Client.Pages;
using EmailAnalyzer.WebUI.Components;
using EmailAnalyzer.Infrastructure;
using EmailAnalyzer.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddScoped<IEmailParser, EmailParser>();
builder.Services.AddSingleton<AnalysisCounterService>();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();

// Middleware przekierowujący niezalogowanych użytkowników
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isPublic = path.StartsWithSegments("/login") ||
                   path.StartsWithSegments("/account") ||
                   path.StartsWithSegments("/_framework") ||
                   path.StartsWithSegments("/_blazor") ||
                   (context.Request.Path.Value?.Contains('.') == true);

    if (!isPublic && context.User.Identity?.IsAuthenticated != true)
    {
        var returnUrl = Uri.EscapeDataString(path + context.Request.QueryString);
        context.Response.Redirect($"/login?returnUrl={returnUrl}");
        return;
    }

    await next();
});

app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/account/login", async (HttpContext context, IConfiguration config) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString().Trim();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var expectedUsername = config["Auth:Username"];
    var expectedPassword = config["Auth:Password"];

    if (!string.IsNullOrEmpty(expectedUsername) &&
        username == expectedUsername &&
        password == expectedPassword)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        var redirect = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
        return Results.Redirect(redirect);
    }

    return Results.Redirect("/login?error=1");
}).DisableAntiforgery();

app.MapGet("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EmailAnalyzer.WebUI.Client._Imports).Assembly);

app.Run();
