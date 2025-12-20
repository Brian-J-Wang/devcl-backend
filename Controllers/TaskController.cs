using DevCl.Services;
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
        catch (Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [HttpPost("{taskId}")]
    public ActionResult ReplaceTask(string taskId, [FromBody] TaskItem task) {
        var result = taskService.ReplaceTask(taskId, task);
        return Ok(result);
    }

    [HttpPatch("{itemId}")]
    public ActionResult UpdateTask(string itemId, [FromBody] List<UpdateNugget> nuggets) {
        var result = taskService.UpdateTask(itemId, nuggets);
        return Ok(result);
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