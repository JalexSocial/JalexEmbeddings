using Jalex.Embeddings.Models;
using Microsoft.SemanticKernel.SemanticFunctions.Partitioning;
using OpenAI_API;

namespace Jalex.Embeddings.Services;

public class EmbeddingBuilderService
{
    private readonly OpenAIAPI _client;

    public EmbeddingBuilderService(OpenAIAPI client)
    {
        _client = client;
    }

    public EmbeddingDocument ConvertMarkdownToEmbeddingDocument(string title, string filename, List<string> lines, int maxTokensPerParagraph = 500)
    {
        var paragraphs = SemanticTextPartitioner.SplitMarkdownParagraphs(lines, 500);

        var doc = new EmbeddingDocument
        {
            Title = title,
            Filename = filename,
            Fragments = new()
        };

        for (int i = 0; i < paragraphs.Count; i++)
        {
            var embedding = GetEmbeddings(paragraphs[i]);

            if (embedding.Length > 0)
            {
                doc.Fragments.Add(new EmbeddingDocumentFragment
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = doc.Id,
                    Sequence = i + 1,
                    Text = paragraphs[i],
                    Embedding = embedding
                });
            }

        }

        return doc;
    }

    public float[] GetEmbeddings(string text)
    {
        return _client.Embeddings.GetEmbeddingsAsync(text).Result ?? Array.Empty<float>();
    }
}
