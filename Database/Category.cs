using System.Text.Json.Serialization;

public class NewCategory {
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("format")]
    public string Format { get; set; }
}

public class DeleteCategory {
    [JsonPropertyName("id")]
    public string Id { get; set; }
}

public class UpdateCategory {

}