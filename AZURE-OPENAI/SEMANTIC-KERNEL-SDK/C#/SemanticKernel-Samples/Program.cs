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

//missing in MS Learn https://learn.microsoft.com/en-us/training/modules/create-plugins-semantic-kernel/8-exercise-save-prompts-files
using Microsoft.SemanticKernel.ChatCompletion;

using System;
using System.Threading.Tasks;

//demo 10
using Microsoft.SemanticKernel.Planning.Handlebars;

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
    Console.WriteLine("5. Demo 5: Prompt templates - Few Shot Learning");
    /* Setting up prompt configuration in separated files*/
    Console.WriteLine("6. Demo 6: Using Prompt Files");
    Console.WriteLine("7. Demo 7: Using Prompt Files - Multiple Plugins");
    /*Create Native Function*/
    Console.WriteLine("8. Demo 8: Using Native Function");
    Console.WriteLine ("9. Using both Promtps and Functions");
    /*Mixing all with Planner*/
    Console.WriteLine("10. Using Planner with Prompts and Functions"); //Planner is similar to the function calling or tools in OpenAI

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
        case "4":
            RunDemo4(kernel).GetAwaiter().GetResult();;
            break;
        case "5":
            RunDemo5(kernel).GetAwaiter().GetResult();;
            break;
        case "6":
            RunDemo6(kernel).GetAwaiter().GetResult();;
            break;
        case "7":
            RunDemo7(kernel).GetAwaiter().GetResult();;
            break;
        case "8":
            RunDemo8(kernel).GetAwaiter().GetResult();;
            break;
        case "9":
            RunDemo9(kernel).GetAwaiter().GetResult();;
            break;
        case "10":
            RunDemo10(kernel).GetAwaiter().GetResult();;
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

/*2. Build a kernel with plugins: plugin=ConversationSummaryPlugin and function=GetConversationActionItems (included in the ConversationSummary built-in plugin)*/
async Task RunDemo2(Kernel kernel)
{
    Console.WriteLine("\n DEMO 2 \n");

    string input = @"I'm a vegan in search of new recipes. 
    I love spicy food! Please give me a list of breakfast 
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

    string prompt = @$"You are a travel assistant. You are helpful, creative, 
    and very friendly. Consider the traveler's background:
    ${history}

    Create a list of helpful phrases and words in 
    ${language} a traveler would find useful.

    Group phrases by category. Include common direction 
    words. Display the phrases in the following format: 
    Hello - Ciao [chow]

    Begin with: 'Here are some phrases in ${language} 
    you may find helpful:' 
    and end with: 'I hope this helps you on your trip!'";

    var result = await kernel.InvokePromptAsync(prompt);
    Console.WriteLine(result);

}

/*5 - Prompt Template - Few Shot Learning */
async Task RunDemo5(Kernel kernel)
{
    Console.WriteLine("\n DEMO 5 \n");
    string input = @"I have a vacation from June 1 to July 
        22. I want to go to Greece. I live in Chicago.";
    string prompt = @$"
        <message role=""system"">Instructions: Identify the 
        from and to destinations and dates from the user's 
        request</message>
        <message role=""user"">Can you give me a list of 
        flights from Seattle to Tokyo? I want to travel 
        from March 11 to March 18.</message>
        <message role=""assistant"">
        Seattle|Tokyo|03/11/2024|03/18/2024
        </message>

        <message role=""user"">${input}</message>";


    var result = await kernel.InvokePromptAsync(prompt);
    Console.WriteLine(result);
}

/*6 - Using Prompt Files */
async Task RunDemo6(Kernel kernel)
{
    var prompts = kernel.CreatePluginFromPromptDirectory("Prompts");
    /*This object represents a collection of functions. CreatePluginFromPromptDirectory accepts the path of a plugin directory, and each subdirectory's name is used as the function name. 
    For example, if you nested 'SuggestChords' inside a folder called 'ChordProgressions,' you would use the prompt directory 'Prompts/ChordProgressions' and the function name would stay the same.*/
    string input = "G, C";

    var result = await kernel.InvokeAsync(
        prompts["SuggestChords"],
        new() {
            { "startingChords", input },
        }
    );
    Console.WriteLine(result);
}

/*7 - Using Prompt Files - Multiple Plugins */
async Task RunDemo7(Kernel kernel)
{
    //Done at start
    //kernel.ImportPluginFromType<ConversationSummaryPlugin>();

    var prompts = kernel.ImportPluginFromPromptDirectory("Prompts/TravelPlugins");
    ChatHistory history = [];
    string input = @"I'm planning an anniversary trip with my 
        spouse. We like hiking, mountains, and beaches. Our 
        travel budget is $15000";

    var result = await kernel.InvokeAsync<string>(prompts["SuggestDestinations"],
        new() {
            { "input", input },
        }
    );

    Console.WriteLine(result);
    history.AddUserMessage(input);
    history.AddAssistantMessage(result);


}

/*8 - Using Native Function */
async Task RunDemo8(Kernel kernel)
{
    kernel.ImportPluginFromType<MusicLibraryPlugin>();

    var result = await kernel.InvokeAsync(
        "MusicLibraryPlugin", 
        "AddToRecentlyPlayed", 
        new() {
            ["artist"] = "Tiara", 
            ["song"] = "Danse", 
            ["genre"] = "French pop, electropop, pop"
        }
);
}

/*9 - Using both Prompts and Functions */
async Task RunDemo9(Kernel kernel)
{
    kernel.ImportPluginFromType<MusicLibraryPlugin>();

    string prompt = @"This is a list of music available to the user:
        {{MusicLibraryPlugin.GetMusicLibrary}} 

        This is a list of music the user has recently played:
        {{MusicLibraryPlugin.GetRecentPlays}}

        Based on their recently played music, suggest a song from
        the list to play next";

    var result = await kernel.InvokePromptAsync(prompt);
    Console.WriteLine(result);
}

/*10 - Using Planner with Prompts and Functions */
async Task RunDemo10(Kernel kernel)
{
    kernel.ImportPluginFromType<MusicLibraryPlugin>();
    kernel.ImportPluginFromType<MusicConcertPlugin>();
    kernel.ImportPluginFromPromptDirectory("Prompts");

    #pragma warning disable SKEXP0060 //ignore warnings for preview

    var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = true });

    string location = "Redmond WA USA";
    string goal = @$"Based on the user's recently played music, suggest a 
        concert for the user living in ${location}";

    var plan = await planner.CreatePlanAsync(kernel, goal);
    var result = await plan.InvokeAsync(kernel);

    Console.WriteLine($"Results: {result}");

    #pragma warning restore SKEXP0060
    
    
}

