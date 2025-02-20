using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using DevCL.Database.Model;
using DevCL.Extensions.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DevCL.Controllers;

[ApiController]
[Route("collections/{id}/collaborators")]
public class CollaboratorController : ControllerBase {
    IMongoCollection<CLCollection> checklists;
    IMongoCollection<User> users;
    JwtSecurityTokenHandler tokenHandler;
    public CollaboratorController(MongoClient mongoClient, JwtSecurityTokenHandler handler) {
        checklists = mongoClient.GetDatabase("dev_cl").GetCollection<CLCollection>("collection");
        users = mongoClient.GetDatabase("dev_cl").GetCollection<User>("users");
        tokenHandler = handler;
    }

    [Authorize]
    [HttpPost]
    public ActionResult AddNewCollaborator(string id, [FromHeader] string authorization ,[FromBody] JsonDocument request) {
        try {
            string userId = tokenHandler.ExtractUserId(authorization);
            string alias = request.RootElement.GetProperty("alias").ToString();
            string email = request.RootElement.GetProperty("email").ToString();


            //only the owner of the collection can add collaborators
            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var document = checklists.Find(filter).First();

            if (document.Owner != userId) {
                return Unauthorized();
            }

            Console.WriteLine($"{alias}, {email}");

            var userFilter = Builders<User>.Filter.Eq(d => d.Email, email);

            var userDocument = users.Find(userFilter);

            Collaborator collaborator = new Collaborator() {
                Alias = alias,
                Email = email,
                Id = userDocument.Any() ? userDocument.First().Id.ToString() : ""
            };
            var update = Builders<CLCollection>.Update.Push(d => d.Collaborators, collaborator);

            var result = checklists.UpdateOne(filter, update);

            return Ok(new {
                alias,
                email
            });
        }
        catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }

    [Authorize]
    [HttpDelete]
    public ActionResult RemoveCollaborator(string id, [FromHeader] string authorization, [FromBody] JsonDocument request) {
        try {
            string userId = tokenHandler.ExtractUserId(authorization);
            string alias = request.RootElement.GetProperty("alias").ToString();

            var filter = Builders<CLCollection>.Filter.Eq(d => d.Id, id);
            var document = checklists.Find(filter).First();

            if (document.Owner != userId) {
                return Unauthorized();
            }

            var update = Builders<CLCollection>.Update.PullFilter(
                "collaborators",
                Builders<Collaborator>.Filter.Eq(d => d.Alias, alias)
            );

            checklists.UpdateOne(filter, update);

            return Ok();
        } catch (Exception) {
            return StatusCode(500, "An unexpected error occured");
        }
    }
}