using Microsoft.SemanticKernel;

namespace _03_05e;

public class NativeFunction
{
  public static async Task Execute()
  {
    var modelDeploymentName = "gpt-4-unai";
    var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AOAI_SWEDEN_END");
    var azureOpenAIApiKey = Environment.GetEnvironmentVariable("AOAI_SWEDEN_KEY");

    var builder = Kernel.CreateBuilder();
    builder.Services.AddAzureOpenAIChatCompletion(
        modelDeploymentName,
        azureOpenAIEndpoint,
        azureOpenAIApiKey
    );
    builder.Plugins.AddFromType<MyMathPlugin>();
    var kernel = builder.Build();
    

    // Also able to add it after the kernel has been built
    // kernel.ImportPluginFromType<MyMathPlugin>();
    var NumberToSquareRoot = 81;
    var squareRootResult =
        await kernel.InvokeAsync(
          "MyMathPlugin",
          "Sqrt",
          new() {
            { "number1", NumberToSquareRoot }
          });

    Console.WriteLine($"The Square root of {NumberToSquareRoot} is:  {squareRootResult}");

    Console.ReadLine();
  }
}