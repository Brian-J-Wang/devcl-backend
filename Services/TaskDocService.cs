using System.IdentityModel.Tokens.Jwt;
using DevCL.Extensions.JWT;
using DevCL.Database.Model;
using MongoDB.Driver;
using DevCL.Controllers;
using System.ComponentModel;

public record GetTaskDocByUserDTO(string Id, string Name, string Owner, string Version); 

public class TaskDocService {
    readonly IMongoCollection<TasksDoc> collection;
    readonly JwtSecurityTokenHandler jwtService;
    public TaskDocService(MongoDbContext context, JwtSecurityTokenHandler jwt) {
        collection = context.Collection;
        jwtService = jwt;
    }

    public TasksDoc GetTaskDoc(string docuId) {
        var doc = collection.Find(doc => doc.Id == docuId).FirstOrDefault() ?? throw new KeyNotFoundException($"document with id:{docuId} not found");
        return doc;
    }

    public List<GetTaskDocByUserDTO> GetTaskDocsByUser(string authToken) {
        string userId = jwtService.ExtractUserId(authToken);
        return [.. collection.Find(doc => doc.Owner == userId).ToList().Select(taskDoc => new GetTaskDocByUserDTO(taskDoc.Id,
            taskDoc.Name,
            taskDoc.Owner,
            taskDoc.Version))];
    }

    public TasksDoc AddNewTaskDoc(string authToken, NewCollectionRequest request) {
        string userId = jwtService.ExtractUserId(authToken);
        TasksDoc document = new TasksDoc() {
            Owner = userId,
            Name = request.Title,
        };
        collection.InsertOne(document);

        return document;
    }

    public void DeleteTaskDoc(string authToken, string docuId) {
        string userId = jwtService.ExtractUserId(authToken);
        collection.DeleteOne(doc => doc.Id == docuId && doc.Owner == userId);
    }

    public void updateDocumentVersion(PatchType type) {

    }
}