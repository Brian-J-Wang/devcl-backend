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

    public IMongoCollection<Project> Collection => db.GetCollection<Project>("collection");
    public IMongoCollection<User> Users => db.GetCollection<User>("users");
    public IMongoCollection<TaskItem> Tasks => db.GetCollection<TaskItem>("tasks");
    public IMongoCollection<Attribute> Attributes => db.GetCollection<Attribute>("attributes");
    public IMongoCollection<Attribute> SharedAttributes => db.GetCollection<Attribute>("shared_attributes");
}