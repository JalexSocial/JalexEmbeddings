using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Jalex.Embeddings.Models;

public class EmbeddingDocumentFragment
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = Guid.NewGuid().ToString();
	[JsonPropertyName("documentId")]
	public string DocumentId { get; set; } = string.Empty;
    /// <summary>
    /// Order that this document fragment appears in the document
    /// </summary>
    [JsonPropertyName("sequence")]
	public int Sequence { get; set; } 
    [JsonPropertyName("text")]
	public string Text { get; set; } = string.Empty;
	[JsonPropertyName("filename")]
	public float[] Embedding { get; set; } = Array.Empty<float>();
}
