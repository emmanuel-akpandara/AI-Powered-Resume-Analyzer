using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;

public class OpenAIAnalyzer
{
    private readonly OpenAIClient _client;

    public OpenAIAnalyzer(IConfiguration config)
    {
        _client = new OpenAIClient(config["OpenAI:Key"]);
    }

    public async Task<string> AnalyzeResumeAsync(string resumeText)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("You are a resume parsing assistant. Return structured JSON with: skills, education, experience, certifications, summary."),
            ChatMessage.CreateUserMessage(resumeText)
        };

        var options = new ChatCompletionOptions()
        {
            Temperature = 0.2f,
            MaxOutputTokenCount = 2000
        };

        // Get a ChatClient from the OpenAIClient
        var chatClient = _client.GetChatClient("gpt-3.5-turbo");

        var response = await chatClient.CompleteChatAsync(messages, options);

        return response.Value.Content[0].Text;
    }
}