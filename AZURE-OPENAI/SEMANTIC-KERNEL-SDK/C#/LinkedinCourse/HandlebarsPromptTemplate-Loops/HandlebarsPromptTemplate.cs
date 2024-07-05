using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
namespace _03_08e;

public class HandlebarsPromptTemplate
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
    builder.Plugins.AddFromType<WhatTimeIsIt>();
    var kernel = builder.Build();

    // Create agenda 
    List<string> todaysCalendar = ["8am - wakeup", "9am - work", "12am - lunch", "1pm - work", "6pm - exercise", "7pm - study", "10pm - sleep"];

    // this == each item in the list
    var handlebarsTemplate = @"
                    Please explain in a fun way the day agenda
                    {{ set ""dayAgenda"" (todaysCalendar)}}
                    {{ set ""whatTimeIsIt"" (WhatTimeIsIt-Time) }}

                    {{#each dayAgenda}}
                        Explain what you are doing at {{this}} in a fun way.
                    {{/each}}

                    Explain what you will be doing next at {{whatTimeIsIt}} in a fun way.";

    // HANDLEBARS are  a way to create your own plans , allowing the model to leverage native features 
    //like loops and conditions without additional prompting

    var handlebarsFunction = kernel.CreateFunctionFromPrompt(
        new PromptTemplateConfig()
        {
          Template = handlebarsTemplate,
          TemplateFormat = "handlebars"
        },
        new HandlebarsPromptTemplateFactory()
    );

    var todaysFunCalendar = await kernel.InvokeAsync(
        handlebarsFunction,
        new() {
          { "todaysCalendar", todaysCalendar }
        }
    );

    Console.WriteLine($"Today's fun calendar:  {todaysFunCalendar}");

    Console.ReadLine();
  }
}