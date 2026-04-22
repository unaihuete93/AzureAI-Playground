// Import packages
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
//Entra ID auth
using System;
using Azure.Identity;
//logging
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System.Text.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

//handlebars
using System.IO;
using System.Reflection;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

public static class EmbeddedResource
{
    public static string Read(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}

class LoggingHttpClientHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(content),
                new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

class Program
{
    static async Task Main(string[] args)
    {

        // Define the model ID, endpoint, and API key for Azure OpenAI chat completion
        string yourDeploymentName = "gpt-4-unai";
        string yourEndpoint = Environment.GetEnvironmentVariable("AOAI_SWEDEN_END");
        //string yourKey = Environment.GetEnvironmentVariable("AOAI_SWEDEN_KEY");

        //Create a kernel with Azure OpenAI chat completion

        // AUTHENTICATE with key
        //var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(yourDeploymentName, endpoint, apiKey);

        //AUTHENTICATE With Service Principal/MI
        var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(yourDeploymentName, yourEndpoint, new DefaultAzureCredential());

        //with HTTPClient (COMMENT ONE OF THE TWO LINES)
        //var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(yourDeploymentName, yourEndpoint, new DefaultAzureCredential(), httpClient: new HttpClient(new LoggingHttpClientHandler()));
        
        //UNCOMMENT TO DEBUG LOCALLY
         builder.Services.AddLogging(c => c.AddDebug()
                                           .SetMinimumLevel(LogLevel.Trace) // don't do this in production!
                                           .AddConsole());

        // Build the kernel
        Kernel kernel = builder.Build();
        

        // Add a plugin
        kernel.Plugins.AddFromType<LightsPlugin>("Lights");

        // Add a prompt template from file (handlebars YAML)
        var handlebarsPromptYaml = File.ReadAllText("Prompts/handlebars-demo.yaml");
        var templateFactory = new HandlebarsPromptTemplateFactory();
        var function = kernel.CreateFunctionFromPromptYaml(handlebarsPromptYaml, templateFactory);


        //chat completion service 
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();



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
    }
}