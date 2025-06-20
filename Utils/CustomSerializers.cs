using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

public class ObjectDictionarySerializer : IBsonSerializer<Dictionary<string, object>>
{
    public Type ValueType => typeof(Dictionary<string, object>);

    public Dictionary<string, object> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(context.Reader);

        Dictionary<string, object> result = new Dictionary<string, object>();

        foreach (var element in bsonDocument.Elements)
        {
            result[element.Name] = BsonTypeMapper.MapToDotNetValue(element.Value);
        }

        return result;
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Dictionary<string, object> value)
    {
        var bsonDocument = new BsonDocument();

        foreach (var kvp in value)
        {
            bsonDocument.Add(new BsonElement(kvp.Key, BsonValue.Create(kvp.Value)));
        }

        BsonSerializer.Serialize(context.Writer, bsonDocument);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        var bsonDocument = new BsonDocument();

        foreach (var kvp in value as Dictionary<string, object> ?? new Dictionary<string, object>())
        {
            dynamic bsonValue = kvp.Value;
            if (kvp.Value.GetType() == typeof(JsonElement))
            {
                bsonValue = ConvertJsonElementToBsonValue((JsonElement)kvp.Value);
            }
            bsonDocument.Add(new BsonElement(kvp.Key, BsonValue.Create(bsonValue)));
        }

        BsonSerializer.Serialize(context.Writer, bsonDocument);
    }

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return Deserialize(context, args);
    }

    private BsonValue ConvertJsonElementToBsonValue(JsonElement jsonElement)
    {
        // Depending on the type of JsonElement, convert it to the appropriate BSON type
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                return BsonDocument.Parse(jsonElement.ToString()); // Convert to BsonDocument
            case JsonValueKind.Array:
                return BsonArray.Create(jsonElement.ToString()); // Convert to BsonArray
            case JsonValueKind.String:
                return new BsonString(jsonElement.GetString());
            case JsonValueKind.Number:
                if (jsonElement.TryGetInt32(out int intValue))
                    return new BsonInt32(intValue);
                else if (jsonElement.TryGetDouble(out double doubleValue))
                    return new BsonDouble(doubleValue);
                else return new BsonDouble((double)jsonElement.GetDecimal()); // In case it's a decimal
            case JsonValueKind.True:
            case JsonValueKind.False:
                return new BsonBoolean(jsonElement.GetBoolean());
            case JsonValueKind.Null:
                return BsonNull.Value;
            default:
                throw new NotSupportedException($"Unsupported JsonValueKind: {jsonElement.ValueKind}");
        }
    }
}

public class JsonElementSerializer : IBsonSerializer<JsonElement>
{
    public Type ValueType => typeof(JsonElement);

    // Serialize the JsonElement to a BSON-compliant value
    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JsonElement value)
    {
        // Convert the JsonElement to BSON
        BsonValue bsonValue = ConvertJsonElementToBsonValue(value);
        BsonSerializer.Serialize(context.Writer, bsonValue);
    }

    // Deserialize the BSON value back into a JsonElement
    public JsonElement Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        BsonValue bsonValue = BsonSerializer.Deserialize<BsonValue>(context.Reader);
        string jsonString = bsonValue.ToString(); // Convert BSON back to JSON string
        return JsonSerializer.Deserialize<JsonElement>(jsonString); // Deserialize to JsonElement
    }

    private BsonValue ConvertJsonElementToBsonValue(JsonElement jsonElement)
    {

        Console.WriteLine("converitng");
        // Depending on the type of JsonElement, convert it to the appropriate BSON type
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                return BsonDocument.Parse(jsonElement.ToString()); // Convert to BsonDocument
            case JsonValueKind.Array:
                return BsonArray.Create(jsonElement.ToString()); // Convert to BsonArray
            case JsonValueKind.String:
                return new BsonString(jsonElement.GetString());
            case JsonValueKind.Number:
                if (jsonElement.TryGetInt32(out int intValue))
                    return new BsonInt32(intValue);
                else if (jsonElement.TryGetDouble(out double doubleValue))
                    return new BsonDouble(doubleValue);
                else return new BsonDouble((double) jsonElement.GetDecimal()); // In case it's a decimal
            case JsonValueKind.True:
            case JsonValueKind.False:
                return new BsonBoolean(jsonElement.GetBoolean());
            case JsonValueKind.Null:
                return BsonNull.Value;
            default:
                throw new NotSupportedException($"Unsupported JsonValueKind: {jsonElement.ValueKind}");
        }
    }

    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        throw new NotImplementedException();
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        throw new NotImplementedException();
    }
}