using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Text.Json.Serialization;

namespace DevCL.Database.Model;

public class CLCollection {
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id")]
    public string Id { get; set; }

    [BsonRequired, BsonElement("owner"), JsonPropertyName("owner")]
    public required string Owner { get; set; }

    [BsonRequired, BsonElement("name"), JsonPropertyName("name")]
    public required string Name { get; set; }

    [BsonRequired, BsonElement("version"), JsonPropertyName("version")]
    public required string Version { get; set; }

    [BsonElement("patches"), JsonPropertyName("patches")]
    public List<PatchNotes> patchNotes = new List<PatchNotes>();

    [BsonElement("categories"), JsonPropertyName("categories")]
    public List<CLCategory> Categories = new List<CLCategory>();

    [BsonElement("items"), JsonPropertyName("items")]
    public List<CLItem> Items { get; set; } = new List<CLItem>();
}

public class PatchNotes {
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id")]
    public string Id { get; set; }

    [BsonElement("version"), JsonPropertyName("version")]
    public required string Version { get; set; }

    [BsonElement("content"), JsonPropertyName("content")]
    public List<string> Content { get; set; } = new List<string>();
}

public class CLCategory {
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id")]
    public string? Id { get; set; }

    [BsonElement("name"), JsonPropertyName("name")]
    public required string Name { get; set; }
    public required string format;

    [BsonElement("items"), JsonPropertyName("items")]
    public List<CLItem> Items { get; set; } = new List<CLItem>();
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

public class IncomingCLItem {
    public string? Id { get; set; }
    [BsonElement("blurb"), JsonPropertyName("blurb")]
    public string? Title { get; set; }
    [BsonElement("category"), JsonPropertyName("category")]
    public string? Section { get; set; }
    [BsonElement("checked"), JsonPropertyName("checked")]
    public Boolean? IsChecked { get; set; }

    public CLItem ToCLItem() {
        return new CLItem() {
            Id = Id ?? ObjectId.GenerateNewId().ToString(),
            Blurb = Title ?? "",
            Checked = IsChecked ?? false,
            Category = Section ?? "null"
        };
    }

    public UpdateDefinition<CLCollection> GetUpdateDefinition() {
        var update = Builders<CLCollection>.Update.Combine();

        if (Title != null) {
            update = update.Set("items.$[item].blurb", Title);
        }

        if (Section != null) {
            update = update.Set("items.$[item].category", Section);
        }

        if (IsChecked != null) {
            update = update.Set("items.$[item].checked", IsChecked);
        }

        return update;
    }
}


//property names must match the field names in the database since it's name is used 
//generate the update builder.
