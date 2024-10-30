using DevCL.Database;
using DevCL.Database.Model;
using DevCL.Exceptions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace DevCL.Controllers;

[ApiController]
[Route("collections")]
public class CollectionController : ControllerBase {
    [HttpGet("{id}")]
    public ActionResult GetCollection(string id) {
        //add verification that the person using this document is the owner of the document

        try {
            var document = CLCollections.RetreiveDocument(id);
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

    [HttpPatch("{id}/items")]
    public ActionResult UpdateTask(string id, [FromBody] IncomingCLItem item) {
        try {
            Console.WriteLine(item.ToJson().ToString());
            CLItem response = CLCollections.UpdateTask(id, item);

            return Ok(response);
        }
        catch (InvalidOperationException) {
            Console.WriteLine("Soemthing went wrong");
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

public enum PatchType {
    major,
    minor,
    patch
}

public class UpdateParams {
    public PatchType PatchType { get; set; }
}