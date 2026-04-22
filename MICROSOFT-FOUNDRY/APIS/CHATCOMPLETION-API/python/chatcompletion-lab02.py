
# pip install openai==1.55.3


import os
# from dotenv import load_dotenv

# Add Azure OpenAI package
# Add Azure OpenAI package
from openai import AzureOpenAI

def main(): 
        
    try: 
    
        # Get configuration settings 
        #load_dotenv()
        azure_oai_endpoint = os.getenv("AOAI_SWEDEN_END")
        azure_oai_key = os.getenv("AOAI_SWEDEN_KEY")
        azure_oai_deployment = "gpt-35-turbo-16k"
        
        # Initialize the Azure OpenAI client...
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


        while True:
            # Get input text
            input_text = input("Enter the prompt (or type 'quit' to exit): ")
            if input_text.lower() == "quit":
                break
            if len(input_text) == 0:
                print("Please enter a prompt.")
                continue

            print("\nSending request for summary to Azure OpenAI endpoint...\n\n")
            
            # Add code to send request...
            # Add code to send request...
            # Send request to Azure OpenAI model
            response = client.chat.completions.create(
                model=azure_oai_deployment,
                temperature=0.7,
                max_tokens=400,
                messages=[
                    {"role": "system", "content": system_message},
                    {"role": "user", "content": input_text}
                ]
            )
            generated_text = response.choices[0].message.content

            # Print the response
            print("Response: " + generated_text + "\n")
                        
                        

    except Exception as ex:
        print(ex)

if __name__ == '__main__': 
    main()