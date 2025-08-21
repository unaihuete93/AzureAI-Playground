from dotenv import load_dotenv
import os
import sys
import time
import requests
import json


def main():

    # Clear the console
    os.system('cls' if os.name=='nt' else 'clear')

    try:

        # Get the business card schema
        with open("biz-card.json", "r") as file:
            schema_json = json.load(file)
        
        card_schema = json.dumps(schema_json)

        # Get config settings
        #load_dotenv()
        ai_svc_endpoint = os.getenv('AI_FOUNDRY_SERVICE_END')
        ai_svc_key = os.getenv('AI_FOUNDRY_PROJECT_KEY')
        analyzer = "business-card-analyzer"

        # Create the analyzer
        create_analyzer (card_schema, analyzer, ai_svc_endpoint, ai_svc_key)

        print("\n")

    except Exception as ex:
        print(ex)



def create_analyzer (schema, analyzer, endpoint, key):
    
    # Create a Content Understanding analyzer
    print (f"Creating {analyzer}")

    # Set the API version
    CU_VERSION = "2025-05-01-preview"

    # initiate the analyzer creation operation
    headers = {
        "Ocp-Apim-Subscription-Key": key,
        "Content-Type": "application/json"}

    url = f"{endpoint}/contentunderstanding/analyzers/{analyzer}?api-version={CU_VERSION}"

    # Delete the analyzer if it already exists
    response = requests.delete(url, headers=headers)
    print(response.status_code)
    time.sleep(1)
 
    # Now create it
    response = requests.put(url, headers=headers, data=(schema))
    print(response.status_code)

    # Get the response and extract the callback URL
    callback_url = response.headers["Operation-Location"]

    # Check the status of the operation
    time.sleep(1)
    result_response = requests.get(callback_url, headers=headers)

    # Keep polling until the operation is no longer running
    status = result_response.json().get("status")
    while status == "Running":
        time.sleep(1)
        result_response = requests.get(callback_url, headers=headers)
        status = result_response.json().get("status")

    result = result_response.json().get("status")
    print(result)
    if result == "Succeeded":
        print(f"Analyzer '{analyzer}' created successfully.")
    else:
        print("Analyzer creation failed.")
        print(result_response.json())



if __name__ == "__main__":
    main()        