using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MKCards.Client.Services;

namespace MKCards.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Add HttpClient
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Add SignalR client services
            // builder.Services.AddSingleton<HubConnectionBuilder>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

            // Add logging
            builder.Services.AddLogging();

            builder.Services.AddSingleton<PlayerHubConnection>();

            var host = builder.Build();

            var playerHubConnectionService = host.Services.GetRequiredService<PlayerHubConnection>();
            await playerHubConnectionService.StartConnectionAsync();

            await host.RunAsync();
        }
    }
}
