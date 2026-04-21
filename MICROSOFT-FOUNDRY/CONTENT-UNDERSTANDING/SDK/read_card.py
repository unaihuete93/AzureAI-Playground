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

        # Get the business card
        image_file = 'biz-card-1.png'
        if len(sys.argv) > 1:
            image_file = sys.argv[1]

        # Get config settings
        # load_dotenv()
        ai_svc_endpoint = os.getenv('AI_FOUNDRY_SERVICE_END')
        ai_svc_key = os.getenv('AI_FOUNDRY_PROJECT_KEY')
        analyzer = "business-card-analyzer"

        # Analyze the business card
        analyze_card (image_file, analyzer, ai_svc_endpoint, ai_svc_key)

        print("\n")

    except Exception as ex:
        print(ex)



def analyze_card (image_file, analyzer, endpoint, key):
    
    # Use Content Understanding to analyze the image
    # Use Content Understanding to analyze the image
    print (f"Analyzing {image_file}")

    # Set the API version
    CU_VERSION = "2025-05-01-preview"

    # Read the image data
    with open(image_file, "rb") as file:
        image_data = file.read()
        
    ## Use a POST request to submit the image data to the analyzer
    print("Submitting request...")
    headers = {
        "Ocp-Apim-Subscription-Key": key,
        "Content-Type": "application/octet-stream"}
    url = f'{endpoint}/contentunderstanding/analyzers/{analyzer}:analyze?api-version={CU_VERSION}'
    response = requests.post(url, headers=headers, data=image_data)

    # Get the response and extract the ID assigned to the analysis operation
    print(response.status_code)
    response_json = response.json()
    id_value = response_json.get("id")

    # Use a GET request to check the status of the analysis operation
    print ('Getting results...')
    time.sleep(1)
    result_url = f'{endpoint}/contentunderstanding/analyzerResults/{id_value}?api-version={CU_VERSION}'
    result_response = requests.get(result_url, headers=headers)
    print(result_response.status_code)

    # Keep polling until the analysis is complete
    status = result_response.json().get("status")
    while status == "Running":
        time.sleep(1)
        result_response = requests.get(result_url, headers=headers)
        status = result_response.json().get("status")

    # Process the analysis results
    if status == "Succeeded":
        print("Analysis succeeded:\n")
        result_json = result_response.json()
        output_file = "results.json"
        with open(output_file, "w") as json_file:
            json.dump(result_json, json_file, indent=4)
            print(f"Response saved in {output_file}\n")

        # Iterate through the fields and extract the names and type-specific values
        contents = result_json["result"]["contents"]
        for content in contents:
            if "fields" in content:
                fields = content["fields"]
                for field_name, field_data in fields.items():
                    if field_data['type'] == "string":
                        print(f"{field_name}: {field_data['valueString']}")
                    elif field_data['type'] == "number":
                        print(f"{field_name}: {field_data['valueNumber']}")
                    elif field_data['type'] == "integer":
                        print(f"{field_name}: {field_data['valueInteger']}")
                    elif field_data['type'] == "date":
                        print(f"{field_name}: {field_data['valueDate']}")
                    elif field_data['type'] == "time":
                        print(f"{field_name}: {field_data['valueTime']}")
                    elif field_data['type'] == "array":
                        print(f"{field_name}: {field_data['valueArray']}")




if __name__ == "__main__":
    main()        