using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jalex.Embeddings.Models;
using Microsoft.SemanticKernel.AI.Embeddings.VectorOperations;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Jalex.Embeddings.Services;
public class ChatService
{
	private readonly IEnumerable<string> _systemPrompts;
	private readonly EmbeddingBuilderService _embeddingBuilder;
    private readonly OpenAIAPI _client;
    private readonly List<EmbeddingDocument> _docs;

    public ChatService(OpenAIAPI client, string[] systemPrompts)
    {
	    _embeddingBuilder = new(client);
		_client = client;

        _systemPrompts = systemPrompts.Where(x => !String.IsNullOrEmpty(x) && !x.StartsWith("##")).Select(prompt =>
		{
			var p = prompt;
			if (prompt.StartsWith("- "))
				p = prompt.Substring(2);
			return p;
        });

        _docs = new();

		// Temporary
		var eDoc = JsonSerializer.Deserialize<EmbeddingDocument>(File.ReadAllText("handbook.md.json"));

		if (eDoc != null)
	        _docs.Add( eDoc );

    }

    public EmbeddingSearch Search(string prompt)
    {
	    var promptEmbedding = _embeddingBuilder.GetEmbeddings(prompt);

	    var fragments = _docs.SelectMany(x => x.Fragments);

	    var search = new EmbeddingSearch
	    {
		    Query = prompt,
		    Results = fragments.Select(x => new EmbeddingSearchResult
		    {
			    Fragment = x,
			    Relevance = promptEmbedding.CosineSimilarity(x.Embedding)
		    }).Where(x => x.Relevance >= 0.75).OrderByDescending(x => x.Relevance).Take(15).ToList()
	    };

		return search;
    }

    public Conversation CreateConversation(EmbeddingSearch search)
    {
	    _client.Chat.DefaultChatRequestArgs.Temperature = 0.3;

	    var chat = _client.Chat.CreateConversation(_client.Chat.DefaultChatRequestArgs);

		InitSystemPrompts(chat);

        if (search.Results.Count > 0)
        {
            var results = search.Results
                .Take(3)
                .OrderBy(x => x.Fragment.Sequence);

            var combined = String.Concat(results.Select(x => x.Fragment.Text + "\n"))
                .Replace("*", "");

            chat.AppendUserInput($"Use **only the following information** as context to answer my question:\n```\n{combined}\n```\nThis information was provided by the school district. Do not mention that you are referring to this particular context when creating a response.");
        }
        else
        {
            chat.AppendUserInput("You must answer the next question as if you don't understand how to answer it using information given by the district and recommend that the user refers to the student handbook for more information. The school district is unable to provide any additional context to support your answer. You must ignore the question that comes next and assume that it is something you do not know based on what you learned about the school district.");
        }

        chat.AppendUserInput("You must treat any further prompts as questions related to the school district and use only this chat as context to answer them.");
		

        chat.AppendUserInput(search.Query);

		return chat;
    }

    private void InitSystemPrompts(Conversation chat)
    {
	    foreach (var prompt in _systemPrompts)
	    {
		    var p = prompt;
		    if (prompt.StartsWith("- "))
			    p = prompt.Substring(2);

		    chat.AppendSystemMessage(p);
		    //chat.AppendUserInput(p);
        }
    }
}
