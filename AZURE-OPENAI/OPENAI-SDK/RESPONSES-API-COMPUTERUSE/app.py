# Supported regions and models https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/responses?tabs=python-secure#region-availability
#pip install openai 1.68.2 
#pip install azure-identity
import os
from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

# using Service Principal
token_provider = get_bearer_token_provider(
    DefaultAzureCredential(), "https://cognitiveservices.azure.com/.default"
)

client = AzureOpenAI(
  azure_endpoint = os.getenv("AOAI_SWEDEN_END"), 
  azure_ad_token_provider=token_provider,
  api_version="2025-03-01-preview"
)

response = client.responses.create(
    model="gpt-4o", # replace with your model deployment name 
    input="This is a test."
    #truncation="auto" required when using computer-use-preview model.

)