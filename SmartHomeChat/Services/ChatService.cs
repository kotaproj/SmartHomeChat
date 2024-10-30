using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Kernel = Microsoft.SemanticKernel.Kernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

using SmartHomeChat.Plugins;

namespace SmartHomeChat.Services;

#pragma warning disable SKEXP0001

public class ChatService
{
    private Kernel _kernel;
    public ChatHistory? chatHistory;

    public ChatService(IConfiguration configuration)
    {
        string? apiKey = configuration["API_KEY"];
        string? modelName = configuration["MODEL_NAME"];
        string? endPoint = configuration["ENDPOINT"];
        if (apiKey == null || modelName == null || endPoint == null)
        {
            throw new ArgumentNullException("API_KEY/MODEL_NAME/ENDPOINT is not configured.");
        }

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(modelName, endPoint, apiKey);
        _kernel = builder.Build();

        // Add a plugin (the LightsPlugin class is defined below)
        _kernel.Plugins.AddFromType<LightsPlugin>("Lights");
        _kernel.Plugins.AddFromType<AirConsPlugin>("AirCons");

        chatHistory = new ChatHistory();
    }



    public async Task<string> Ask(string message)
    {
        // Enable planning
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // Create a history store the conversation
        var history = new ChatHistory();
        history.AddUserMessage(message);

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        // Get the response from the AI
        var result = await chatCompletionService.GetChatMessageContentAsync(
           history,
           executionSettings: openAIPromptExecutionSettings,
           kernel: _kernel);

        // Print the results
        Console.WriteLine("Assistant > " + result);

        // Add the message from the agent to the chat history
        history.AddAssistantMessage(result.ToString());

        return result.ToString();
    }
}

