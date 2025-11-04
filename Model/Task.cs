using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DevCL.Model;

public enum Status {
    incomplete,
    inprogress,
    complete
}

public class TaskItem {
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId), BsonElement("taskDocId"), JsonPropertyName("taskDocId")]
    public required string TaskDocId { get; set; }

    [BsonElement("blurb"), JsonPropertyName("blurb")]
    public string Blurb { get; set; } = "";

    [BsonDefaultValue(Status.incomplete), BsonElement("checked"), JsonPropertyName("checked")]
    public Status Status { get; set; } = Status.incomplete;

    public TaskItem WithTaskDocId(string taskDocId) {
        TaskDocId = taskDocId;
        return this;
    }
}

public class UpdateTaskItem {
    public string Id { get; } = "";
    public string? Blurb;

    public UpdateDefinition<TaskItem> GetUpdateDefinition() {
        var update = Builders<TaskItem>.Update.Combine();

        if (Blurb != null) {
            update = update.Set("blurb", Blurb);
        }

        return update;
    }
}

public class PostTaskItem {
    public string Id { get; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("blurb"), JsonPropertyName("blurb")]
    public required string Blurb { get; set; }

    public TaskItem ToTaskItem() {
        return new TaskItem() {
            Id = Id,
            TaskDocId = "",
            Blurb = Blurb
        };
    }
}