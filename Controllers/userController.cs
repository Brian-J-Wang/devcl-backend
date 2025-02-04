using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("user")]
public class UserController : ControllerBase {
    IMongoCollection<User> userCollection;

    public UserController(MongoClient mongoClient) {
        userCollection = mongoClient.GetDatabase("dev_cl").GetCollection<User>("users");
    }

    [HttpPost("/signin")]
    public ActionResult SignIn([FromBody] SignInUser user) {
        Console.WriteLine(user);

        return Ok();
    }

    [HttpPost("/signup")]
    public ActionResult SignUp([FromBody] SignUpUser user) {
        Console.WriteLine(user);

        return Ok();
    }
}