using System.IdentityModel.Tokens.Jwt;

namespace DevCL.Extensions.JWT;

public static class JWTExtensions {
    public static string ExtractUserId(this JwtSecurityTokenHandler handler, string authString) {
        //auth string is Bearer {jwt}
        //gets the second half of Bearer {jwt}
        string token = authString.Split(" ")[1];

        if (!handler.CanReadToken(token)) {
            throw new Exception("Poorly formed token");
        }

        var jwt = handler.ReadJwtToken(token);
        string id = jwt.Claims.First((claims) => {
            return claims.Type == "id";
        }).Value;

        return id;
    }
}