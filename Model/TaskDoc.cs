using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Text.Json.Serialization;

namespace DevCL.Database.Model;

[BsonIgnoreExtraElements]
public class TasksDoc {
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

public class CLItem {
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id"),]
    public string? Id { get; set; }

    [BsonElement("blurb"), JsonPropertyName("blurb")]
    public string? Blurb { get; set; }

    [BsonDefaultValue(false), BsonElement("checked"), JsonPropertyName("checked")]
    public bool Checked { get; set; }

    [BsonElement("category"), JsonPropertyName("category")]
    public string? Category { get; set; }
}

public class Collaborator {
    [BsonRepresentation(BsonType.String), JsonPropertyName("_id")]
    public string Id { get; set; }

    [BsonElement("alias"), JsonPropertyName("alias")]
    public string Alias { get; set; }

    [BsonElement("email"), JsonPropertyName("email")]
    public string Email { get; set; }
}

public class Identifier {
    public Identifier(string collection, string category = "", string item = "") {
        this.collection = collection;
        this.category = category;
        this.item = item;
    }

    public string collection;
    public string category;
    public string item;   
}

public class PatchItem {
    [JsonPropertyName("category")]
    public string? Category { get; set;}

    [JsonPropertyName("checked")]
    public bool? Checked { get; set; }

    [JsonPropertyName("blurb")]
    public bool? Blurb { get; set; }
}

public class NewCollectionRequest {
    [JsonPropertyName("title")]
    public required string Title { get; set; }
}

//property names must match the field names in the database since it's name is used 
//generate the update builder.
