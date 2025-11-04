using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class User {
    [BsonId, BsonRepresentation(BsonType.ObjectId), JsonPropertyName("_id")]
    public required ObjectId Id { get; set; }

    [BsonRequired, BsonElement("email"), JsonPropertyName("email")]
    public required string Email { get; set; }

    [BsonRequired, BsonElement("username"), JsonPropertyName("username")]
    public required string Username { get; set; }

    [BsonRequired, BsonElement("password"), JsonPropertyName("password")]
    public required string Password { get; set; }
}

public class SignUpUser {
    [BsonRequired, BsonElement("email"), JsonPropertyName("email")]
    public required string Email { get; set; }

    [BsonRequired, BsonElement("username"), JsonPropertyName("username")]
    public required string Username { get; set; }

    [BsonRequired, BsonElement("password"), JsonPropertyName("password")]
    public required string Password { get; set; }

    public User ToUser() {
        return new User() {
            Id = ObjectId.GenerateNewId(),
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