using System.IdentityModel.Tokens.Jwt;
using DevCL.Extensions.JWT;
using DevCL.Database.Model;
using MongoDB.Driver;
using DevCL.Controllers;

public record GetUserProjects(string Id, string Name, string Owner, string Version); 

public class TaskDocService {
    readonly IMongoCollection<Project> collection;
    readonly JwtSecurityTokenHandler jwtService;
    public TaskDocService(MongoDbContext context, JwtSecurityTokenHandler jwt) {
        collection = context.Collection;
        jwtService = jwt;
    }

    public Project GetTaskDoc(string docuId) {
        var doc = collection.Find(doc => doc.Id == docuId).FirstOrDefault() ?? throw new KeyNotFoundException($"document with id:{docuId} not found");
        return doc;
    }

    public List<GetUserProjects> GetUserProjects(string authToken) {
        string userId = jwtService.ExtractUserId(authToken);
        return [.. collection.Find(doc => doc.Owner == userId).ToList().Select(taskDoc => new GetUserProjects(taskDoc.Id,
            taskDoc.Name,
            taskDoc.Owner,
            taskDoc.Version))];
    }

    public Project CreateNewProject(string authToken, NewProjectRequest request) {
        string userId = jwtService.ExtractUserId(authToken);
        Project project = new Project() {
            Owner = userId,
            Name = request.Name,
        };
        collection.InsertOne(project);

        return project;
    }

    public void DeleteProject(string authToken, string docuId) {
        string userId = jwtService.ExtractUserId(authToken);
        collection.DeleteOne(doc => doc.Id == docuId && doc.Owner == userId);
    }

    public void updateDocumentVersion(PatchType type) {

    }
}