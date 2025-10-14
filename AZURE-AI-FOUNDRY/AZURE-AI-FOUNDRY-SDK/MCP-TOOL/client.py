import os
from dotenv import load_dotenv

# Add references
from azure.identity import DefaultAzureCredential
from azure.ai.agents import AgentsClient
from azure.ai.agents.models import McpTool, ToolSet, ListSortOrder


# Load environment variables from .env file
load_dotenv()
project_endpoint = os.getenv("AI_FOUNDRY_PROJECT_END")
model_deployment = "gpt-4.1"

# Connect to the agents client
agents_client = AgentsClient(
        endpoint=project_endpoint,
        credential=DefaultAzureCredential()
)


# MCP server configuration
mcp_server_url = "https://learn.microsoft.com/api/mcp"
mcp_server_label = "mslearn"

# Initialize agent MCP tool
mcp_tool = McpTool(
     server_label=mcp_server_label,
     server_url=mcp_server_url,
)
    
mcp_tool.set_approval_mode("never")
    
toolset = ToolSet()
toolset.add(mcp_tool)

# Create agent with MCP tool and process agent run
with agents_client:

    # Create a new agent
    agents = list(agents_client.list_agents())
    
    mcp_agent = next((a for a in agents if a.name == "my-mcp-agent"), None)
    if mcp_agent:
        print(f"Found existing agent: {mcp_agent.name}")
    else:
        print("No existing agent found, creating a new one.")
        agent = agents_client.create_agent(
            model=model_deployment,
            name="my-mcp-agent",
            instructions="""
            You have access to an MCP server called `microsoft.docs.mcp` - this tool allows you to 
            search through Microsoft's latest official documentation. Use the available MCP tools 
            to answer questions and perform tasks."""
        )      
    # Log info
    print(f"Created agent, ID: {agent.id}")
    print(f"MCP Server: {mcp_tool.server_label} at {mcp_tool.server_url}")

    # Create thread for communication
    thread = agents_client.threads.create()
    print(f"Created thread, ID: {thread.id}")

    # Create a message on the thread
    prompt = input("\nHow can I help?: ")
    message = agents_client.messages.create(
        thread_id=thread.id,
        role="user",
        content=prompt,
    )
    print(f"Created message, ID: {message.id}")

    # Set approval mode, use mcp tools without approval
    mcp_tool.set_approval_mode("never")

    # Create and process agent run in thread with MCP tools
    run = agents_client.runs.create_and_process(thread_id=thread.id, agent_id=agent.id, toolset=toolset)
    print(f"Created run, ID: {run.id}")
    
    # Check run status
    print(f"Run completed with status: {run.status}")
    if run.status == "failed":
        print(f"Run failed: {run.last_error}")

    # Display run steps and tool calls
    run_steps = agents_client.run_steps.list(thread_id=thread.id, run_id=run.id)
    for step in run_steps:
        print(f"Step {step['id']} status: {step['status']}")

        # Check if there are tool calls in the step details
        step_details = step.get("step_details", {})
        tool_calls = step_details.get("tool_calls", [])

        if tool_calls:
            # Display the MCP tool call details
            print("  MCP Tool calls:")
            for call in tool_calls:
                print(f"    Tool Call ID: {call.get('id')}")
                print(f"    Type: {call.get('type')}")
                print(f"    Type: {call.get('name')}")

        print()  # add an extra newline between steps

    # Fetch and log all messages
    messages = agents_client.messages.list(thread_id=thread.id, order=ListSortOrder.ASCENDING)
    print("\nConversation:")
    print("-" * 50)
    for msg in messages:
        if msg.text_messages:
            last_text = msg.text_messages[-1]
            print(f"{msg.role.upper()}: {last_text.text.value}")
            print("-" * 50)

    # Clean-up and delete the agent once the run is finished.
    #agents_client.delete_agent(agent.id)
    #print("Deleted agent")

    ##DEMO  "Give me the Azure CLI commands to create an Azure Container App with a managed identity."