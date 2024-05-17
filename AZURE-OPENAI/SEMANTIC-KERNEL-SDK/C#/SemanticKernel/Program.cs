//DEMO
// The following demo tries the following features for Semantic Kernel SDK:
// 1. Create a builder
// 2. Plugins 

//if package missing
// dotnet add package Microsoft.SemanticKernel.Plugins.Core --version 1.2.0-alpha
// dotnet add package Microsoft.SemanticKernel --version 1.2.0
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;

//Replace with your values
string yourDeploymentName = "gpt-4-unai";
string yourEndpoint = "https://ai-050-swedencentral-previews.openai.azure.com/";

//Loaded from GitHub Secrets (env variables)
string yourKey = Environment.GetEnvironmentVariable("AOAI_SWEDEN_KEY");

var builder = Kernel.CreateBuilder();

builder.Services.AddAzureOpenAIChatCompletion(
    yourDeploymentName,
    yourEndpoint,
    yourKey);


//1. Build simple kernel and test it
var kernel = builder.Build();
var result = await kernel.InvokePromptAsync(
    "Give me a list of breakfast foods with eggs and cheese");
    
Console.WriteLine(result);

//2. Build a kernel with plugins: plugin=ConversationSummaryPlugin and function=GetConversationActionItems
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Plugins.AddFromType<ConversationSummaryPlugin>();
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernel = builder.Build();

string input = @"I'm a vegan in search of new recipes. 
I love spicy food! Can you give me a list of breakfast 
recipes that are vegan friendly?";

result = await kernel.InvokeAsync(
    "ConversationSummaryPlugin", 
    "GetConversationActionItems", 
    new() {{ "input", input }});

Console.WriteLine(result);