using System.Text.Json.Nodes;
using DevCL.Model;
using MongoDB.Driver;

public class AttributeService {
    IMongoCollection<Attribute> userAttributes;
    IMongoCollection<Attribute> sharedAttribute;
    public AttributeService(MongoDbContext context) {
        userAttributes = context.Attributes;
        sharedAttribute = context.SharedAttributes;
    }

    public Attribute AddSharedAttribute(Attribute attribute) {
        var result = sharedAttribute.FindOneAndReplace(
            Builders<Attribute>.Filter.Eq(d => d.Id, attribute.Id),
            attribute,
            new FindOneAndReplaceOptions<Attribute> { IsUpsert = true });

        return result;
    }

    public void ClearSharedCollection() {
        sharedAttribute.DeleteMany(FilterDefinition<Attribute>.Empty);
    }

    public IEnumerable<Attribute> GetAttributes(string docuId) {
        var shared = sharedAttribute.Find(FilterDefinition<Attribute>.Empty).ToEnumerable();
        return shared;
    }

    public bool ValidateTaskAttribute(TaskAttribute taskAttribute) {
        var attributeDoc = sharedAttribute.Find((attribute) => attribute.Id == taskAttribute.Id).FirstOrDefault();

        if (attributeDoc == null) {
            return false;
        }

        return attributeDoc.isValidAttribute(taskAttribute);
    }
}