using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SpotkaniaAPI.Models;

namespace SpotkaniaAPI.Functions;

/// <summary>
/// Funkcje pomocnicze do pobierania osób
/// </summary>
public class GetPersonsFunction
{
    private readonly ILogger<GetPersonsFunction> _logger;
    private readonly Container _container;

    public GetPersonsFunction(
        ILogger<GetPersonsFunction> logger,
        Container container)
    {
        _logger = logger;
        _container = container;
    }

    /// <summary>
    /// Pobiera wszystkie osoby z systemu
    /// </summary>
    [Function("GetAllPersons")]
    public async Task<HttpResponseData> GetAll(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "persons")]
        HttpRequestData req)
    {
        _logger.LogInformation("Processing GET /api/persons request");

        try
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<Person>(query);

            var persons = new List<Person>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                persons.AddRange(response);
            }

            _logger.LogInformation($"Retrieved {persons.Count} persons");

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteAsJsonAsync(persons);
            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting persons: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// Pobiera konkretną osobę po ID
    /// </summary>
    [Function("GetPersonById")]
    public async Task<HttpResponseData> GetById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "persons/{id}")]
        HttpRequestData req,
        string id)
    {
        _logger.LogInformation($"Processing GET /api/persons/{id} request");

        try
        {
            var person = await _container.ReadItemAsync<Person>(id, new PartitionKey(id));

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteAsJsonAsync(person.Resource);
            return successResponse;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(new { error = "Person not found" });
            return notFoundResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting person: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }
}
