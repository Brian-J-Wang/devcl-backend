using Microsoft.Extensions.Options;
using configs;
using MongoDB.Driver;
using DevCL.Database.Model;
using DevCL.Model;

public class MongoDbContext
{
    public readonly IMongoDatabase db;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        MongoClient client = new MongoClient(options.Value.ConnectionString);
        db = client.GetDatabase("dev_cl");
    }

    public IMongoCollection<TasksDoc> Collection => db.GetCollection<TasksDoc>("collection");
    public IMongoCollection<User> Users => db.GetCollection<User>("users");
    public IMongoCollection<TaskItem> Tasks => db.GetCollection<TaskItem>("tasks");
}