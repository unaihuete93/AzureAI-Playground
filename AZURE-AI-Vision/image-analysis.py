## GENERIC IMAGE ANALYSIS PYTHON APPLICATION
## OUTPUT: Image Analysis (console), Object Detection, People Detection, Background Removal, Foreground Matte

#from dotenv import load_dotenv
import os
from array import array
from PIL import Image, ImageDraw
import sys
import time
from matplotlib import pyplot as plt
import numpy as np
import asyncio  # Import asyncio for asynchronous execution

# Import namespaces
from azure.ai.vision.imageanalysis import ImageAnalysisClient
from azure.ai.vision.imageanalysis.models import VisualFeatures
from azure.core.credentials import AzureKeyCredential


async def main():
    global cv_client

    try:
        # Get Configuration Settings
        #load_dotenv()
        ai_endpoint = os.getenv('AI_VISION_V4_END')
        ai_key = os.getenv('AI_VISION_V4_KEY')

        # Get image
        image_file = '/workspaces/AzureAI-Playground/AZURE-AI-Vision/images/Oyarzabal2.jpg'
        if len(sys.argv) > 1:
            image_file = sys.argv[1]

        # Authenticate Azure AI Vision client
        cv_client = ImageAnalysisClient(endpoint=ai_endpoint, credential=AzureKeyCredential(ai_key))

        # Analyze image
        await AnalyzeImage(image_file, cv_client)

        # Generate thumbnail
        # BackgroundForeground(image_file, cv_client)  # Update this function similarly if needed

    except Exception as ex:
        print(ex)


async def AnalyzeImage(image_file, cv_client):
    print('\nAnalyzing', image_file)

    # Read image data as bytes
    with open(image_file, 'rb') as f:
        image_data = f.read()

    # Specify features to be retrieved
    result = cv_client.analyze(
        image_data=image_data,
        visual_features=[
            VisualFeatures.CAPTION,
            VisualFeatures.DENSE_CAPTIONS,
            VisualFeatures.TAGS,
            VisualFeatures.OBJECTS,
            VisualFeatures.PEOPLE
        ],
        language="en",
        gender_neutral_caption=True  # Optional
    )

    if result is not None:  # Check if the result is valid
        # Get image captions
        if result.caption is not None:
            print("\nCaption:")
            print(" Caption: '{}' (confidence: {:.2f}%)".format(result.caption.text, result.caption.confidence * 100))

        # Get image dense captions
        if result.dense_captions is not None:
            print("\nDense Captions:")
            for caption in result.dense_captions['values']:
                print(" Caption: '{}' (confidence: {:.2f}%)".format(caption['text'], caption['confidence'] * 100))

        # Get image tags
        if result.tags is not None:
            print("\nTags:")
            for tag in result.tags['values']:
                print(" Tag: '{}' (confidence: {:.2f}%)".format(tag['name'], tag['confidence'] * 100))

        # Get objects in the image
        if result.objects is not None:
            print("\nObjects in image:")

            # Prepare image for drawing
            image = Image.open(image_file)
            fig = plt.figure(figsize=(image.width/100, image.height/100))
            plt.axis('off')
            draw = ImageDraw.Draw(image)
            color = 'cyan'

            for detected_object in result.objects['values']:
                # Print object details
                bounding_box = detected_object['boundingBox']
                tags = detected_object['tags']

                if tags:
                    tag = tags[0]  # Assuming the first tag is the most relevant
                    print(" Object: '{}' (confidence: {:.2f}%)".format(tag['name'], tag['confidence'] * 100))

                # Draw object bounding box
                box = ((bounding_box['x'], bounding_box['y']),
                       (bounding_box['x'] + bounding_box['w'], bounding_box['y'] + bounding_box['h']))
                draw.rectangle(box, outline=color, width=3)
                if tags:
                    plt.annotate(tag['name'], (bounding_box['x'], bounding_box['y']), backgroundcolor=color)

            # Save annotated image
            plt.imshow(image)
            plt.tight_layout(pad=0)
            outputfile = 'AZURE-AI-Vision/objects.jpg'
            fig.savefig(outputfile)
            print('  Results saved in', outputfile)

        # Get people in the image
        if result.people is not None:
            print("\nPeople in image:")

            # Prepare image for drawing
            image = Image.open(image_file)
            fig = plt.figure(figsize=(image.width/100, image.height/100))
            plt.axis('off')
            draw = ImageDraw.Draw(image)
            color = 'cyan'

            for detected_person in result.people['values']:
                bounding_box = detected_person['boundingBox']
                confidence = detected_person['confidence']

                if confidence * 100 > 75:  # Only show persons with confidence above 75%
                    print(" Person detected with confidence: {:.2f}%".format(confidence * 100))
                    print(" Bounding Box: x={}, y={}, w={}, h={}".format(
                        bounding_box['x'], bounding_box['y'], bounding_box['w'], bounding_box['h']))

                    # Draw bounding box
                    box = ((bounding_box['x'], bounding_box['y']),
                           (bounding_box['x'] + bounding_box['w'], bounding_box['y'] + bounding_box['h']))
                    draw.rectangle(box, outline=color, width=3)

            # Save annotated image
            plt.imshow(image)
            plt.tight_layout(pad=0)
            outputfile = 'AZURE-AI-Vision/people.jpg'
            fig.savefig(outputfile)
            print('  Results saved in', outputfile)

    else:
        error_details = "ErrorDetails"  # Replace with the correct class or method to extract error details
        print(" Analysis failed.")
        print("   Error details could not be retrieved.")



if __name__ == "__main__":
    asyncio.run(main())