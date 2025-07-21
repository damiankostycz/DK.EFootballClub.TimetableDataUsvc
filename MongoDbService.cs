using DK.EFootballClub.TimetableDataUsvc.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DK.EFootballClub.TimetableDataUsvc;

public class MongoDbService
{    
    private readonly IMongoCollection<Timetable> _timetables;

    public MongoDbService(string? connectionString, string? databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _timetables = database.GetCollection<Timetable>("Timetables");
    }

    public async Task<List<Timetable>> GetAllTimetablesAsync()
    {
        return await _timetables.Find(_ => true).ToListAsync();
    }

    public async Task<Timetable?> GetTimetableByIdAsync(string id)
    {
        var filter = Builders<Timetable>.Filter.Eq("_id", ObjectId.Parse(id));
        return await _timetables.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Timetable?> CreateTimetableAsync(Timetable timetable)
    {
        await _timetables.InsertOneAsync(timetable);
        return timetable;
    }

    public async Task<Timetable?> UpdateTimetableAsync(string id, Timetable updatedTimetable)
    {
        var filter = Builders<Timetable>.Filter.Eq("_id", ObjectId.Parse(id));
        var result = await _timetables.ReplaceOneAsync(filter, updatedTimetable);

        if (result.ModifiedCount > 0)
        {
            return await GetTimetableByIdAsync(id);
        }

        return null;
    }

    public async Task<bool> DeleteTimetableAsync(string id)
    {
        var filter = Builders<Timetable>.Filter.Eq("_id", ObjectId.Parse(id));
        var result = await _timetables.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
}