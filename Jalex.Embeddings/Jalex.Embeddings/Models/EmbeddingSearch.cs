using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jalex.Embeddings.Models;

public class EmbeddingSearch
{
    public string Query { get; set; } = string.Empty;
    public List<EmbeddingSearchResult> Results { get; set; } = new();
}
