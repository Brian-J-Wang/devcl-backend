using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using DevCL.Database.Model;
using DevCL.Exceptions;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

    [HttpPatch("{itemId}")]
    public ActionResult UpdateTask(string id, string itemId, [FromBody] IncomingCLItem item) {
        try {
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);

            var update = Builders<CLCollection>.Update.Set("items.$[item].checked", item.IsChecked);

            var arrayFilter = new [] {
                new BsonDocument("item._id", ObjectId.Parse(itemId))
            };

            var options = new UpdateOptions {
                ArrayFilters = arrayFilter.Select(bson => new BsonDocumentArrayFilterDefinition<BsonDocument>(bson)).ToList()
            };

            checklists.UpdateOne(filter, update, options);

            return Ok(item);
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