using Microsoft.Extensions.Logging.Console;
using Microsoft.SemanticKernel.Plugins.Core;
using System.Text.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

// Note: do not update to 1.5 until the following issue is resolved:
// https://github.com/microsoft/semantic-kernel/issues/5264

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

public class FunctionCalling
{
  public static async Task Execute()
  {
    var modelDeploymentName = "gpt-4-unai";
    var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AOAI_SWEDEN_END");
    var azureOpenAIApiKey = Environment.GetEnvironmentVariable("AOAI_SWEDEN_KEY");

    var client = new HttpClient(new LoggingHttpClientHandler());

    var builder = Kernel.CreateBuilder();
    builder.Services.AddAzureOpenAIChatCompletion(
        modelDeploymentName,
        azureOpenAIEndpoint,
        azureOpenAIApiKey,
        // Use the custom HttpClientHandler
        httpclient: client
    );
    var kernel = builder.Build();

    kernel.Plugins.AddFromType<WhatDateIsIt>();

    string userPrompt = "What is the date?";

    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
      ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    var result = await kernel.InvokePromptAsync(
        userPrompt,
        new(openAIPromptExecutionSettings));

    Console.WriteLine($"Result: {result}");
    Console.ReadLine();
  }
}