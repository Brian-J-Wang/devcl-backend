using System.Text.Json.Nodes;
using DevCL.Model;
using MongoDB.Driver;

public class AttributeService {
    IMongoCollection<BaseAttribute> projectAttributes;
    IMongoCollection<BaseAttribute> templateAttributes;
    public AttributeService(MongoDbContext context) {
        projectAttributes = context.Attributes;
        templateAttributes = context.SharedAttributes;
    }

    public BaseAttribute AddSharedAttribute(BaseAttribute attribute) {
        var result = templateAttributes.FindOneAndReplace(
            Builders<BaseAttribute>.Filter.Eq(d => d.Id, attribute.Id),
            attribute,
            new FindOneAndReplaceOptions<BaseAttribute> { IsUpsert = true });

        return result;
    }

    public void ClearSharedCollection() {
        templateAttributes.DeleteMany(FilterDefinition<BaseAttribute>.Empty);
    }

    public IEnumerable<BaseAttribute> GetAttributes(string docuId) {
        var shared = templateAttributes.Find(FilterDefinition<BaseAttribute>.Empty).ToEnumerable();
        return shared;
    }

    public bool ValidateTaskAttribute(TaskAttribute taskAttribute) {
        var attributeDoc = templateAttributes.Find((attribute) => attribute.Id == taskAttribute.Id).FirstOrDefault();

        if (attributeDoc == null) {
            return false;
        }

        return attributeDoc.isValidAttributeValue(taskAttribute);
    }
}