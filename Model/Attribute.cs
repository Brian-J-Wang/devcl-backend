using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using DevCL.Model;

//Attributes are values that a task may or may not have, while values inherent to Tasks are values that it will always have.
[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(EnumAttribute), typeof(IntegerAttribute), typeof(UserAttribute))]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EnumAttribute), "enum")]
public abstract class Attribute {
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public required string Name { get; set; }
    public Boolean Shared { get; set; } = false;
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentTaskDoc { get; set; }

    public abstract bool isValidAttribute(TaskAttribute attribute);
}

public class EnumAttribute : Attribute {
    public List<string> ValidValues { get; set; } = new List<string>();

    public override bool isValidAttribute(TaskAttribute attribute) {
        if (attribute.Value.ValueKind != System.Text.Json.JsonValueKind.String) {
            return false;
        }

        string value = attribute.Value.GetString()!;
        if (value == null) {
            return false;
        }

        if (ValidValues.Contains(value)) {
            return true;
        } else {
            return false;
        }
    }   
}

public class IntegerAttribute : Attribute {
    public int minValue { get; set; }
    public int maxValue { get; set; }

    public override bool isValidAttribute(TaskAttribute attribute) {
        throw new NotImplementedException();
    }
}

public class UserAttribute : Attribute {
    public override bool isValidAttribute(TaskAttribute attribute) {
        throw new NotImplementedException();
    }
}
