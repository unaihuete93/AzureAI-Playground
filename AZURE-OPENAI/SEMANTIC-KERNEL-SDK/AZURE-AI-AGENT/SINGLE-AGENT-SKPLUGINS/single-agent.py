import os
import asyncio
from pathlib import Path

# Add references
#from dotenv import load_dotenv
from azure.identity.aio import DefaultAzureCredential
from semantic_kernel.agents import AzureAIAgent, AzureAIAgentSettings, AzureAIAgentThread
from semantic_kernel.functions import kernel_function
from typing import Annotated


async def main():
    # Clear the console
    os.system('cls' if os.name=='nt' else 'clear')

    # Load the expnses data file
    script_dir = Path(__file__).parent
    file_path = script_dir / 'data.txt'
    with file_path.open('r') as file:
        data = file.read() + "\n"

    # Ask for a prompt
    user_prompt = input(f"Here is the expenses data in your file:\n\n{data}\n\nWhat would you like me to do with it?\n\n")
    
    # Run the async agent code
    await process_expenses_data (user_prompt, data)
    
async def process_expenses_data(prompt, expenses_data):

    # Get configuration settings
    project_endpoint = os.getenv("AI_FOUNDRY_PROJECT_END")
    model_deployment = "gpt-4"  # Make sure this is a valid deployment name

    #create env variables AZURE_AI_AGENT_ENDPOINT and AZURE_AI_AGENT_MODEL_DEPLOYMENT_NAME

    os.environ["AZURE_AI_AGENT_ENDPOINT"] = project_endpoint
    os.environ["AZURE_AI_AGENT_MODEL_DEPLOYMENT_NAME"] = model_deployment
    #print both
    print("AZURE_AI_AGENT_ENDPOINT:", os.environ["AZURE_AI_AGENT_ENDPOINT"])
    print("AZURE_AI_AGENT_MODEL_DEPLOYMENT_NAME:", os.environ["AZURE_AI_AGENT_MODEL_DEPLOYMENT_NAME"])
    ai_agent_settings = AzureAIAgentSettings()

    # Connect to the Azure AI Foundry project
        
    async with (
        DefaultAzureCredential(
            exclude_environment_credential=True,
            exclude_managed_identity_credential=True) as creds,
        AzureAIAgent.create_client(
            credential=creds
        ) as project_client,
    ):
        #print used credential
        print("DefaultAzureCredential:", creds)
        # Define an Azure AI agent that sends an expense claim email
        expenses_agent_def = await project_client.agents.create_agent(
            model= ai_agent_settings.model_deployment_name,
            name="expenses_agent",
            instructions="""You are an AI assistant for expense claim submission.
                            When a user submits expenses data and requests an expense claim, use the plug-in function to send an email to expenses@contoso.com with the subject 'Expense Claim`and a body that contains itemized expenses with a total.
                            Then confirm to the user that you've done so."""
        )

        # Create a semantic kernel agent
        # Create a semantic kernel agent
        expenses_agent = AzureAIAgent(
            client=project_client,
            definition=expenses_agent_def,
            plugins=[EmailPlugin()]
        )

        # Use the agent to process the expenses data
        # If no thread is provided, a new thread will be
        # created and returned with the initial response
        thread: AzureAIAgentThread | None = None
        try:
            # Add the input prompt to a list of messages to be submitted
            prompt_messages = [f"{prompt}: {expenses_data}"]
            # Invoke the agent for the specified thread with the messages
            response = await expenses_agent.get_response(prompt_messages, thread=thread)
            # Display the response
            print(f"\n# {response.name}:\n{response}")
        except Exception as e:
            # Something went wrong
            print (e)
        finally:
            # Cleanup: Delete the thread and agent
            await thread.delete() if thread else None
            await project_client.agents.delete_agent(expenses_agent.id)


# Create a Plugin for the email functionality
class EmailPlugin:
   """A Plugin to simulate email functionality."""

   @kernel_function(description="Sends an email.")
   def send_email(self,
                  to: Annotated[str, "Who to send the email to"],
                  subject: Annotated[str, "The subject of the email."],
                  body: Annotated[str, "The text body of the email."]):
       print("\nTo:", to)
       print("Subject:", subject)
       print(body, "\n")

if __name__ == "__main__":
    asyncio.run(main())