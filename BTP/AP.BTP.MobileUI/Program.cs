using AP.BTP.MobileUI.Handlers;
using AP.BTP.MobileUI.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace AP.BTP.MobileUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, MobileAuthenticationStateProvider>();
            builder.Services.AddScoped<MobileAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddTransient<AuthorizationHeaderHandler>();

            var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5107";
            if (!apiBaseUrl.EndsWith("/")) apiBaseUrl += "/";

            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
            .AddHttpMessageHandler<AuthorizationHeaderHandler>();

            await builder.Build().RunAsync();
        }
    }
}
