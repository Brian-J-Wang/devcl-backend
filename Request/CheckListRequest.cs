using System.Text.Json.Serialization;
using DevCL.Database.Model;

namespace DevCL.Requests;

public class CheckListPatchRequest : PatchRequest<CLCollection> {
    [JsonPropertyName("name")]
    public string? checklistName { get; set; }
}