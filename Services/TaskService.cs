using MongoDB.Driver;
using DevCL.Model;

namespace DevCl.Services;

public class TaskService {
    readonly IMongoCollection<TaskItem> tasks;
    public TaskService(MongoDbContext dbContext) {
        tasks = dbContext.Tasks;
    }

    public List<TaskItem> GetTasksFromCollection(string collectionId) {
        return tasks.Find(task => task.TaskDocId == collectionId).ToList();
    }

    public TaskItem AddTask(string docuId, PostTaskItem task) {
        var taskItem = task.ToTaskItem().WithTaskDocId(docuId);
        tasks.InsertOne(taskItem);

        return taskItem;
    }

    //returns true if task was found and removed, otherwise false
    public Boolean DeleteTask(string itemId) {
        var result = tasks.DeleteOne(task => task.Id == itemId);
        return result.DeletedCount == 1;
    }

    public TaskItem UpdateTask(string itemId, UpdateTaskItem task) {
        var result = tasks.FindOneAndUpdate(item => item.Id == itemId, task.GetUpdateDefinition(), new FindOneAndUpdateOptions<TaskItem> {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = false
        });

        return result;
    }
}