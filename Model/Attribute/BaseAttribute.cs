using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace DevCL.Model;

//Attributes are values that a task may or may not have, while values inherent to Tasks are values that it will always have.
[BsonIgnoreExtraElements]
[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(EnumAttribute), typeof(IntegerAttribute), typeof(UserAttribute))]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EnumAttribute), "enum")]
public abstract class BaseAttribute {
    [BsonId, BsonElement("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public required string Name { get; set; }
    public required string PrimaryColor { get; set; } = "#F00";
    [JsonPropertyName("config")]
    public AttributeConfig AttributeConfig { get; set; } = new AttributeConfig();
    public abstract bool isValidAttributeValue(TaskAttribute attribute);
}

public class AttributeConfig {
    public ColoringMode coloringMode { get; set; } = ColoringMode.primary;
    public bool showAttributeName { get; set; } = false;
}

public enum ColoringMode {
    primary,
    secondary,
    both
}

public class UserAttribute : BaseAttribute {
    public override bool isValidAttributeValue(TaskAttribute attribute) {
        throw new NotImplementedException();
    }
}
