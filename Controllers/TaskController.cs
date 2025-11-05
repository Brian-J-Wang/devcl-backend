using DevCl.Services;
using DevCL.Database.Model;
using DevCL.Model;
using Microsoft.AspNetCore.Mvc;

namespace DevCL.Controllers;

[ApiController]
[Route("collections/{docuId}/tasks")]
public class TaskController : ControllerBase {
    TaskService taskService;
    public TaskController(TaskService taskService) {
        this.taskService = taskService; 
    }

    [HttpGet]
    public ActionResult GetTasks(string docuId) {
        try {
            var tasks = taskService.GetTasksFromCollection(docuId);
            return Ok(tasks);
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost]
    public ActionResult AddTask(string docuId, [FromBody] PostTaskItem item) {
        try {
            var result = taskService.AddTask(docuId, item);
            return Ok(result);
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPatch("{itemId}")]
    public ActionResult UpdateTask(string itemId, [FromBody] UpdateTaskItem item) {
        try {
            var result = taskService.UpdateTask(itemId, item);
            return Ok(result);
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
    public ActionResult DeleteTask(string postId) {
        try {
            taskService.DeleteTask(postId);
            return Ok(postId);
        }
        catch(Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}