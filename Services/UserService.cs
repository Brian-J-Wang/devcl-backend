using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevCL.Extensions.JWT;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

public record UserDto(string Id, string Username);
public record UserSignIn(string Jwt, string Id, string Username);
public record UserSignUp(string Id, string Username);

public class UserService {
    readonly IMongoCollection<User> users;
    readonly JwtSecurityTokenHandler jwtService;
    readonly string secret;

    public UserService(MongoDbContext context, JwtSecurityTokenHandler handler) {
        users = context.Users;
        jwtService = handler;
        secret = Env.GetString("JWT_SECRET");
    }

    public UserDto getUserById(string authToken) {
        string userId = jwtService.ExtractUserId(authToken);
        var docs = users.Find(user => user.Id == userId).FirstOrDefault() ?? throw new KeyNotFoundException();
        return new UserDto(docs.Id, docs.Username);
    }

    public UserSignIn signInUser(string email, string password) {
        var user = users.Find(user => user.Email == email).FirstOrDefault();

        if (user == null) {
            throw new Exception("user not found");
        }

        if (BCrypt.Net.BCrypt.Verify(password, user.Password)) {
            return new UserSignIn(GenerateJWT(user.Id, user.Username), user.Id, user.Username);
        }
        else {
            throw new Exception();
        }
    }
    
    public UserSignUp signUpUser(SignUpUser signUpUser) {
        bool userExists = users.Find(user => user.Email == signUpUser.Email).Any();

        if (userExists) {
            throw new Exception();
        }

        var document = signUpUser.ToUser();
        users.InsertOne(document);

        return new UserSignUp(document.Id, document.Username);
    }
    
    private string GenerateJWT(string id, string username) {
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

        return jwtService.WriteToken(jwtToken);
    }
}