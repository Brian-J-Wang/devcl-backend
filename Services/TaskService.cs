using MongoDB.Driver;
using DevCL.Model;
using MongoDB.Bson;

namespace DevCl.Services;

public class TaskService {
    readonly IMongoCollection<TaskItem> tasks;
    readonly AttributeService attributeService;
    public TaskService(MongoDbContext dbContext, AttributeService attributeService) {
        tasks = dbContext.Tasks;
        this.attributeService = attributeService;
    }

    public List<TaskItem> GetTasksFromCollection(string collectionId) {
        return tasks.Find(task => task.TaskDocId == collectionId).ToList();
    }

    public TaskItem AddTask(string docuId, PostTaskItem task) {
        var taskItem = task.ToTaskItem().WithTaskDocId(docuId);

        HashSet<ObjectId> recordedAttribute = new HashSet<ObjectId>();
        taskItem.Attributes = task.Attributes.Where((attribute) => {
            if (recordedAttribute.Contains(attribute.Id)) {
                return false;
            } else if (attributeService.ValidateTaskAttribute(attribute)) {
                Console.WriteLine("passed validation");
                recordedAttribute.Add(attribute.Id);
                return true;
            } else {
                return false;
            }
        }).ToList();

        tasks.InsertOne(taskItem);

        return taskItem;
    }

    //returns true if task was found and removed, otherwise false
    public bool DeleteTask(string itemId) {
        var result = tasks.DeleteOne(task => task.Id == itemId);
        return result.DeletedCount == 1;
    }

    public TaskItem ReplaceTask(string taskId, TaskItem taskItem) {
        var task = tasks.FindOneAndReplace(task => task.Id == taskId, taskItem, new FindOneAndReplaceOptions<TaskItem, TaskItem> {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = false
        });
        
        return task;
    }

    public TaskItem UpdateTask(string itemId, List<UpdateNugget> nuggets) {
        var result = tasks.FindOneAndUpdate(item => item.Id == itemId, BuildUpdateDefinition(nuggets), new FindOneAndUpdateOptions<TaskItem> {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = false
        });

        return result;
    }

    UpdateDefinition<TaskItem> BuildUpdateDefinition(List<UpdateNugget> nuggets) {
        var update = Builders<TaskItem>.Update.Combine();

        foreach (var nugget in nuggets) {
            var propertyName = nugget.PropertyName.ToCharArray();
            propertyName[0] = char.ToUpper(propertyName[0]);
            var propertyInfo = typeof(TaskItem).GetProperty(new string(propertyName));
            if (propertyInfo == null) {
                continue;
            }

            switch (nugget.UpdateType) {
                case UpdateType.add:
                    if (propertyInfo.IsDefined(typeof(Addable), inherit: false)) {
                        throw new NotImplementedException();
                    }
                    break;
                case UpdateType.remove:
                    if (propertyInfo.IsDefined(typeof(Removeable), inherit: false)) {
                        throw new NotImplementedException();
                    }
                    break;
                case UpdateType.update:
                    if (propertyInfo.IsDefined(typeof(Updateable), inherit: false)) {
                        update = update.Set(propertyInfo.Name, nugget.Value );
                    }
                    break;
            }
        }
        return update;
    }
}