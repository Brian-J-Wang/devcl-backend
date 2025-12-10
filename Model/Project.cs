using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace DevCL.Database.Model;

[BsonIgnoreExtraElements]
public class Project {
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRequired, BsonElement("owner"), JsonPropertyName("owner")]
    public required string Owner { get; set; }

    [BsonRequired, BsonElement("name"), JsonPropertyName("name")]
    public required string Name { get; set; }

    [BsonRequired, BsonElement("version"), JsonPropertyName("version")]
    public string Version { get; set; } = "0.0.1";

    [BsonElement("collaborators"), JsonPropertyName("collaborators")]
    public List<Collaborator> Collaborators { get; set; } = new List<Collaborator>();
}

public class Collaborator {
    [BsonRepresentation(BsonType.String), JsonPropertyName("_id")]
    public string Id { get; set; }

    [BsonElement("alias"), JsonPropertyName("alias")]
    public string Alias { get; set; }

    [BsonElement("email"), JsonPropertyName("email")]
    public string Email { get; set; }
}

public class NewProjectRequest {
    public required string Name { get; set; }
}