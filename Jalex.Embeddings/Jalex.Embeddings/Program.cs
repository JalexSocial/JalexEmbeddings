using Jalex.Embeddings;
using Jalex.Embeddings.Services;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using Microsoft.SemanticKernel.SemanticFunctions.Partitioning;
using OpenAI_API.Completions;
using System.Text.Json;

var builder = new ConfigurationBuilder()
	.AddUserSecrets(typeof(Secrets).Assembly);
var configurationRoot = builder.Build();

var secrets = configurationRoot.GetSection("OpenAI").Get<Secrets>();

if (secrets is null)
	throw new ArgumentException("Invalid secrets configuration");

var filename = "handbook.md";
var text = File.ReadAllLines(filename).ToList();

OpenAIAPI client = new OpenAIAPI(secrets.ApiKey);

EmbeddingBuilderService embeddingBuilder = new EmbeddingBuilderService(client);

var doc = embeddingBuilder.ConvertMarkdownToEmbeddingDocument("EAHS 2022–2023 Student–Parent Handbook", filename, text, 500);

string jsonString = JsonSerializer.Serialize(doc, new JsonSerializerOptions(JsonSerializerDefaults.Web));

File.WriteAllText($"{filename}.json", jsonString);