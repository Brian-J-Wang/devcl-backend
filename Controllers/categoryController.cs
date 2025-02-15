using DevCL.Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DevCL.Controllers;

[Route("collections/{id}/category")]
public class CategoryController : ControllerBase {
    IMongoCollection<CLCollection> checklists;
    public CategoryController(MongoClient mongoClient) {
        checklists = mongoClient.GetDatabase("dev_cl").GetCollection<CLCollection>("collection");
    }

    [Authorize]
    [HttpPost]
    public ActionResult AddCategory(string id, [FromBody] NewCategory req) {
        try {
            CLCategory newCategory = new CLCategory() {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = req.Name,
                format = req.Format
            };
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var update = Builders<CLCollection>.Update.Push(d => d.Categories, newCategory);

            var document = checklists.FindOneAndUpdate(filter, update);

            return Ok(newCategory.ToJson());
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [Authorize]
    [HttpDelete]
    public ActionResult RemoveCategory(string id, [FromBody] DeleteCategory req) {
        try {
            Console.WriteLine("here");
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var update = Builders<CLCollection>.Update.PullFilter(
                "categories",
                Builders<CLCategory>.Filter.Eq(d => d.Id, req.Id)
            );

            checklists.UpdateOne(filter, update);

            return Ok(req.ToJson());
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [Authorize]
    [HttpPatch]
    public ActionResult UpdateCategory(string id, [FromBody] UpdateCategory req) {
        return StatusCode(500, "Not Implemented");

    }
}