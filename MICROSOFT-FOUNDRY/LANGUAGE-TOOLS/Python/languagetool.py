import os
from azure.ai.textanalytics import TextAnalyticsClient
from azure.core.credentials import AzureKeyCredential
from azure.identity import DefaultAzureCredential

#get secrets from environment variables
foundry_endpoint = os.getenv('AI_FOUNDRY_ENDPOINT')

# Create client using endpoint and SP environment credentials
credential = DefaultAzureCredential()
ai_client = TextAnalyticsClient(endpoint=foundry_endpoint, credential=credential)

# Method for recognizing content in documents
def recognize_your_text(client):
    documents = [
        "Call our office at 312-555-1234 and ask for Graham Barnes, or send an email to support@contoso.com."
    ]
    # Replace null with the correct method to recognize the requested information.
    response = client.recognize_entities(documents, language="en")
    result = [doc for doc in response if not doc.is_error]
    for doc in result:
        print("Redacted Text: {}".format(documents))
        for entity in doc.entities:
            print("Entity: {}".format(entity.text))
            print("\tCategory: {}".format(entity.category))
            print("\tConfidence Score: {}".format(entity.confidence_score))
            print("\tOffset: {}".format(entity.offset))
            print("\tLength: {}".format(entity.length))

recognize_your_text(ai_client)

