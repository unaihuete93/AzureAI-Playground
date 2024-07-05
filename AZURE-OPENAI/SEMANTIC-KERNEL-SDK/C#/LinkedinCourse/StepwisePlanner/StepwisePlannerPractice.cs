using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.ChatCompletion;

namespace _04_07e;

public class StepwisePlannerPractice
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
    var kernel = builder.Build();

    var kernelFunctionRespondAsScientific = kernel.CreateFunctionFromPrompt(
        new PromptTemplateConfig()
        {
          Name = "RespondAsScientific",
          Description = "Respond as if you were a Scientific.",
          Template = @"After the user request/question, 
                    {{$input}},
                    Respond to the user question as if you were a Scientific. 
                    Respond to it as you were him, showing your personality",
          TemplateFormat = "semantic-kernel",
          InputVariables = [
                new() { Name = "input" }
            ]
        });

    var kernelFunctionRespondAsPoliceman = kernel.CreateFunctionFromPrompt(
        new PromptTemplateConfig()
        {
          Name = "RespondAsPoliceman",
          Description = "Respond as if you were a Policeman.",
          Template = @"After the user request/question, 
                    {{$input}},
                    Respond to the user question as if you were a Policeman, showing your personality, 
                    humor and level of intelligence.",
          TemplateFormat = "semantic-kernel",
          InputVariables = [
                new() { Name = "input" }
            ]
        });

    KernelPlugin roleOpinionsPlugin =
        KernelPluginFactory.CreateFromFunctions(
            "roleTalk",
            "Responds to questions or statements asuming different roles.",
            new[] {
                    kernelFunctionRespondAsScientific,
                    kernelFunctionRespondAsPoliceman
                  });
    kernel.Plugins.Add(roleOpinionsPlugin);

    string planPrompt = "This is the user question to my expert friends:" +
        "---" +
        "User Question: " +
        "I am being attacked by a thug which wants to rob me, what do the superheroes recommend me to do in my position? I am weak, no combat skills and not a good runner... " +
        "---" +
        "Please take this question as input for getting the expert opinions, Mr. Policeman, Scientist suggestions. Do not modify the input." +
        "Use the plugin roleTalk to get the suggestions and opinions of the experts." +
        "In addition state each expert opinion on each other stated opinions." +
        "Put the expert responses preceded with EXPERT SUGGESTIONS: and inside that preceed with Policeman: and Scientist: for clarity." +
        "Perform this with the following steps: " +
        "1. Get the suggestions from each the experts." +
        "2. Get the opinions of each expert on the other expert suggestions." +
        "3. Return the results in the format: " +
        "Expert SUGGESTIONS: Policeman: <suggestion> Scientist: <suggestion> " +
        "OPINIONS: Policeman: <opinion on Scientist> Scientist: <opinion on Policeman> " +
        "IMPORTANT: on the plan ensure that the user question is asigned to a variable and used as input. Do not modify the user question input.";

    #pragma warning disable SKEXP0061 // XXXX is the warning number

    var planner = new FunctionCallingStepwisePlanner();
    FunctionCallingStepwisePlannerResult result =
        await planner.ExecuteAsync(kernel, planPrompt);

    

    // Print the result to the console
    Console.WriteLine($"Result: {result.FinalAnswer}");
    Console.WriteLine($"Iterations needed: {result.Iterations}");

    //print the Contents of ChatHistory
    ChatHistory chatMessageContents = result.ChatHistory;

    foreach (var chatMessage in chatMessageContents)
    {
      Console.WriteLine($"ChatMessage: {chatMessage}");
    }
    #pragma warning restore SKEXP0061
    Console.ReadLine();
  }
}