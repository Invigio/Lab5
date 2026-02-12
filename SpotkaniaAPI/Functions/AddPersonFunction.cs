using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SpotkaniaAPI.Models;
using Newtonsoft.Json;

namespace SpotkaniaAPI.Functions;

/// <summary>
/// Funkcja do dodawania nowych osób do systemu
/// </summary>
public class AddPersonFunction
{
    private readonly ILogger<AddPersonFunction> _logger;
    private readonly Container _container;

    public AddPersonFunction(
        ILogger<AddPersonFunction> logger,
        Container container)
    {
        _logger = logger;
        _container = container;
    }

    [Function("AddPerson")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "persons")]
        HttpRequestData req)
    {
        _logger.LogInformation("Processing POST /api/persons request");

        try
        {
            // Odczytaj body requestu
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var person = JsonConvert.DeserializeObject<Person>(requestBody);

            if (person == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid request body" });
                return badResponse;
            }

            // Walidacja
            if (string.IsNullOrWhiteSpace(person.Name))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Name is required" });
                return badResponse;
            }

            if (string.IsNullOrWhiteSpace(person.Email))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Email is required" });
                return badResponse;
            }

            // Wygeneruj ID jeśli nie podano
            if (string.IsNullOrWhiteSpace(person.Id))
            {
                person.Id = Guid.NewGuid().ToString();
            }

            // Ustaw datę utworzenia
            person.CreatedAt = DateTime.UtcNow;

            // Upewnij się że lista BookedSlots jest zainicjalizowana
            person.BookedSlots ??= new List<BookedSlot>();

            // Zapisz do Cosmos DB
            var response = await _container.CreateItemAsync(person, new PartitionKey(person.Id));

            _logger.LogInformation($"Person created: {person.Id} - {person.Name}");

            // Zwróć odpowiedź
            var successResponse = req.CreateResponse(HttpStatusCode.Created);
            await successResponse.WriteAsJsonAsync(response.Resource);
            return successResponse;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning($"Person already exists");
            var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
            await conflictResponse.WriteAsJsonAsync(new { error = "Person with this ID already exists" });
            return conflictResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating person: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }
}
