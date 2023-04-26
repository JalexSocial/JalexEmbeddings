using Jalex.Embeddings;
using Jalex.Embeddings.Services;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using System.Text.Json;
using Jalex.Embeddings.Models;
using Microsoft.SemanticKernel.AI.Embeddings.VectorOperations;

var builder = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Secrets).Assembly);
var configurationRoot = builder.Build();

var secrets = configurationRoot.GetSection("OpenAI").Get<Secrets>();

if (secrets is null)
    throw new ArgumentException("Invalid secrets configuration");

var filename = "handbook.md";
var jsonFilename = $"{filename}.json";
var text = File.ReadAllLines(filename).ToList();

OpenAIAPI client = new OpenAIAPI(secrets.ApiKey);
EmbeddingBuilderService embeddingBuilder = new EmbeddingBuilderService(client);

if (!File.Exists(jsonFilename))
{
	var doc = embeddingBuilder.ConvertMarkdownToEmbeddingDocument("EAHS 2022–2023 Student–Parent Handbook", filename,
		text, 500);

	string jsonString = JsonSerializer.Serialize(doc, new JsonSerializerOptions(JsonSerializerDefaults.Web));

	File.WriteAllText(jsonFilename, jsonString);
}

var edoc = JsonSerializer.Deserialize<EmbeddingDocument>(File.ReadAllText(jsonFilename));

if (edoc is not null)
{
	while (true)
	{
		Console.Write("Ask a question: ");
		string prompt = Console.ReadLine()!;

		var promptEmbedding = embeddingBuilder.GetEmbeddings(prompt);

		var fragments =
			edoc.Fragments.OrderByDescending(x => promptEmbedding.CosineSimilarity(x.Embedding)).ToList();

		Console.ForegroundColor = ConsoleColor.DarkCyan;

		var results = fragments.Take(3);

		foreach (var result in results)
		{
			Console.WriteLine(result.Text);
		}

		Console.WriteLine("\n\n\n");
		Console.ResetColor();
	}
}