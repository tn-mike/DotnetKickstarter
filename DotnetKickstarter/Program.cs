using DotnetKickstarter.Application.Interfaces;
using DotnetKickstarter.Application.Services;
using DotnetKickstarter.Infrastructure.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetKickstarter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IMapData, MapData>();
                services.AddTransient<IStructureFile, StructureFile>();
                services.AddTransient<IProcessService, ProcessService>();
            })
            .Build();

            var generator = host.Services.GetRequiredService<IProcessService>();
            var result = generator.Run();

            Console.WriteLine(result);
            //Console.WriteLine("Enter to exit...");
            //Console.ReadLine();

            host.RunAsync();
        }
    }
}
