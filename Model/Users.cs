using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class User {
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id")]
    public required string Id { get; set; }

    [BsonRequired]
    public required string Email { get; set; }

    [BsonRequired]
    public required string Username { get; set; }

    [BsonRequired]
    public required string Password { get; set; }
}

public class SignUpUser {
    [BsonRequired]
    public required string Email { get; set; }

    [BsonRequired]
    public required string Username { get; set; }

    [BsonRequired]
    public required string Password { get; set; }

    public User ToUser() {
        return new User() {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = Email,
            Username = Username,
            Password = BCrypt.Net.BCrypt.HashPassword(Password)
        };
    }
}

public class SignInUser {
    [BsonRequired, BsonElement("email"), JsonPropertyName("email")]
    public required string Email { get; set; }
    
    [BsonRequired, BsonElement("password"), JsonPropertyName("password")]
    public required string Password { get; set; }
}