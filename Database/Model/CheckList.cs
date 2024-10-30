using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DevCL.Database.Model;

public class CheckList {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonRequired]
    public required string owner { get; set; }
    [BsonRequired]
    public required string checklistName { get; set; }
    [BsonRequired]
    public required string version { get; set; }
    public List<PatchNotes> patchNotes = new List<PatchNotes>();
    [BsonElement("checkList")]
    public List<CLCategory> checkListSection = new List<CLCategory>();
}

public class PatchNotes {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string version { get; set; }
    public List<string> content { get; set; } = new List<string>();
}

public class CLCategory {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public required string name { get; set; }
    public required string format;
    public List<CLItem> items { get; set; } = new List<CLItem>();
}

public class CLItem {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonDefaultValue(false)]
    [BsonElement("checked")]
    public bool IsChecked { get; set; } = false;
}

public class IncomingCLItem {
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Section { get; set; }
    public Boolean? IsChecked { get; set; }

    public CLItem ToCLItem() {
        return new CLItem() {
            Id = Id ?? ObjectId.GenerateNewId().ToString(),
            Title = Title ?? "",
            IsChecked = IsChecked ?? false
        };
    }
}

