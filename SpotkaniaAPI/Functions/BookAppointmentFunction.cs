using System.Globalization;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SpotkaniaAPI.Models;
using Newtonsoft.Json;

namespace SpotkaniaAPI.Functions;

/// <summary>
/// Funkcja do rezerwacji spotkania
/// </summary>
public class BookAppointmentFunction
{
    private readonly ILogger<BookAppointmentFunction> _logger;
    private readonly Container _container;

    public BookAppointmentFunction(
        ILogger<BookAppointmentFunction> logger,
        Container container)
    {
        _logger = logger;
        _container = container;
    }

    [Function("BookAppointment")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "book")]
        HttpRequestData req)
    {
        _logger.LogInformation("Processing POST /api/book request");

        try
        {
            // Odczytaj body requestu
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var bookingRequest = JsonConvert.DeserializeObject<BookAppointmentRequest>(requestBody);

            if (bookingRequest == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid request body" });
                return badResponse;
            }

            // Walidacja
            if (string.IsNullOrWhiteSpace(bookingRequest.PersonId))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "PersonId is required" });
                return badResponse;
            }

            if (string.IsNullOrWhiteSpace(bookingRequest.Date))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Date is required" });
                return badResponse;
            }

            if (string.IsNullOrWhiteSpace(bookingRequest.Time))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Time is required" });
                return badResponse;
            }

            if (string.IsNullOrWhiteSpace(bookingRequest.ClientName))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "ClientName is required" });
                return badResponse;
            }

            if (string.IsNullOrWhiteSpace(bookingRequest.ClientEmail))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "ClientEmail is required" });
                return badResponse;
            }

            // Walidacja formatu daty
            if (!DateTime.TryParseExact(bookingRequest.Date, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime requestedDate))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid date format. Use YYYY-MM-DD" });
                return badResponse;
            }

            // Walidacja formatu czasu
            if (!TimeSpan.TryParse(bookingRequest.Time, out _))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid time format. Use HH:mm" });
                return badResponse;
            }

            // Pobierz osobę z Cosmos DB
            ItemResponse<Person> personResponse;
            try
            {
                personResponse = await _container.ReadItemAsync<Person>(
                    bookingRequest.PersonId,
                    new PartitionKey(bookingRequest.PersonId));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { error = "Person not found" });
                return notFoundResponse;
            }

            var person = personResponse.Resource;

            // Sprawdź czy osoba pracuje w tym dniu
            var dayOfWeek = requestedDate.ToString("dddd", CultureInfo.InvariantCulture);
            if (!person.WorkHours.ContainsKey(dayOfWeek) || !person.WorkHours[dayOfWeek].Enabled)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = $"Person does not work on {dayOfWeek}" });
                return badResponse;
            }

            // Sprawdź czy godzina mieści się w godzinach pracy
            var workDay = person.WorkHours[dayOfWeek];
            var requestedTime = TimeSpan.Parse(bookingRequest.Time);
            var startTime = TimeSpan.Parse(workDay.Start);
            var endTime = TimeSpan.Parse(workDay.End);

            if (requestedTime < startTime || requestedTime >= endTime)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new
                {
                    error = $"Time {bookingRequest.Time} is outside working hours ({workDay.Start} - {workDay.End})"
                });
                return badResponse;
            }

            // Sprawdź czy slot nie jest już zajęty
            var isSlotBooked = person.BookedSlots.Any(slot =>
                slot.Date == bookingRequest.Date && slot.Time == bookingRequest.Time);

            if (isSlotBooked)
            {
                var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                await conflictResponse.WriteAsJsonAsync(new
                {
                    error = "This time slot is already booked"
                });
                return conflictResponse;
            }

            // Utwórz nowy BookedSlot
            var newBooking = new BookedSlot
            {
                Date = bookingRequest.Date,
                Time = bookingRequest.Time,
                ClientName = bookingRequest.ClientName,
                ClientEmail = bookingRequest.ClientEmail,
                Description = bookingRequest.Description,
                BookedAt = DateTime.UtcNow
            };

            // Dodaj do listy BookedSlots
            person.BookedSlots.Add(newBooking);

            // Zaktualizuj dokument w Cosmos DB
            var updatedPerson = await _container.ReplaceItemAsync(
                person,
                person.Id,
                new PartitionKey(person.Id));

            _logger.LogInformation($"Appointment booked for {person.Name}: {bookingRequest.Date} {bookingRequest.Time}");

            // Zwróć odpowiedź
            var successResponse = req.CreateResponse(HttpStatusCode.Created);
            await successResponse.WriteAsJsonAsync(new
            {
                message = "Appointment booked successfully",
                personId = person.Id,
                personName = person.Name,
                date = bookingRequest.Date,
                time = bookingRequest.Time,
                clientName = bookingRequest.ClientName,
                booking = newBooking
            });
            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error booking appointment: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }
}
