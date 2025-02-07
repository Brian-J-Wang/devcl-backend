using System.IdentityModel.Tokens.Jwt;
using DevCL.Database;
using DevCL.Database.Model;
using DevCL.Exceptions;
using DevCL.Extensions.JWT;
using DevCL.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DevCL.Controllers;

[ApiController]
[Route("collections")]
public class CollectionController : ControllerBase {
    MongoClient client;
    IMongoCollection<CLCollection> checklists;
    JwtSecurityTokenHandler tokenHandler;
    public CollectionController(MongoClient mongoClient, JwtSecurityTokenHandler handler) {
        client = mongoClient;
        checklists = client.GetDatabase("dev_cl").GetCollection<CLCollection>("collection");
        tokenHandler = handler;
    }

    [Authorize]
    [HttpGet]
    public ActionResult GetUserCollections([FromHeader] string authorization) {
        try {
            string userId = tokenHandler.ExtractUserId(authorization);

            var filter = Builders<CLCollection>.Filter.Eq(d => d.Owner, userId);
            var userCollections = checklists.Find(filter);

            return Ok(userCollections.ToList());
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    [Authorize]
    [HttpPost]
    public ActionResult CreateNewCollection([FromBody] NewCollectionRequest request) {
        try 
        {
            CLCollection collection = new CLCollection() {
                Id = ObjectId.GenerateNewId().ToString(),
                Owner = request.User,
                Name = request.Name,
                Version = "0.0.1"
            };

            return Ok(collection);
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured.");
        }
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
}



public enum PatchType {
    major,
    minor,
    patch
}

public class UpdateParams {
    public PatchType PatchType { get; set; }
}