using System.IdentityModel.Tokens.Jwt;
using DevCL.Extensions.JWT;
using DevCL.Database.Model;
using MongoDB.Driver;
using DevCL.Controllers;

public class CollectionService {
    readonly IMongoCollection<TasksDoc> collection;
    readonly JwtSecurityTokenHandler jwtService;
    public CollectionService(MongoDbContext context, JwtSecurityTokenHandler jwt) {
        collection = context.Collection;
        jwtService = jwt;
    }

    public TasksDoc GetDocument(string docuId) {
        var doc = collection.Find(doc => doc.Id == docuId).FirstOrDefault() ?? throw new KeyNotFoundException($"document with id:{docuId} not found");
        return doc;
    }
    
    public List<TasksDoc> GetDocumentsByUser(string authToken) {
        string userId = jwtService.ExtractUserId(authToken);
        return collection.Find(doc => doc.Owner == userId).ToList();
    }

    public TasksDoc AddNewDocument(string authToken, NewCollectionRequest request) {
        string userId = jwtService.ExtractUserId(authToken);
        TasksDoc document = new TasksDoc() {
            Owner = userId,
            Name = request.Title,
        };
        collection.InsertOne(document);

        return document;
    }

    public void DeleteDocument(string authToken, string docuId) {
        string userId = jwtService.ExtractUserId(authToken);
        collection.DeleteOne(doc => doc.Id == docuId && doc.Owner == userId);
    }

    public void updateDocumentVersion(PatchType type) {
        
    }
}