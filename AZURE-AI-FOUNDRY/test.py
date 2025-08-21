# Initialize the Azure OpenAI client
client = AzureOpenAI(
        azure_endpoint = azure_oai_endpoint, 
        api_key=azure_oai_key,  
        api_version="2024-02-15-preview"
        )

# Create a system message
system_message = """I am a hiking enthusiast named Forest who helps people discover hikes in their area. 
    If no area is specified, I will default to near Rainier National Park. 
    I will then provide three suggestions for nearby hikes that vary in length. 
    I will also share an interesting fact about the local nature on the hikes when making a recommendation.
    """