using System.Net;
using System.Text.Json;
using DK.EFootballClub.TimetableDataUsvc.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DK.EFootballClub.TimetableDataUsvc;

public class TimetableHttpTrigger(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimetableHttpTrigger>();
    private readonly string? _dbConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
    private readonly string? _dbName = Environment.GetEnvironmentVariable("DATABASE_NAME");

   [Function("GetAllTimetables")]
    public async Task<HttpResponseData> GetAllTimetables(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "timetables")] HttpRequestData req)
    {
        var response = req.CreateResponse();
        try
        {
            var db = new MongoDbService(_dbConnectionString, _dbName);
            List<Timetable> timetables  = await db.GetAllTimetablesAsync();
            await response.WriteAsJsonAsync(timetables);
            response.Headers.Add("Location", $"/api/timetables");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching timetables");
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("CreateTimetable")]
    public async Task<HttpResponseData> CreateTimetable(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "timetables")] HttpRequestData req)
    {
        var response = req.CreateResponse();

        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var timetable = JsonSerializer.Deserialize<Timetable>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (timetable == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Invalid timetable data.");
                return response;
            }

            var db = new MongoDbService(_dbConnectionString, _dbName);
            Timetable? createdTimetable = await db.CreateTimetableAsync(timetable);

            response.StatusCode = HttpStatusCode.Created;
            response.Headers.Add("Location", $"/api/timetables/{createdTimetable!.Id}");
            await response.WriteAsJsonAsync(createdTimetable);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating timetable");
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("UpdateTimetable")]
    public async Task<HttpResponseData> UpdateTimetabler(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "timetables/{id}")] HttpRequestData req,
        string id)
    {
        var response = req.CreateResponse();

        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedTimetableData = JsonSerializer.Deserialize<Timetable>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (updatedTimetableData == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Invalid timetable data.");
                return response;
            }

            var db = new MongoDbService(_dbConnectionString, _dbName);
            var updatedTimetable = await db.UpdateTimetableAsync(id, updatedTimetableData);

            if (updatedTimetable == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteStringAsync($"Timetable with ID {id} not found.");
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Headers.Add("Location", $"/api/timetables/{updatedTimetable.Id}");
            await response.WriteAsJsonAsync(updatedTimetable);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating timetable with ID {id}");
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }

    [Function("DeleteTimetable")]
    public async Task<HttpResponseData> DeleteTimetable(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "timetables/{id}")] HttpRequestData req,
        string id)
    {
        var response = req.CreateResponse();

        try
        {
            var db = new MongoDbService(_dbConnectionString, _dbName);
            var success = await db.DeleteTimetableAsync(id);

            if (!success)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteStringAsync($"Timetable with ID {id} not found.");
                return response;
            }

            response.StatusCode = HttpStatusCode.NoContent;
            response.Headers.Add("Location", $"/api/timetables");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting timetable with ID {id}");
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync("Internal server error");
            return response;
        }
    }
}