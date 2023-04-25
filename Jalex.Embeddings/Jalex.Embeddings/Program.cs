using Jalex.Embeddings;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using Microsoft.SemanticKernel.SemanticFunctions.Partitioning;

var builder = new ConfigurationBuilder()
	.AddUserSecrets(typeof(Secrets).Assembly);
var configurationRoot = builder.Build();

var secrets = configurationRoot.GetSection("OpenAI").Get<Secrets>();

if (secrets is null)
	throw new ArgumentException("Invalid secrets configuration");

var text = File.ReadAllLines("handbook.md").ToList();

var paragraphs = SemanticTextPartitioner.SplitMarkdownParagraphs(text, 330);

OpenAIAPI client = new OpenAIAPI(secrets.ApiKey);

var result = client.Embeddings.CreateEmbeddingAsync("The cat in the hat knows a lot about that").Result;
var data = result?.Data.FirstOrDefault();

if (data?.Embedding != null)
{
	var embedding = data.Embedding;

	if (embedding != null)
	{

	}
}
