using System.Reflection.Metadata;
using DevCL.Database.Model;
using DevCL.Exceptions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DevCL.Controllers;

[Route("collections/{id}/items")]
public class ItemsController : ControllerBase {
    MongoClient client;
    IMongoCollection<CLCollection> checklists;
    public ItemsController(MongoClient mongoClient) {
        client = mongoClient;
        checklists = client.GetDatabase("dev_cl").GetCollection<CLCollection>("collection");
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
        catch (CollectionNotInitializedException ex) {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "An unexpected error occured.");
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}