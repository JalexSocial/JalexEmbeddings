﻿using Jalex.Embeddings;
using Jalex.Embeddings.Services;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using System.Text.Json;
using Jalex.Embeddings.Models;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings.VectorOperations;
using OpenAI_API.Chat;

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
		text, 150);

	string jsonString = JsonSerializer.Serialize(doc, new JsonSerializerOptions(JsonSerializerDefaults.Web));

	File.WriteAllText(jsonFilename, jsonString);
}

var edoc = JsonSerializer.Deserialize<EmbeddingDocument>(File.ReadAllText(jsonFilename));

if (edoc is not null)
{
    client.Chat.DefaultChatRequestArgs.Temperature = 0.2;
    var chat = client.Chat.CreateConversation(client.Chat.DefaultChatRequestArgs);
	chat.AppendSystemMessage(File.ReadAllText("systemprompt.txt"));
	
	while (true)
	{
		Console.Write("Ask a question: ");
		string prompt = Console.ReadLine()!;

		var promptEmbedding = embeddingBuilder.GetEmbeddings(prompt);

		var fragments =
			edoc.Fragments.OrderByDescending(x => promptEmbedding.CosineSimilarity(x.Embedding)).ToList();

		Console.ForegroundColor = ConsoleColor.DarkCyan;

		var results = fragments.Take(3).OrderBy(x => x.Sequence);

        var combined = String.Concat(results.Select(x => x.Text + "\n")).Replace("*","");

        chat.AppendUserInput($"Use the following information as context to answer my question:\n```\n{combined}\n```\n");
        chat.AppendUserInput(prompt);

		/*
        foreach (var result in results)
		{
			Console.WriteLine(result.Text);
		}
		**/
        await foreach (var message in chat.StreamResponseEnumerableFromChatbotAsync())
        {
            Console.Write(message);
        }

        Console.WriteLine("\n\n\n");
		Console.ResetColor();
	}
}