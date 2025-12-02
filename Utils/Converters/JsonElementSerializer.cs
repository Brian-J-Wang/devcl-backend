using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace DevCL.Utils.Converters;

class JsonElementSerializer : IBsonSerializer<JsonElement> {
    public Type ValueType => typeof(JsonElement);

    public JsonElement Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
        var reader = context.Reader;
        switch(reader.GetCurrentBsonType()) {
            case BsonType.String:
                return JsonDocument.Parse(reader.ReadString()).RootElement;
        }

        throw new Exception();
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JsonElement value) {        
        Console.WriteLine(value.ValueKind);
        switch(value.ValueKind) {
            case JsonValueKind.Number:
                context.Writer.WriteInt32(value.GetInt32());
                break;
            case JsonValueKind.String:
                context.Writer.WriteString(value.GetString());
                break;
            case JsonValueKind.Object:
                
                BsonDocumentSerializer.Instance.Serialize(context, BsonDocument.Parse(value.GetRawText()));
                break;
            case JsonValueKind.Array:
                context.Writer.WriteStartArray();
                foreach (var item in value.EnumerateArray()) {
                    Serialize(context, args, item);
                }
                context.Writer.WriteEndArray();
                break;
            case JsonValueKind.Undefined: 
                throw new Exception();
        }
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value) {
        var obj = (JsonElement)value;
        Console.WriteLine("ObjSerialize");

        switch (obj.ValueKind) {
            case JsonValueKind.Number:
                context.Writer.WriteInt32(obj.GetInt32());
                break;
            case JsonValueKind.String:
                context.Writer.WriteString(obj.GetString());
                break;
            case JsonValueKind.Object:
                
                break;
            case JsonValueKind.Undefined: 
                throw new Exception();
        }
    }

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) {
        var reader = context.Reader;
        switch(reader.GetCurrentBsonType()) {
            case BsonType.String:
                return JsonDocument.Parse(JsonSerializer.Serialize(reader.ReadString())).RootElement;
            default:
                throw new Exception();
        }
    }
}