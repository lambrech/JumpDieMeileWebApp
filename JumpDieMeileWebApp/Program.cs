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
 * Performance when loading ~10000 rows - not much we can do right now. problem is deserialize performance in webassembly
 * On saving handling / On loading handling
 * Fail saving loading handling
 *
 * After register pages - ok for initial
 *
 * Initial load animation
 *
 * security issue all data visible in post
 *
 * help 4 auto resizing iframe (remove scrollbar when resizing)
 *
 * use observer: https://www.geeksforgeeks.org/how-to-detect-the-change-in-divs-dimension/
 * send message to parent: https://betterprogramming.pub/how-to-automatically-resize-an-iframe-7be6bfbb1214
 */