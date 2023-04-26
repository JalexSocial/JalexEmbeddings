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

    public EmbeddingDocument ConvertMarkdownToEmbeddingDocument(string title, string filename, List<string> lines, int maxTokensPerParagraph = 350)
    {
        var paragraphs = SemanticTextPartitioner.SplitMarkdownParagraphs(lines, maxTokensPerParagraph);

        var doc = new EmbeddingDocument
        {
            Title = title,
            Filename = filename,
            Fragments = new()
        };

        for (int i = 0; i < paragraphs.Count; i++)
        {
            var text = paragraphs[i];
            if (i > 0) text = paragraphs[i - 1] + "\n" + text;
            if (i < paragraphs.Count - 1) text += "\n" + paragraphs[i + 1];

            var embedding = GetEmbeddings(text);

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
