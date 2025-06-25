//dotnet add package Microsoft.SemanticKernel

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using System;

// string filePath = Path.GetFullPath("appsettings.json");
// var config = new ConfigurationBuilder()
//     .AddJsonFile(filePath)
//     .Build();

// Set your values in appsettings.json
// string apiKey = config["PROJECT_KEY"]!;
// string endpoint = config["PROJECT_ENDPOINT"]!;
// string deploymentName = config["DEPLOYMENT_NAME"]!;

string yourDeploymentName = "gpt-4-unai";
string yourEndpoint = Environment.GetEnvironmentVariable("AOAI_SWEDEN_END");
string yourKey = Environment.GetEnvironmentVariable("AOAI_SWEDEN_KEY");

// Create a kernel with Azure OpenAI chat completion
//var builder = Kernel.CreateBuilder();
//builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

// AUTHENTICATE with key
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(yourDeploymentName, yourEndpoint, yourKey);


var kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add the plugin to the kernel
kernel.Plugins.AddFromType<FlightBookingPlugin>("FlightBookingPlugin");

// Configure function choice behavior
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() 
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Configure function choice behavior
// KernelFunction searchFlights = kernel.Plugins.GetFunction("FlightBookingPlugin", "search_flights");

// PromptExecutionSettings openAIPromptExecutionSettings = new() 
// {
//     FunctionChoiceBehavior = FunctionChoiceBehavior.Required(functions: [searchFlights]) 
// };

var history = new ChatHistory();
history.AddSystemMessage("The year is 2025 and the current month is January");

AddUserMessage("Find me a flight to Tokyo on the 19");
await GetReply();
GetInput();
await GetReply();


void GetInput() {
    Console.Write("User: ");
    string input = Console.ReadLine()!;
    history.AddUserMessage(input);
}

async Task GetReply() {
    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel
    );
    Console.WriteLine("Assistant: " + reply.ToString());
    history.AddAssistantMessage(reply.ToString());
}

void AddUserMessage(string msg) {
    Console.WriteLine("User: " + msg);
    history.AddUserMessage(msg);
}