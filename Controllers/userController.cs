using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using DevCL.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using System.Security.Claims;
using MongoDB.Bson;
using DevCL.Extensions.JWT;

[ApiController]
[Route("users")]
public class UserController : ControllerBase {
    UserService userService;

    public UserController(UserService userService) {
        this.userService = userService;
    }

    [HttpGet]
    public ActionResult GetUser([FromHeader] string authorization) {
        try {
            var user = userService.getUserById(authorization);

            return Ok(new {
                _id = user.Id,
                username = user.Username
            });
        }
        catch (Exception) {
            return StatusCode(500, "Something Went Wrong");
        }
    }

    [HttpPost("signin")]
    public ActionResult SignIn([FromBody] SignInUser user) {
        try {
            var document = userService.signInUser(user.Email, user.Password);
            return Ok(new {
                jwt = document.Jwt,
                _id = document.Id,
                username = document.Username
            });
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "Something Went Wrong");
        }
    }
    
    [HttpPost("signup")]
    public ActionResult SignUp([FromBody] SignUpUser user) {
        try {
            var document = userService.signUpUser(user);

            return Ok(new {
                _id = document.Id,
                username = document.Username
            });
        }
        catch(Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "Something Went Wrong");
        }
    }
}