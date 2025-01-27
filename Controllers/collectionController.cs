using DevCL.Database;
using DevCL.Database.Model;
using DevCL.Exceptions;
using DevCL.Requests;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DevCL.Controllers;

[ApiController]
[Route("collections")]
public class CollectionController : ControllerBase {
    MongoClient client;
    IMongoCollection<CLCollection> checklists;
    public CollectionController(MongoClient mongoClient) {
        client = mongoClient;
        checklists = client.GetDatabase("dev_cl").GetCollection<CLCollection>("collection");
    }

    [HttpGet("{id}")]
    public ActionResult GetCollection(string id) {
        //add verification that the person using this document is the owner of the document
        try {
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var document = checklists.Find(filter).First();

            return Ok(document.ToJson());
        }
        catch (InvalidOperationException ex) {
            Console.WriteLine(ex.Message);
            return NotFound();
        }
        catch (CollectionNotInitializedException ex) {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [HttpPatch("{id}/version")]
    public ActionResult UpdateCollection(string id, [FromBody] UpdateParams updateParams) {
        if (updateParams == null) {
            return BadRequest("Request Params is required");
        }

        try {
            PatchNotes response = CLCollections.PushNewVersion(id, updateParams.PatchType);

            return Ok(response);
        } 
        catch (InvalidOperationException) {
            Console.WriteLine($"Document {id} could not be found");
            return NotFound();
        }
        catch (CollectionNotInitializedException ex) {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    //roll backs the latest patch notes
    [HttpDelete("{id}/version")]
    public ActionResult DeletePatchNote(string id) {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/items")]
    public ActionResult AddNewTask(string id, [FromBody] IncomingCLItem item) {
        try {
            CLItem response = CLCollections.AddNewTask(id, item);

            return Ok(response);
        }
        catch (InvalidOperationException) {
            Console.WriteLine($"Document {id} could not be found");
            return NotFound();
        }
        catch (CollectionNotInitializedException ex) {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [HttpPatch("{id}")]
    public ActionResult PatchDocument(string id, [FromBody] CheckListPatchRequest request) {
        
        try {
            CLCollections.UpdateDocument(id, request);
            return Ok(request);
        }
        catch(Exception) {
            return StatusCode(500, "An unexpected error occured");
        } 
    }


    [HttpPatch("{id}/items")]
    public ActionResult UpdateTask(string id, [FromBody] IncomingCLItem item) {
        try {
            Console.WriteLine(item.ToJson().ToString());
            CLItem response = CLCollections.UpdateTask(id, item);

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

    [HttpPatch("{collectionId}/category/item")]
    public ActionResult PatchItem([FromRoute] string collectionId, [FromQuery] string categoryId, [FromQuery] string itemId, [FromBody] PatchItem item) {
        Identifier id = new Identifier(collectionId, categoryId, itemId);

        try {
            CLItem response = CLCollections.PatchItem(id, item);

            return Ok(response);
        }
        catch (Exception) {
            return StatusCode(500, "Something went wrong");
        } 
    }

    [HttpPost("{collectionId}/category/item")]
    public ActionResult PostItem([FromRoute] string collectionId, [FromQuery] string categoryId, [FromBody] PostItem postItem) {
        CLItem item = postItem.toCLItem();

        Identifier id = new Identifier(collectionId, categoryId, item.Id ?? throw new Exception());

        try {
            CLItem response = CLCollections.PostItem(id, item);

            return Ok(response);
        }
        catch (Exception) {
            return StatusCode(500, "Something went wrong");
        }
    }

    [HttpDelete("{collectionId}/category/item")]
    public ActionResult DeleteItem([FromRoute] string collectionId, [FromQuery] string categoryId, [FromQuery] string itemId) {
        Identifier id = new Identifier(collectionId, categoryId, itemId);

        try {
            CLCollections.DeleteItem(id);

            return Ok();
        }
        catch (Exception) {
            return StatusCode(500, "Something went wrong");
        }
    }
}



public enum PatchType {
    major,
    minor,
    patch
}

public class UpdateParams {
    public PatchType PatchType { get; set; }
}