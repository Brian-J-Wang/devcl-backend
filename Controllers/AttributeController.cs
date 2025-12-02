using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

[ApiController, Route("taskDocs/{docuId}/attributes")]
public class AttributeController : ControllerBase {
    AttributeService attributeService;
    public AttributeController(AttributeService service) {
        attributeService = service;
    }

    [HttpGet]
    public ActionResult GetAttributes(string docuId) {
        try {
            var attributes = attributeService.GetAttributes(docuId);
            return Ok(attributes);
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "An unexpected error occured");
        }
    }
}