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
using _03_05e;

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


        builder.Plugins.AddFromType<MyMathPlugin>();

        var kernel = builder.Build();

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
            
        var prompt = "What is the square root of 81?";

        var result = await kernel.InvokePromptAsync(prompt, new(executionSettings));

        // Print the result
        Console.WriteLine(result);
    }
}

