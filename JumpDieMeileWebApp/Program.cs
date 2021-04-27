namespace JumpDieMeileWebApp
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Persistence;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using MudBlazor.Services;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddMudServices();

            builder.Services.AddSingleton<IPersistenceProvider>(new DbRelayPersistenceProvider());
            //builder.Services.AddSingleton<IPersistenceProvider>(new MemoryPersistenceProvider());

            await builder.Build().RunAsync();
        }
    }
}