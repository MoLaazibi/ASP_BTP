using AP.BTP.API.Middleware;
using AP.BTP.API.Services;
using AP.BTP.Application.Extensions;
using AP.BTP.Infrastructure.Contexts;
using AP.BTP.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AP.BTP.API
{
    public class Program
    {
        private const string MultiAuthScheme = "MultiAuth";

        // Protected constructor allows subclassing for testing or other scenarios
        protected Program()
        {
        }
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureConfiguration(builder);
            ConfigureServices(builder);

            var app = builder.Build();

            ApplyMigrations(app);
            LogDataProtectionKeysPath(app);

            ConfigureMiddleware(app);

            app.Run();
        }

        #region Helper

        private static void ConfigureConfiguration(WebApplicationBuilder builder)
        {
            builder.Configuration
                .AddJsonFile("appsettings.api.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.api.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.RegisterApplication();
            builder.Services.RegisterInfrastructure(builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache();
            builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
            builder.Services.AddSingleton<GeminiChatService>();
            builder.Services.AddScoped<TaskContextService>();
            builder.Services.Configure<DefaultUserOptions>(builder.Configuration.GetSection("DefaultUser"));
            builder.Services.AddSingleton<DefaultUserStore>();

            ConfigureDataProtection(builder);

            ConfigureAuthenticationAndAuthorization(builder);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", policy =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.WithOrigins("http://localhost:5087", "https://localhost:7162")
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithOrigins("http://10.156.2.6:8080", "http://frontend:8080", "http://localhost:8080")
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type => type.FullName?.Replace(".", "_"));
            });
        }

        private static void ConfigureDataProtection(WebApplicationBuilder builder)
        {
            var keysPath = builder.Environment.IsProduction()
                ? "/app/dataprotection-keys"
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "dataprotection-keys"));

            if (!Directory.Exists(keysPath))
                Directory.CreateDirectory(keysPath);

            builder.Services.AddLogging(b => b.AddConsole());

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
                .SetApplicationName("BTP-SharedAuth");
        }

        private static void ConfigureAuthenticationAndAuthorization(WebApplicationBuilder builder)
        {
            var auth0Domain = builder.Configuration["Auth0:Domain"];
            var auth0Audience = builder.Configuration["Auth0:Audience"];

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = Program.MultiAuthScheme;
                    options.DefaultAuthenticateScheme = Program.MultiAuthScheme;
                    options.DefaultChallengeScheme = Program.MultiAuthScheme;
                })
                .AddPolicyScheme(Program.MultiAuthScheme, "Cookie or Bearer", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        return !string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                            ? JwtBearerDefaults.AuthenticationScheme
                            : CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                })
                .AddCookie(options =>
                {
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                    options.Cookie.Name = ".AspNetCore.Cookies";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    options.SlidingExpiration = true;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://{auth0Domain}/";
                    options.Audience = auth0Audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = $"https://{auth0Domain}/",
                        ValidAudience = auth0Audience,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AllowAnonymousAccess", policy => policy.RequireAssertion(_ => true));
            });
        }

        private static void ApplyMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<BTPContext>();
                var migrationLogger = services.GetRequiredService<ILogger<Program>>();
                migrationLogger.LogInformation("Applying database migrations...");
                context.Database.Migrate();
                migrationLogger.LogInformation("Database migrations applied successfully.");
            }
            catch (Exception ex)
            {
                var migrationLogger = services.GetRequiredService<ILogger<Program>>();
                migrationLogger.LogError(ex, "An error occurred while applying migrations.");
            }
        }

        private static void LogDataProtectionKeysPath(WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Data Protection Keys Path: /app/dataprotection-keys, Environment: {Environment}, IsProduction: {IsProduction}",
                app.Environment.EnvironmentName, app.Environment.IsProduction());
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
            });

            var fopts = app.Services.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
            fopts.KnownNetworks.Clear();
            fopts.KnownProxies.Clear();

            app.UseCors("AllowLocalhost");
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower();
                if (path != null && (path.StartsWith("/swagger") ||
                                     path.StartsWith("/health")))
                {
                    await next.Invoke();
                    return;
                }
                await next.Invoke();
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.MapControllers();
        }

        #endregion
    }
}
