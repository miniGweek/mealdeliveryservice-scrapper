using MealDeliveryServiceScrapper;
using MealDeliveryServiceScrapper.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((hostBuilderContext, loggerConfiguration) =>
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("MealDeliveryFood.log");
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<ExtractPapaMacrosNutritionInfo>();
    })
    .Build();

await host.Services.GetRequiredService<ExtractPapaMacrosNutritionInfo>().Extract();