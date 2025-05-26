using System;
using Azure;
using System.IO;
using System.Text;
using System.Collections.Generic;
//using Microsoft.Extensions.Configuration;

// Add references
using Azure.Identity;
using Azure.AI.Projects;
using Azure.AI.Inference;

namespace chat_app
{
    class Program
    {
        static void Main(string[] args)
        {
            // Clear the console
            Console.Clear();
            
            try
            {
                // Get config settings
                
                string project_connection = Environment.GetEnvironmentVariable("AI_FOUNDRY_PROJECT_END");
                if (string.IsNullOrEmpty(project_connection))
                {
                    throw new Exception("Environment variable 'AI_FOUNDRY_PROJECT_END' is not set.");
                }
                string model_deployment = "gpt-4.1";


                // Initialize the project client

                // Use DefaultAzureCredential with options to include environment (using GH repo secrets) and exclude managed identity
                DefaultAzureCredentialOptions options = new()
                    { ExcludeEnvironmentCredential = false,
                    ExcludeManagedIdentityCredential = true };


                var projectClient = new AIProjectClient(
                    new Uri(project_connection),
                    new DefaultAzureCredential(options));

                // Get a chat client
                ChatCompletionsClient chat = projectClient.GetChatCompletionsClient();

                // Initialize prompt with system message
                var prompt = new List<ChatRequestMessage>(){
                                new ChatRequestSystemMessage("You are a helpful AI assistant that answers questions.")
                            };

                // Loop until the user types 'quit'
                string input_text = "";
                while (input_text.ToLower() != "quit")
                {
                    // Get user input
                    Console.WriteLine("Enter the prompt (or type 'quit' to exit):");
                    input_text = Console.ReadLine();
                    if (input_text.ToLower() != "quit")
                    {
                        // Get a chat completion

                        prompt.Add(new ChatRequestUserMessage(input_text));
                        var requestOptions = new ChatCompletionsOptions()
                        {
                            Model = model_deployment,
                            Messages = prompt
                        };

                        Response<ChatCompletions> response = chat.Complete(requestOptions);
                        var completion = response.Value.Content;
                        Console.WriteLine(completion);
                        prompt.Add(new ChatRequestAssistantMessage(completion));

                    }
                }



                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}