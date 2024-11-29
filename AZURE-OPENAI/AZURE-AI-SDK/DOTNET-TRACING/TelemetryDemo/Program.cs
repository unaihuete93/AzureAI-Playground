//Azure imports
using Azure;
//using Azure.Identity;
using Azure.AI.Inference;
//using Azure.AI.OpenAI;

// Open telemetry imports
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

// Enables experimental Azure SDK observability
AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

// By default instrumentation captures chat messages without content
// since content can be very verbose and have sensitive information.
// The following AppContext switch enables content recording.
AppContext.SetSwitch("Azure.Experimental.TraceGenAIMessageContent", true);

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddSource("Azure.AI.Inference.*")
    .ConfigureResource(r => r.AddService("sample"))
    .AddConsoleExporter()
    .AddOtlpExporter()
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddMeter("Azure.AI.Inference.*")
    .ConfigureResource(r => r.AddService("sample"))
    .AddConsoleExporter()
    .AddOtlpExporter()
    .Build();


var endpoint = new Uri(System.Environment.GetEnvironmentVariable("AOAI_SWEDEN_END"));
var credential = new AzureKeyCredential(System.Environment.GetEnvironmentVariable("AOAI_SWEDEN_KEY"));
var model = "gpt-4-unai"; //System.Environment.GetEnvironmentVariable("MODEL_NAME");

var client = new ChatCompletionsClient(
    endpoint,
    credential,
    new AzureAIInferenceClientOptions());

var requestOptions = new ChatCompletionsOptions()
{
    Messages =
    {
        new ChatRequestSystemMessage("You are a helpful assistant."),
        new ChatRequestUserMessage("What is the capital of France?"),
    },
    Model = model,
    Temperature = 1,
    MaxTokens = 1000,
     ResponseFormat = new ChatCompletionsResponseFormatJSON()
};
// Call the endpoint and output the response.
var response = client.Complete(requestOptions);
//Console.WriteLine($"Response: {response.Value.Choices[0].Message.Content}");
