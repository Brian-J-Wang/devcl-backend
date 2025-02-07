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
    IMongoCollection<User> userCollection;
    JwtSecurityTokenHandler tokenHandler;
    string secret;

    public UserController(MongoClient mongoClient, JwtSecurityTokenHandler handler) {
        userCollection = mongoClient.GetDatabase("dev_cl").GetCollection<User>("users");
        tokenHandler = handler;
        secret = Env.GetString("JWT_SECRET");
    }

    [HttpGet]
    public ActionResult GetUser([FromHeader] string authorization) {
        try {
            string userId = tokenHandler.ExtractUserId(authorization);
                
            var filter = Builders<User>.Filter.Eq(d => d.Id, ObjectId.Parse(userId));
            var user = userCollection.Find(filter).First();

            return Ok(new {
                _id = user.Id.ToString(),
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
            var filter = Builders<User>.Filter.Eq(d => d.Email, user.Email);

            var result = userCollection.Find(filter);
            if (!result.Any()) {
                return BadRequest("Incorrect Email or Password");
            }

            var document = result.First();

            if (BCrypt.Net.BCrypt.Verify(user.Password, document.Password)) {
                return Ok(new {
                    jwt = GenerateJWT(document.Id.ToString(), document.Username),
                    _id = document.Id.ToString(),
                    username = document.Username
                });    
            } else {
                return BadRequest("Incorrect Email or Password");
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "Something Went Wrong");
        }
    }

    protected string GenerateJWT(string id, string username) {
        var claims = new List<Claim> {
            new Claim("username", username),
            new Claim("id", id)
        };

        var jwtToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secret.ToArray())
                ),
                SecurityAlgorithms.HmacSha256Signature
            )
        );

        return tokenHandler.WriteToken(jwtToken);
    }

    [HttpPost("signup")]
    public ActionResult SignUp([FromBody] SignUpUser user) {
        try {

            var filter = Builders<User>.Filter.Eq(d => d.Email, user.Email);
            bool exists = userCollection.Find(filter).Any();

            if (exists) {
                throw new DocumentAlreadyExistsException("Email already exists");
            }

            var document = user.ToUser();

            userCollection.InsertOne(document);

            return Ok(new {
                _id = document.Id.ToString(),
                username = document.Username
            });
        }
        catch(DocumentAlreadyExistsException) {
            return BadRequest("Email already exists");
        }
        catch(Exception ex) {
            Console.WriteLine(ex);
            return StatusCode(500, "Something Went Wrong");
        }
    }
}