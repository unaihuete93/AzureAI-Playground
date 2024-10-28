// Import packages
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
//Entra ID auth
using System;
using Azure.Identity;


// Define the model ID, endpoint, and API key for Azure OpenAI chat completion
string yourDeploymentName = "gpt-4-unai";
string yourEndpoint = Environment.GetEnvironmentVariable("AOAI_SWEDEN_END");
//string yourKey = Environment.GetEnvironmentVariable("AOAI_SWEDEN_KEY");


// Create a kernel with Azure OpenAI chat completion
//var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(yourDeploymentName, endpoint, apiKey);

//With Service Principal/MI
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(yourDeploymentName, yourEndpoint, new DefaultAzureCredential());


// Add enterprise components
//builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add a plugin (the LightsPlugin class is defined below)
//kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() 
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do {
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
