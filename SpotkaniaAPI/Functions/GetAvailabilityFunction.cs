using System.Globalization;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SpotkaniaAPI.Models;

namespace SpotkaniaAPI.Functions;

/// <summary>
/// Funkcja do pobierania dostępnych slotów czasowych dla danej osoby w konkretnym dniu
/// </summary>
public class GetAvailabilityFunction
{
    private readonly ILogger<GetAvailabilityFunction> _logger;
    private readonly Container _container;

    public GetAvailabilityFunction(
        ILogger<GetAvailabilityFunction> logger,
        Container container)
    {
        _logger = logger;
        _container = container;
    }

    [Function("GetAvailability")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "availability/{id}")]
        HttpRequestData req,
        string id)
    {
        _logger.LogInformation($"Processing GET /api/availability/{id} request");

        try
        {
            // Pobierz parametr date z query string
            var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var dateParam = queryParams["date"];

            if (string.IsNullOrWhiteSpace(dateParam))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Date parameter is required (format: YYYY-MM-DD)" });
                return badResponse;
            }

            // Walidacja formatu daty
            if (!DateTime.TryParseExact(dateParam, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime requestedDate))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid date format. Use YYYY-MM-DD" });
                return badResponse;
            }

            // Pobierz osobę z Cosmos DB
            ItemResponse<Person> personResponse;
            try
            {
                personResponse = await _container.ReadItemAsync<Person>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { error = "Person not found" });
                return notFoundResponse;
            }

            var person = personResponse.Resource;

            // Określ dzień tygodnia po angielsku
            var dayOfWeek = requestedDate.ToString("dddd", CultureInfo.InvariantCulture);
            _logger.LogInformation($"Checking availability for {person.Name} on {dayOfWeek} ({dateParam})");

            // Sprawdź czy osoba pracuje w tym dniu
            if (!person.WorkHours.ContainsKey(dayOfWeek) || !person.WorkHours[dayOfWeek].Enabled)
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new AvailabilityResponse
                {
                    PersonId = person.Id,
                    PersonName = person.Name,
                    Date = dateParam,
                    DayOfWeek = dayOfWeek,
                    AvailableSlots = new List<string>() // Pusta lista - nie pracuje tego dnia
                });
                return response;
            }

            var workDay = person.WorkHours[dayOfWeek];

            // Generuj wszystkie możliwe sloty 30-minutowe
            var allSlots = GenerateTimeSlots(workDay.Start, workDay.End);

            // Pobierz zarezerwowane sloty dla tej daty
            var bookedSlotsForDate = person.BookedSlots
                .Where(slot => slot.Date == dateParam)
                .Select(slot => slot.Time)
                .ToHashSet();

            // Usuń zarezerwowane sloty z listy dostępnych
            var availableSlots = allSlots
                .Where(slot => !bookedSlotsForDate.Contains(slot))
                .ToList();

            _logger.LogInformation($"Found {availableSlots.Count} available slots for {person.Name} on {dateParam}");

            // Zwróć odpowiedź
            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteAsJsonAsync(new AvailabilityResponse
            {
                PersonId = person.Id,
                PersonName = person.Name,
                Date = dateParam,
                DayOfWeek = dayOfWeek,
                AvailableSlots = availableSlots
            });
            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting availability: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// Generuje listę slotów czasowych co 30 minut między startTime i endTime
    /// </summary>
    /// <param name="startTime">Godzina rozpoczęcia (format: HH:mm)</param>
    /// <param name="endTime">Godzina zakończenia (format: HH:mm)</param>
    /// <returns>Lista stringów z godzinami w formacie HH:mm</returns>
    private List<string> GenerateTimeSlots(string startTime, string endTime)
    {
        var slots = new List<string>();

        if (!TimeSpan.TryParse(startTime, out TimeSpan start) ||
            !TimeSpan.TryParse(endTime, out TimeSpan end))
        {
            _logger.LogWarning($"Invalid time format: start={startTime}, end={endTime}");
            return slots;
        }

        var current = start;
        while (current < end)
        {
            slots.Add(current.ToString(@"HH\:mm"));
            current = current.Add(TimeSpan.FromMinutes(30));
        }

        return slots;
    }
}
