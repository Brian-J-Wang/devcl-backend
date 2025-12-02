using DevCL.Database.Model;

using DevCL.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace DevCL.Controllers;

[ApiController]
[Route("taskDocs")]
public class TaskDocController : ControllerBase {
    TaskDocService collectionService;

    public TaskDocController(TaskDocService collectionService) {
        this.collectionService = collectionService;
    }

    [Authorize]
    [HttpGet]
    public ActionResult GetUserTaskDocs([FromHeader] string authorization) {
        try {
            var userList = collectionService.GetTaskDocsByUser(authorization);
            return Ok(userList);
        }
        catch(Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [Authorize]
    [HttpPost]
    public ActionResult CreateNewTaskDoc([FromHeader] string authorization, [FromBody] NewCollectionRequest request) {
        try {
            var doc = collectionService.AddNewTaskDoc(authorization, request);
            return Ok(doc.ToJson());
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "An unexpected error occured.");
        }
    }
    
    [HttpGet("{docuId}")]
    public ActionResult GetTaskDoc(string docuId) {
        try {
            var doc = collectionService.GetTaskDoc(docuId);
            return Ok(doc.ToJson());
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public ActionResult DeleteTaskDoc([FromHeader] string authorization, string docuId) {
        try {
            collectionService.DeleteTaskDoc(authorization, docuId);
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