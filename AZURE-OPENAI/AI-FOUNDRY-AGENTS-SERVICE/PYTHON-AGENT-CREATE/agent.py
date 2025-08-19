import os
#from dotenv import load_dotenv
from typing import Any
from pathlib import Path

# Add references
from azure.identity import DefaultAzureCredential
from azure.ai.agents import AgentsClient
from azure.ai.agents.models import FilePurpose, CodeInterpreterTool, ListSortOrder, MessageRole, FunctionTool, ToolSet
#from azure.ai.agents.models import FunctionTool, ToolSet, ListSortOrder, MessageRole
from user_functions import user_functions
from azure.core.credentials import AzureKeyCredential


# Add references

def main(): 

    # Clear the console
    os.system('cls' if os.name=='nt' else 'clear')

    # Load environment variables from environment variables
    #load_dotenv()
    project_endpoint= os.getenv("AI_FOUNDRY_PROJECT_END")
    model_deployment = "gpt-4.1"
    

    # Display the data to be analyzed
    script_dir = Path(__file__).parent  # Get the directory of the script
    file_path = script_dir / 'data.txt'

    with file_path.open('r') as file:
        data = file.read() + "\n"
        print(data)

    # Connect to the Agent client
    agent_client = AgentsClient(
        endpoint=project_endpoint,
        credential=DefaultAzureCredential()
    )
    with agent_client:
        # Upload the data file and create a CodeInterpreterTool
        file = agent_client.files.upload_and_poll(
            file_path=file_path, purpose=FilePurpose.AGENTS
        )
        print(f"Uploaded {file.filename}")

        code_interpreter = CodeInterpreterTool(file_ids=[file.id])

        # Create a FunctionTool and add it to the ToolSet
        functions = FunctionTool(user_functions)
        toolset = ToolSet()
        toolset.add(functions)
        toolset.add(code_interpreter)
        agent_client.enable_auto_function_calls(toolset)

        # Check if agent "data-agent" exists
        agents = list(agent_client.list_agents())
        agent = None
        for a in agents:
            if a.name == "data-agent":
                agent = a
                print(f"Found existing agent: {agent.name}")
                break
        if not agent:
            agent = agent_client.create_agent(
                model=model_deployment,
                name="data-agent",
                instructions="You are an AI agent that analyzes the data in the file that has been uploaded when asked. You are also a technical support agent. When a user has a technical issue, you get their email address and a description of the issue. Then you use those values to submit a support ticket using the function available to you. If a file is saved, tell the user the file name",
                toolset=toolset,
            )
            print(f"Created new agent: {agent.name}")

        # Create a thread for the conversation
        thread = agent_client.threads.create()

        # Loop until the user types 'quit'
        while True:
            user_prompt = input("Enter a prompt (or type 'quit' to exit): ")
            if user_prompt.lower() == "quit":
                break
            if len(user_prompt) == 0:
                print("Please enter a prompt.")
                continue

            message = agent_client.messages.create(
                thread_id=thread.id,
                role="user",
                content=user_prompt,
            )

            run = agent_client.runs.create_and_process(thread_id=thread.id, agent_id=agent.id)

            if run.status == "failed":
                print(f"Run failed: {run.last_error}")

            last_msg = agent_client.messages.get_last_message_text_by_role(
                thread_id=thread.id,
                role=MessageRole.AGENT,
            )
            if last_msg:
                print(f"Last Message: {last_msg.text.value}")

        print("\nConversation Log:\n")
        messages = agent_client.messages.list(thread_id=thread.id, order=ListSortOrder.ASCENDING)
        for message in messages:
            if message.text_messages:
                last_msg = message.text_messages[-1]
                print(f"{message.role}: {last_msg.text.value}\n")
if __name__ == '__main__': 
    main()
