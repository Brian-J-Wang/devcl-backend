using Microsoft.AspNetCore.Mvc;

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
            return Ok(user);
        }
        catch (Exception) {
            return StatusCode(500, "Something Went Wrong");
        }
    }

    [HttpPost("signin")]
    public ActionResult SignIn([FromBody] SignInUser user) {
        try {
            var document = userService.signInUser(user.Email, user.Password);
            return Ok(document);
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
            return Ok(document);
        }
        catch(Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "Something Went Wrong");
        }
    }
}