# Add references
# Add references
import asyncio
from typing import cast
from dotenv import load_dotenv
from agent_framework import Message
from agent_framework.azure import AzureAIAgentClient
from agent_framework.orchestrations import SequentialBuilder
from azure.identity import AzureCliCredential

load_dotenv()

async def main():
    # Agent instructions
    summarizer_instructions="""
    Summarize the customer's feedback in one short sentence. Keep it neutral and concise.
    Example output:
    App crashes during photo upload.
    User praises dark mode feature.
    """

    classifier_instructions="""
    Classify the feedback as one of the following: Positive, Negative, or Feature request.
    """

    action_instructions="""
    Based on the summary and classification, suggest the next action in one short sentence.
    Example output:
    Escalate as a high-priority bug for the mobile team.
    Log as positive feedback to share with design and marketing.
    Log as enhancement request for product backlog.
    """

    # Create the chat client
    # Create the chat client
    credential = AzureCliCredential()
    async with (
        AzureAIAgentClient(credential=credential) as chat_client,
    ):

        # Create agents
        # Create agents
        summarizer = chat_client.as_agent(
            instructions=summarizer_instructions,
            name="summarizer",
        )

        classifier = chat_client.as_agent(
            instructions=classifier_instructions,
            name="classifier",
        )

        action = chat_client.as_agent(
            instructions=action_instructions,
            name="action",
        )

        # Initialize the current feedback
        # Initialize the current feedback
        feedback="""
        I use the dashboard every day to monitor metrics, and it works well overall. 
        But when I'm working late at night, the bright screen is really harsh on my eyes. 
        If you added a dark mode option, it would make the experience much more comfortable.
        """

        # Build sequential orchestration
        # Build sequential orchestration
        workflow = SequentialBuilder(participants=[summarizer, classifier, action]).build()
    
        # Run and collect outputs
        outputs: list[list[Message]] = []
        async for event in workflow.run(f"Customer feedback: {feedback}", stream=True):
            if event.type == "output":
                outputs.append(cast(list[Message], event.data))
    
        # Display outputs
        if outputs:
            for i, msg in enumerate(outputs[-1], start=1):
                name = msg.author_name or ("assistant" if msg.role == "assistant" else "user")
                print(f"{'-' * 60}\n{i:02d} [{name}]\n{msg.text}")
    
    
if __name__ == "__main__":
    asyncio.run(main())