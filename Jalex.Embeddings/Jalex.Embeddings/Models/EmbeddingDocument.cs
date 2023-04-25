using System.Text.Json.Serialization;

namespace Jalex.Embeddings.Models;

public class EmbeddingDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = String.Empty;
    [JsonPropertyName("fragments")]
    public List<EmbeddingDocumentFragment> Fragments { get; set; } = new();
}
