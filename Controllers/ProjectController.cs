using DevCL.Database.Model;

using DevCL.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace DevCL.Controllers;

[ApiController]
[Route("taskDocs")]
public class ProjectController : ControllerBase {
    TaskDocService collectionService;

    public ProjectController(TaskDocService collectionService) {
        this.collectionService = collectionService;
    }

    [Authorize]
    [HttpGet]
    public ActionResult GetUserProjects([FromHeader] string authorization) {
        try {
            var userList = collectionService.GetUserProjects(authorization);
            return Ok(userList);
        }
        catch(Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [Authorize]
    [HttpPost]
    public ActionResult CreateNewProject([FromHeader] string authorization, [FromBody] NewProjectRequest request) {
        try {
            var doc = collectionService.CreateNewProject(authorization, request);
            return Ok(doc.ToJson());
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "An unexpected error occured.");
        }
    }
    
    [HttpGet("{docuId}")]
    public ActionResult GetProject(string docuId) {
        try {
            var doc = collectionService.GetTaskDoc(docuId);
            return Ok(doc.ToJson());
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured.");
        }
    }

    [Authorize]
    [HttpDelete("{projectId}")]
    public ActionResult DeleteProject([FromHeader] string authorization, string projectId) {
        try {
            Console.WriteLine(projectId);
            collectionService.DeleteProject(authorization, projectId);
            return Ok(new {
                    id = projectId
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