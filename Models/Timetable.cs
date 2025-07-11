using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DK.EFootballClub.TimetableDataUsvc.Models;

public class Timetable
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("team")]
    public string Team { get; set; } = string.Empty;

    [BsonElement("league")]
    public string League { get; set; } = string.Empty;

    [BsonElement("MatchesID")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string>? Matches { get; set; }

    [BsonElement("season")]
    public string Season { get; set; } = string.Empty;
    
}