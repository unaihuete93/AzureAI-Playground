import asyncio
import os

from agent_framework.foundry import FoundryAgent
from azure.identity import DefaultAzureCredential

async def main() -> None:
    project_endpoint = os.getenv("AI_FOUNDRY_PROJECT_END") 
    agent_name = "Agent-mcp-remote" #connected to MS Learn MCP 

    agent = FoundryAgent(
        project_endpoint=project_endpoint,
        agent_name=agent_name,
        credential=DefaultAzureCredential()
    )

    print("Type your prompt and press Enter. Type 'exit' or 'quit' to stop.\n")

    conversation: list[str] = []

    while True:
        try:
            prompt = input("You: ").strip()
        except EOFError:
            print("\nSession ended.")
            break

        if not prompt:
            continue

        if prompt.lower() in {"exit", "quit"}:
            print("Session ended.")
            break

        conversation.append(prompt)
        response = await agent.run(conversation)
        print(f"\nAgent:\n{response}\n")
        conversation.append(str(response))


if __name__ == "__main__":
    asyncio.run(main())