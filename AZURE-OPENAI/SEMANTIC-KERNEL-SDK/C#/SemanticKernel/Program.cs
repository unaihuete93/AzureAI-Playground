//DEMO
/* The following demo tries the following features for Semantic Kernel SDK:
 1. Create a builder
 2. Plugins 
 3. Prompt templates - Create a function from a prompt
 */


//if package missing
// dotnet add package Microsoft.SemanticKernel.Plugins.Core --version 1.2.0-alpha
// dotnet add package Microsoft.SemanticKernel --version 1.2.0
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using System;
using System.Threading.Tasks;

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

//add example plugin for demos  and build kernel

//disable warning- preview feature
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Plugins.AddFromType<ConversationSummaryPlugin>();
#pragma warning restore SKEXP0050
var kernel = builder.Build();

string demoNumber;

do
{
    Console.WriteLine("\n");
    Console.WriteLine("Enter the number of the demo you want to run, or 'exit' to quit:");
    Console.WriteLine("1. Demo 1: Simple kernel call");
    Console.WriteLine("2. Demo 2: Kernel with plugins- ConversationSummaryPlugin - GetConversationActionItems");
    Console.WriteLine("3. Demo 3: Prompt templates - using summarize plugin");
    Console.WriteLine("4. Demo 4: Prompt templates - Simple");

    demoNumber = Console.ReadLine();

    switch (demoNumber)
    {
        case "1":
            RunDemo1(kernel).GetAwaiter().GetResult();;
            break;
        case "2":
            RunDemo2(kernel).GetAwaiter().GetResult();;
            break;
        case "3":
            RunDemo3(kernel).GetAwaiter().GetResult();;
            break;
        // Add more cases as needed
        case "4":
            RunDemo4(kernel).GetAwaiter().GetResult();;
            break;
        case "exit":
            Console.WriteLine("Exiting program");
            break;
        default:
            Console.WriteLine("Invalid demo number");
            break;
    }
} while (demoNumber != "exit");

/*1. Build simple kernel and test it*/
async Task RunDemo1(Kernel kernel)
{
    Console.WriteLine("\n DEMO 1 \n");
    // Code for demo 1
    var result = await kernel.InvokePromptAsync(
        "Give me a list of breakfast foods with eggs and cheese");
    Console.WriteLine(result);
}

/*2. Build a kernel with plugins: plugin=ConversationSummaryPlugin and function=GetConversationActionItems*/
async Task RunDemo2(Kernel kernel)
{
    Console.WriteLine("\n DEMO 2 \n");

    string input = @"I'm a vegan in search of new recipes. 
    I love spicy food! Can you give me a list of breakfast 
    recipes that are vegan friendly?";

    var result = await kernel.InvokeAsync(
        "ConversationSummaryPlugin", 
        "GetConversationActionItems", 
        new() {{ "input", input }});

    Console.WriteLine(result);
}
/*3. Creating Prompt templates using kernel.CreateFunctionFromPrompt*/
async Task RunDemo3(Kernel kernel)
{
    Console.WriteLine("\n DEMO 3 \n");
    Console.WriteLine("\n DEMO 3.1 \n");

    string history = @"In the heart of my bustling kitchen, I have embraced 
        the challenge of satisfying my family's diverse taste buds and 
        navigating their unique tastes. With a mix of picky eaters and 
        allergies, my culinary journey revolves around exploring a plethora 
        of vegetarian recipes.

        One of my kids is a picky eater with an aversion to anything green, 
        while another has a peanut allergy that adds an extra layer of complexity 
        to meal planning. Armed with creativity and a passion for wholesome 
        cooking, I've embarked on a flavorful adventure, discovering plant-based 
        dishes that not only please the picky palates but are also heathy and 
        delicious.";

    //Prompt template - using summarize plugin 
    string functionPrompt = @"User background: 
        {{ConversationSummaryPlugin.SummarizeConversation $history}}
        Given this user's background, provide a list of relevant recipes.";

    var suggestRecipes = kernel.CreateFunctionFromPrompt(functionPrompt);
    var result = await kernel.InvokeAsync(suggestRecipes, 
        new KernelArguments() {
            { "history", history }
        });
    Console.WriteLine(result);

}

/*4 - Prompt Template - Simple */
async Task RunDemo4(Kernel kernel)
{
    Console.WriteLine("\n DEMO 4 \n");
    string language = "French";
    string history = @"I'm traveling with my kids and one of them 
        has a peanut allergy.";

    string prompt = @$"Consider the traveler's background:
        ${history}

        Create a list of helpful phrases and words in 
        ${language} a traveler would find useful.

        Group phrases by category. Include common direction 
        words. Display the phrases in the following format: 
        Hello - Ciao [chow]";

    var result = await kernel.InvokePromptAsync(prompt);
    Console.WriteLine(result);

}



