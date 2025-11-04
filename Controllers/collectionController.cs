using DevCL.Database.Model;
using DevCL.Exceptions;

using DevCL.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace DevCL.Controllers;

[ApiController]
[Route("collections")]
public class CollectionController : ControllerBase {
    CollectionService collectionService;

    public CollectionController(CollectionService collectionService) {
        this.collectionService = collectionService;
    }

    [Authorize]
    [HttpGet]
    public ActionResult GetUserCollections([FromHeader] string authorization) {
        try {
            var userList = collectionService.GetDocumentsByUser(authorization);
            return Ok(userList);
        }
        catch(Exception) {
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [Authorize]
    [HttpPost]
    public ActionResult CreateNewCollection([FromHeader] string authorization, [FromBody] NewCollectionRequest request) {
        try {
            var doc = collectionService.AddNewDocument(authorization, request);
            return Ok(doc.ToJson());
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured.");
        }
    }
    
    [HttpGet("{docuId}")]
    public ActionResult GetCollection(string docuId) {
        try {
            var doc = collectionService.GetDocument(docuId);
            return Ok(doc.ToJson());
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public ActionResult DeleteCollection([FromHeader] string authorization, string docuId) {
        try {
            collectionService.DeleteDocument(authorization, docuId);
            return Ok(new {
                    id = docuId
                });
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [HttpPatch("{id}/version")]
    public ActionResult UpdateCollection(string id, [FromBody] UpdateParams updateParams) {
        throw new NotImplementedException();
    }

    //roll backs the latest patch notes
    [HttpDelete("{id}/version")]
    public ActionResult DeletePatchNote(string id) {
        throw new NotImplementedException();
    }

    [HttpPatch("{id}")]
    public ActionResult PatchDocument(string id, [FromBody] CheckListPatchRequest request) {
        throw new NotImplementedException();
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