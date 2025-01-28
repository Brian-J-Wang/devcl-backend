using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using DevCL.Database.Model;
using DevCL.Exceptions;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DevCL.Controllers;

[Route("collections/{id}/items")]
public class ItemsController : ControllerBase {
    IMongoCollection<CLCollection> checklists;
    public ItemsController(MongoClient mongoClient) {
        checklists = mongoClient.GetDatabase("dev_cl").GetCollection<CLCollection>("collection");
    }

    [HttpGet]
    public ActionResult GetTasks(string id) {
        Console.WriteLine("here");
        try {
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var results = checklists.AsQueryable()
                .Where(doc => doc.Id == id)
                .Select(doc => new { doc.Items }).ToList();

            return Ok(results);
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost]
    public ActionResult AddTask(string id, [FromBody] IncomingCLItem item) {
        try {
            CLItem entry = item.ToCLItem();

            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var update = Builders<CLCollection>.Update.Push(d => d.Items, entry);

            var result = checklists.UpdateOne(filter, update);

            return Ok(entry);
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPatch]
    public ActionResult UpdateTask(string id, [FromBody] IncomingCLItem item) {
        try {
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);

            var arrayFilter = new [] {
                new BsonDocument("section._id", ObjectId.Parse(item.Section)),
                new BsonDocument("item._id", ObjectId.Parse(item.Id))
            };

            var options = new UpdateOptions {
                ArrayFilters = arrayFilter.Select(bson => new BsonDocumentArrayFilterDefinition<BsonDocument>(bson)).ToList()
            };

            var update = Builders<CLCollection>.Update.Set("checkList.$[section].items.$[item].checked", item.IsChecked);

            checklists.UpdateOne(filter, update, options);

            var itemProjection = Builders<CLCollection>.Projection.Expression(u => u.Categories
                        .Where(section => section.Id == item.Section).First()
                        .Items.Where(item => item.Id == item.Id).First());
            CLItem response = checklists.Find(filter).Project(itemProjection).First();

            return Ok(response);
        }
        catch (InvalidOperationException) {
            Console.WriteLine("Something went wrong");
            return NotFound();
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpDelete("{postID}")]
    public ActionResult DeleteTask(string id, string postId) {
        try {
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var update = Builders<CLCollection>.Update.PullFilter(
                "items",
                Builders<BsonDocument>.Filter.Eq("id", postId)
            );

            checklists.UpdateOne(filter, update);

            return Ok(postId);
        }
        catch(Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}