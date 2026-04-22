
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using System.Text.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Azure.AI.OpenAI;


//HTTPClientHandler to see the HTTP request and response
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

//Main program
class Program
{
    static async Task Main(string[] args)
    {
        var modelDeploymentName = "gpt-4-unai";
        var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AOAI_SWEDEN_END");
        var azureOpenAIApiKey = Environment.GetEnvironmentVariable("AOAI_SWEDEN_KEY");

        var client = new HttpClient(new LoggingHttpClientHandler());

        var builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            modelDeploymentName,         // Azure OpenAI Deployment Name. Make sure this is correct
            azureOpenAIEndpoint,  // Azure OpenAI Endpoint
            azureOpenAIApiKey,
            httpClient: client);      // Azure OpenAI Key

        builder.Services.AddLogging(c => c.AddDebug()
                                  .SetMinimumLevel(LogLevel.Trace) // don't do this in production!
                                  .AddConsole());


        //builder.Plugins.AddFromType<MyMathPlugin>();

        var kernel = builder.Build();

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var demoNumber = "1";

        //Menu with different RAG implementations to test
        do
        {
            Console.WriteLine("\n");
            Console.WriteLine("Enter the number of the demo you want to run, or 'exit' to quit:");
            Console.WriteLine("1. Demo 1: using execution settings to define DataSources in the request");
            Console.WriteLine("2. Demo 2: Using a SearchPlugin and Prompt Function");
            
            demoNumber = Console.ReadLine();

            switch (demoNumber)
            {
                case "1":
                    RunDemo1(kernel).GetAwaiter().GetResult();;
                    break;
                // case "2": //SHOWN IN DOCS BUT NOT IMPLEMENTED
                //     RunDemo2(kernel).GetAwaiter().GetResult();;
                //     break;
                
                default:
                    Console.WriteLine("Invalid demo number");
                    break;
            }
        } while (demoNumber != "exit");

        /*1. using execution settings to define DataSources in the request*/
        async Task RunDemo1(Kernel kernel)
        {
            Console.WriteLine("\n DEMO 1 \n");
            // Azure Search configuration
            var index = "brochures-index";
            var azureSearchExtensionConfiguration = new AzureSearchChatExtensionConfiguration
            {
                SearchEndpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT")),
                //no authentication --> default credential System Managed Identity
                IndexName = index
            };

            // if (!string.IsNullOrEmpty(config.SearchKey))
            // {
            //     azureSearchExtensionConfiguration.Authentication = new OnYourDataApiKeyAuthenticationOptions(config.SearchKey);
            // }

            //This configutation is on prerelease https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.connectors.openai.openaipromptexecutionsettings.azurechatextensionsoptions?view=semantic-kernel-dotnet 
            #pragma warning disable SKEXP0010
            var chatExtensionsOptions = new AzureChatExtensionsOptions { Extensions = { azureSearchExtensionConfiguration } };
            executionSettings = new OpenAIPromptExecutionSettings { AzureChatExtensionsOptions = chatExtensionsOptions };
            #pragma warning restore SKEXP0010
            
            //Make the request to the kernel
            var prompt = "Best months to travel to London";

            var result = await kernel.InvokePromptAsync(prompt, new(executionSettings));

            // Print the result
            Console.WriteLine(result);
        }







            
        var prompt = "What are the best months to travel to London?";

        var result = await kernel.InvokePromptAsync(prompt, new(executionSettings));

        // Print the result
        Console.WriteLine(result);
    }
}

