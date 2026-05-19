import os
from dotenv import load_dotenv

# Add references
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient

load_dotenv()
endpoint = os.getenv("AI_FOUNDRY_PROJECT_END")

# Connect to the AI Project client
with (
    DefaultAzureCredential() as credential,
    AIProjectClient(endpoint=endpoint, credential=credential) as project_client,
    project_client.get_openai_client() as openai_client,
):

    # Specify the workflow (already created)
    workflow = {
        "name": "Support-Ticket-Workflow"
    }

    # Create a conversation and run the workflow
    conversation = openai_client.conversations.create()
    print(f"Created conversation (id: {conversation.id})")

    stream = openai_client.responses.create(
        conversation=conversation.id,
        extra_body={"agent_reference": {"name": workflow["name"], "type": "agent_reference"}},
        input="Start",
        stream=True,
    )

    # Process events from the workflow run
    for event in stream:
        if event.type == "response.completed":
            print("\nResponse completed:")
            response = openai_client.responses.retrieve(event.response.id)
            print(f"{response.output_text}")

    # Clean up resources
    openai_client.conversations.delete(conversation_id=conversation.id)
    print("\nConversation deleted")