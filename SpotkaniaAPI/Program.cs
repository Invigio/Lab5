using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Konfiguracja Cosmos DB
        var configuration = context.Configuration;
        var cosmosConnectionString = configuration["CosmosDbConnectionString"];
        var databaseName = configuration["DatabaseName"];
        var containerName = configuration["ContainerName"];

        services.AddSingleton(serviceProvider =>
        {
            var cosmosClient = new CosmosClient(cosmosConnectionString);
            return cosmosClient.GetContainer(databaseName, containerName);
        });
    })
    .Build();

host.Run();
