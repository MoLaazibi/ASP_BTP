using AP.BTP.Application.Extensions;
using AP.BTP.Infrastructure.Extensions;
using AP.BTP.UI.Components;
using AP.BTP.UI.Endpoints;
using AP.BTP.UI.Handlers;
using AP.BTP.UI.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
namespace AP.BTP.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                .AddJsonFile("appsettings.ui.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.ui.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            builder.Services.RegisterApplication();
            builder.Services.RegisterInfrastructure(builder.Configuration);
            builder.Services.AddMemoryCache();

            // Data Protection keys (shared with API)
            // In Docker, use /app/dataprotection-keys; locally, use relative path
            var keysPath = builder.Environment.IsProduction()
                ? "/app/dataprotection-keys"
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "dataprotection-keys"));

            // Ensure directory exists
            if (!Directory.Exists(keysPath))
            {
                Directory.CreateDirectory(keysPath);
            }

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
                .SetApplicationName("BTP-SharedAuth");

            builder.Services.AddHttpClient();

            builder.Services.AddAntiforgery();

            builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.AccessDeniedPath = "/accessdenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                    options.Cookie.Name = ".AspNetCore.Cookies";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    options.SlidingExpiration = true;
                });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthorization();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddTransient<CookieHandler>();
            builder.Services.AddTransient<BearerTokenHandler>();
            builder.Services.AddSingleton<TokenValidationService>();
            builder.Services.AddHttpClient("API", (serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var apiBaseUrl = configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");
                client.BaseAddress = new Uri(apiBaseUrl);
            })
            .AddHttpMessageHandler<CookieHandler>()
            .AddHttpMessageHandler<BearerTokenHandler>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", policy =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.WithOrigins("http://localhost:5087")
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else
                    {
                        // Production: Allow specific frontend origins with credentials
                        policy.WithOrigins("http://10.156.2.6:8080", "http://frontend:8080", "http://localhost:8080")
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });
            builder.Services.AddServerSideBlazor();
            var app = builder.Build();

            app.MapAuthEndpoints();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files")),
                RequestPath = "/Files"
            });

            app.UseCors("AllowLocalhost");

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
            app.MapControllers();

            app.Run();
        }


    }
}
