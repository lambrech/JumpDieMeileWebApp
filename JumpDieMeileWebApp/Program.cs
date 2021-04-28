namespace JumpDieMeileWebApp
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Models;
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

            //var mem = new MemoryPersistenceProvider();
            //builder.Services.AddSingleton<IPersistenceProvider>(mem);
            builder.Services.AddSingleton<IPersistenceProvider>(new DbRelayPersistenceProvider());


            //for (int i = 0; i < 300; i++)
            //{
            //    var runner = new Runner { FirstName = $"TestP{i}", LastName = "Alter", Username = $"run2_dude-{i}", Email = "web@web.de" };
            //    await mem.PersistRunner(runner);
            //}

            await builder.Build().RunAsync();
        }
    }
}

/*
 * Work to do:
 * Performance when loading ~10000 rows
 * On saving handling / On loading handling
 *
 * After register pages
 */