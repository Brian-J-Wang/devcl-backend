using DevCL.Database.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DevCL.Controllers;

[ApiController]
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

    
    // public ActionResult AddTask(string id, [FromBody] IncomingCLItem item) {
    //     try {
    //         CLItem entry = item.ToCLItem();

    //         var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
    //         var update = Builders<CLCollection>.Update.Push(d => d.Items, entry);

    //         var result = checklists.UpdateOne(filter, update);

    //         return Ok(entry);
    //     }
    //     catch (Exception) {
    //         return StatusCode(500, "An unexpected error occured");
    //     }
    // }

    [HttpPost]
    public ActionResult AddTask(string id, [FromBody] CLTask item) {
        try {
            if (!item.SatisfiesPostRequirement()) {
                return BadRequest();
            }

            item.GenerateId();

            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var update = Builders<CLCollection>.Update.Push(d => d.Items, item);

            var result = checklists.UpdateOne(filter, update);

            return Ok(item);
            // if (!item.SatisfiesPostRequirement()) {
            //     throw new Exception();
            // }

            // var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            // var update = Builders<CLCollection>.Update.Push
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPatch("{itemId}")]
    public ActionResult UpdateTask(string id, string itemId, [FromBody] CLTask item) {
        try {
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            
            var document = checklists.Find(filter).FirstOrDefault();
            
            if (document == null) return NotFound();
            
            var targetItems = document.Items.Find((item) => item.Id == itemId);
            if (targetItems == null ) return NotFound();

            foreach (KeyValuePair<string, object> kvp in item.Attributes) {
                if (targetItems.Attributes.ContainsKey(kvp.Key)) {
                    targetItems.Attributes[kvp.Key] = kvp.Value;
                } else {
                    targetItems.Attributes.Add(kvp.Key, kvp.Value);
                }
            }

            checklists.ReplaceOne(filter, document);

            return Ok(item);
        }
        catch (InvalidOperationException) {
            Console.WriteLine("Something went wrong");
            return NotFound();
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpDelete("{postID}")]
    public ActionResult DeleteTask(string id, string postId) {
        try {
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var update = Builders<CLCollection>.Update.PullFilter(
                "items",
                Builders<CLItem>.Filter.Eq(d => d.Id, postId)
            );

            checklists.UpdateOne(filter, update);

            return Ok(postId);
        }
        catch(Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}