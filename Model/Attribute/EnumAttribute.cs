
using MongoDB.Bson;

namespace DevCL.Model;

public class EnumAttribute : BaseAttribute {
    public List<EnumAttributeValue> ValidValues { get; set; } = new List<EnumAttributeValue>();
    public override bool isValidAttributeValue(TaskAttribute attribute) {
        if (attribute.Value.ValueKind != System.Text.Json.JsonValueKind.String) {
            return false;
        }

        string valueName = attribute.Value.GetString()!;
        if (valueName == null) {
            return false;
        }

        return ValidValues.Find(att => att.Id != new ObjectId(valueName)) != null; 
    }
}

public class EnumAttributeValue {
    public required ObjectId Id { get; set; }
    public required string Name { get; set; }
    public string? SecondaryColor { get; set; }
}

 