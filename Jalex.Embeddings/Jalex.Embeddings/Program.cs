using Jalex.Embeddings;
using Jalex.Embeddings.Services;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using System.Text.Json;
using Jalex.Embeddings.Models;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings.VectorOperations;
using OpenAI_API.Chat;
using System;

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

ChatService chatService = new ChatService(client, File.ReadAllLines("systemprompt.txt"));

while (true)
{
	Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Ask a question: ");
	string prompt = Console.ReadLine()!;

    // List one thing I would need to look for in a student handbook to find the answer to the question "{prompt}" Please be very brief and state only the subject matter.
    //var result = await client.Chat.CreateChatCompletionAsync($"List what I would need to look for in a student handbook to find the answer to the prompt: \"{prompt}\". Please be brief and limit your response to at most 20 words.");

    //var alternatePrompt = result.Choices[0].Message.Content;

    var search = chatService.Search(prompt);

    var conversation = chatService.CreateConversation(search);

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    await foreach (var message in conversation.StreamResponseEnumerableFromChatbotAsync())
    {
	    Console.Write(message);
    }

    var totalTokens = conversation.Messages.Sum(x => x.Content.Length) / 4;
    var usage = conversation.MostResentAPIResult.Usage;

    Console.WriteLine("\n");
}

/*
var edoc = JsonSerializer.Deserialize<EmbeddingDocument>(File.ReadAllText(jsonFilename));

if (edoc is not null)
{
    client.Chat.DefaultChatRequestArgs.Temperature = 0.3;
    var chat = client.Chat.CreateConversation(client.Chat.DefaultChatRequestArgs);

    var prompts = File.ReadAllLines("systemprompt.txt").Where(x => !String.IsNullOrEmpty(x) && !x.StartsWith("##"));

    foreach (var prompt in prompts)
    {
        var p = prompt;
        if (prompt.StartsWith("- "))
            p = prompt.Substring(2);

        chat.AppendSystemMessage(p);
    }

    while (true)
	{
		Console.Write("Ask a question: ");
		string prompt = Console.ReadLine()!;

		var promptEmbedding = embeddingBuilder.GetEmbeddings(prompt);

        var search = new EmbeddingSearch
        {
            Query = prompt,
            Results = edoc.Fragments.Select(x => new EmbeddingSearchResult
            {
                Fragment = x,
                Relevance = promptEmbedding.CosineSimilarity(x.Embedding)
            }).OrderByDescending(x => x.Relevance).Take(15).ToList()
        };

        var fragments = search.Results.Select(x => x.Fragment);

		Console.ForegroundColor = ConsoleColor.DarkCyan;

		var results = fragments.Take(2).OrderBy(x => x.Sequence);

        var combined = String.Concat(results.Select(x => x.Text + "\n")).Replace("*","");

        chat.AppendUserInput($"Use the following information as context to answer my question:\n```\n{combined}\n```\n");
        chat.AppendUserInput(prompt);

        var totalTokens = chat.Messages.Sum(x => x.Content.Length) / 4;

        if (totalTokens > 4000)
        {
	        Console.WriteLine("End of chat");
	        break;
        }

        await foreach (var message in chat.StreamResponseEnumerableFromChatbotAsync())
        {
            Console.Write(message);
        }

        totalTokens = chat.Messages.Sum(x => x.Content.Length) / 4;

        if (totalTokens > 3000)
        {
            Console.WriteLine("End of chat");
            break;
        }

        Console.WriteLine("\n\n\n");
		Console.ResetColor();
	}
}
*/