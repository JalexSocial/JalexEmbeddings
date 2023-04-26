using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jalex.Embeddings.Models;
public class EmbeddingSearchResult
{
    public double Relevance { get; set; }
    public EmbeddingDocumentFragment Fragment { get; set; } = new();
}
