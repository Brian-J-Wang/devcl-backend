using System.Text.Json;
using System.Text.Json.Serialization;
using DevCL.Utils.Converters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DevCL.Model;

public enum Status {
    incomplete,
    inprogress,
    complete
}

public class TaskItem {
    [BsonId, BsonRepresentation(BsonType.ObjectId), BsonElement("_id"), JsonPropertyName("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId), BsonElement("taskDocId"), JsonPropertyName("taskDocId")]
    public required string TaskDocId { get; set; }

    [Updateable]
    public string Blurb { get; set; } = "";

    [BsonDefaultValue(Status.incomplete), Updateable]
    public Status Status { get; set; } = Status.incomplete;
    [BsonElement("attributes"), JsonPropertyName("attributes")]
    public List<TaskAttribute> Attributes { get; set; } = new List<TaskAttribute>();
    [Updateable]
    public List<SubTask> SubTasks { get; set; } = new List<SubTask>();
 
    public TaskItem WithTaskDocId(string taskDocId) {
        TaskDocId = taskDocId;
        return this;
    }
}

public class UpdateTaskItem : IUpdateDefintion<TaskItem> {
    public string Id { get; } = "";
    public string? Blurb { get; set; }
    public Status? Status { get; set; }
    public List<SubTask>? SubTasks { get; set; }

    public UpdateDefinition<TaskItem> GetUpdateDefinition() {
        var update = Builders<TaskItem>.Update.Combine();

        foreach (var prop in typeof(UpdateTaskItem).GetProperties()) {
            if (prop.Name == "Id") {
                continue;
            }

            var value = prop.GetValue(this);
            if (value != null) {
                update = update.Set(prop.Name, value);
            }
        }
        
        return update;
    }
}

public enum UpdateType {
    add,
    remove,
    update
}

public class UpdateNugget {
    public string PropertyName { get; set; } = "";
    public JsonElement Value { get; set; }
    public UpdateType UpdateType { get; set; }
}

public class PostTaskItem {
    public string Id { get; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("blurb"), JsonPropertyName("blurb")]
    public required string Blurb { get; set; }
    
    public List<TaskAttribute> Attributes { get; set; } = new List<TaskAttribute>();

    public TaskItem ToTaskItem() {
        return new TaskItem() {
            Id = Id,
            TaskDocId = "",
            Blurb = Blurb,
        };
    }
}

public class TaskAttribute {
    public ObjectId Id { get; set; }
    [BsonElement("value"), BsonSerializer(typeof(JsonElementSerializer))]
    public required JsonElement Value { get; set; }
}

public enum SubTaskState {
    complete,
    incomplete
}

public class SubTask {
    [BsonRepresentation(BsonType.ObjectId)]
    public required ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public required string Blurb { get; set; }
    public required SubTaskState State { get; set;}
}