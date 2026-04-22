import os
import asyncio
import json
from dotenv import load_dotenv
from contextlib import AsyncExitStack
from azure.ai.projects import AIProjectClient
from azure.ai.projects.models import FunctionTool
from azure.identity import DefaultAzureCredential
from azure.ai.projects.models import PromptAgentDefinition, FunctionTool
from openai.types.responses.response_input_param import FunctionCallOutput, ResponseInputParam

# Add references
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

# Clear the console
os.system('cls' if os.name=='nt' else 'clear')

# Load environment variables from .env file
load_dotenv()
# project_endpoint = os.getenv("PROJECT_ENDPOINT")
# model_deployment = os.getenv("MODEL_DEPLOYMENT_NAME")
project_endpoint= os.getenv("AI_FOUNDRY_PROJECT_END")
model_deployment = "gpt-4.1"


async def connect_to_server(exit_stack: AsyncExitStack):
    server_params = StdioServerParameters(
        command="python",
        args=["server.py"],
        env=None
    )

    # Start the MCP server
    stdio_transport = await exit_stack.enter_async_context(stdio_client(server_params))
    stdio, write = stdio_transport

    # Create an MCP client session
    session = await exit_stack.enter_async_context(ClientSession(stdio, write))
    await session.initialize()

    # List available tools
    response = await session.list_tools()
    tools = response.tools
    print(f"\nConnected to server with tools: {[tool.name for tool in tools]}")

    return session

async def chat_loop(session):

    # Connect to the agents client
    with (
        DefaultAzureCredential() as credential,
        AIProjectClient(endpoint=project_endpoint, credential=credential) as project_client,
        project_client.get_openai_client() as openai_client,
    ):

        # Get the mcp tools available from the server
        response = await session.list_tools()
        tools = response.tools

        # Build a function for each tool
        agent_tools = []
        for tool in tools:
            agent_tools.append(FunctionTool(
                name=tool.name,
                description=tool.description or "",
                parameters=tool.inputSchema,
            ))

        # Create the agent
        agent = project_client.agents.create_version(
            agent_name="inventory-agent",
            definition=PromptAgentDefinition(
                model=model_deployment,
                instructions="You are an inventory management agent. Use the available tools to check inventory levels and weekly sales data to help users manage their product inventory.",
                tools=agent_tools,
            ),
        )
        print(f"Created agent: {agent.name} (version: {agent.version})")

        # Create a thread for the chat session
        conversation = openai_client.conversations.create()

        # Create an input list to hold function call outputs to send back to the model
        input_list: ResponseInputParam = []

        while True:
            user_input = input("Enter a prompt for the inventory agent. Use 'quit' to exit.\nUSER: ").strip()
            if user_input.lower() == "quit":
                print("Exiting chat.")
                break

            # Send a prompt to the agent
            openai_client.conversations.items.create(
                conversation_id=conversation.id,
                items=[{"type": "message", "role": "user", "content": user_input}],
            )

            # Retrieve the agent's response, which may include function calls to the MCP server tools
            response = openai_client.responses.create(
                conversation=conversation.id,
                extra_body={"agent": {"name": agent.name, "type": "agent_reference"}},
                input=input_list,
            )

            # Check the run status for failures
            if response.status == "failed":
                print(f"Response failed: {response.error}")

            # Process function calls
            input_list = []
            for item in response.output:
                if item.type == "function_call":
                    tool_name = item.name
                    tool_args = json.loads(item.arguments) if item.arguments else {}
                    print(f"\nCalling MCP tool: {tool_name} with args: {tool_args}")

                    # Call the MCP server tool
                    result = await session.call_tool(tool_name, tool_args)
                    print(f"Tool result: {result.content[0].text}")

                    # Add the function call output to the input list
                    input_list.append(
                        FunctionCallOutput(
                            type="function_call_output",
                            call_id=item.call_id,
                            output=result.content[0].text,
                        )
                    )

            # Send function call outputs back to the model and retrieve a response
            if input_list:
                response = openai_client.responses.create(
                    conversation=conversation.id,
                    extra_body={"agent": {"name": agent.name, "type": "agent_reference"}},
                    input=input_list,
                )

            # Print the agent's response
            for item in response.output:
                if hasattr(item, "content"):
                    for content in item.content:
                        if hasattr(content, "text"):
                            print(f"\nAGENT: {content.text}")
           
           
        # Delete the agent when done
        print("Cleaning up agents:")
        project_client.agents.delete_version(agent_name=agent.name, agent_version=agent.version)
        print("Deleted inventory agent.")


async def main():
    import sys
    exit_stack = AsyncExitStack()
    try:
        session = await connect_to_server(exit_stack)
        await chat_loop(session)
    finally:
        await exit_stack.aclose()

if __name__ == "__main__":
    asyncio.run(main())