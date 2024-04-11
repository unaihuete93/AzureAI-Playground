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

# Import namespaces
import azure.ai.vision as sdk


def main():
    global cv_client

    try:
        # Get Configuration Settings
        #load_dotenv()
        ai_endpoint = os.getenv('AIVISION_ENDPOINT')
        ai_key = os.getenv('AIVISION_KEY')

        # Get image
        image_file = 'images/Oyarzabal2.jpg'
        if len(sys.argv) > 1:
            image_file = sys.argv[1]

        # Authenticate Azure AI Vision client
        cv_client = sdk.VisionServiceOptions(ai_endpoint, ai_key)
        
        # Analyze image
        AnalyzeImage(image_file, cv_client)

        # Generate thumbnail
        BackgroundForeground(image_file, cv_client)

    except Exception as ex:
        print(ex)


def AnalyzeImage(image_file, cv_client):
    print('\nAnalyzing', image_file)

    # Specify features to be retrieved
    analysis_options = sdk.ImageAnalysisOptions()

    features = analysis_options.features = (
        sdk.ImageAnalysisFeature.CAPTION |
        sdk.ImageAnalysisFeature.DENSE_CAPTIONS |
        sdk.ImageAnalysisFeature.TAGS |
        sdk.ImageAnalysisFeature.OBJECTS |
        sdk.ImageAnalysisFeature.PEOPLE
    )
        


    # Get image analysis
    # Get image analysis
    image = sdk.VisionSource(image_file)

    image_analyzer = sdk.ImageAnalyzer(cv_client, image, analysis_options)

    result = image_analyzer.analyze()

    if result.reason == sdk.ImageAnalysisResultReason.ANALYZED:
        # Get image captions
        if result.caption is not None:
            print("\nCaption:")
            print(" Caption: '{}' (confidence: {:.2f}%)".format(result.caption.content, result.caption.confidence * 100))

        # Get image dense captions
        if result.dense_captions is not None:
            print("\nDense Captions:")
            for caption in result.dense_captions:
                print(" Caption: '{}' (confidence: {:.2f}%)".format(caption.content, caption.confidence * 100))

        # Get image tags
        if result.tags is not None:
            print("\nTags:")
            for tag in result.tags:
                print(" Tag: '{}' (confidence: {:.2f}%)".format(tag.name, tag.confidence * 100))

        # Get objects in the image
        # Get objects in the image
        if result.objects is not None:
            print("\nObjects in image:")

            # Prepare image for drawing
            image = Image.open(image_file)
            fig = plt.figure(figsize=(image.width/100, image.height/100))
            plt.axis('off')
            draw = ImageDraw.Draw(image)
            color = 'cyan'

            for detected_object in result.objects:
                # Print object name
                print(" {} (confidence: {:.2f}%)".format(detected_object.name, detected_object.confidence * 100))
                
                # Draw object bounding box
                r = detected_object.bounding_box
                bounding_box = ((r.x, r.y), (r.x + r.w, r.y + r.h))
                draw.rectangle(bounding_box, outline=color, width=3)
                plt.annotate(detected_object.name,(r.x, r.y), backgroundcolor=color)

            # Save annotated image
            plt.imshow(image)
            plt.tight_layout(pad=0)
            outputfile = 'objects.jpg'
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

        for detected_people in result.people:
            # Draw object bounding box
            r = detected_people.bounding_box
            bounding_box = ((r.x, r.y), (r.x + r.w, r.y + r.h))
            draw.rectangle(bounding_box, outline=color, width=3)

            # Return the confidence of the person detected
            #print(" {} (confidence: {:.2f}%)".format(detected_people.bounding_box, detected_people.confidence * 100))
            
        # Save annotated image
        plt.imshow(image)
        plt.tight_layout(pad=0)
        outputfile = 'people.jpg'
        fig.savefig(outputfile)
        print('  Results saved in', outputfile)

    else:
        error_details = sdk.ImageAnalysisErrorDetails.from_result(result)
        print(" Analysis failed.")
        print("   Error reason: {}".format(error_details.reason))
        print("   Error code: {}".format(error_details.error_code))
        print("   Error message: {}".format(error_details.message))


def BackgroundForeground(image_file, cv_client):
    print('\n')
    
    # Remove the background from the image or generate a foreground matte
    # Remove the background from the image or generate a foreground matte
    print('\nRemove the background from the image or generate a foreground matte')

    image = sdk.VisionSource(image_file)

    analysis_options = sdk.ImageAnalysisOptions()

    # Set the image analysis segmentation mode to background or foreground
    analysis_options.segmentation_mode = sdk.ImageSegmentationMode.BACKGROUND_REMOVAL
    #analysis_options.segmentation_mode = sdk.ImageSegmentationMode.FOREGROUND_MATTING
        
    image_analyzer = sdk.ImageAnalyzer(cv_client, image, analysis_options)

    result = image_analyzer.analyze()

    if result.reason == sdk.ImageAnalysisResultReason.ANALYZED:

        image_buffer = result.segmentation_result.image_buffer
        print(" Segmentation result:")
        print("   Output image buffer size (bytes) = {}".format(len(image_buffer)))
        print("   Output image height = {}".format(result.segmentation_result.image_height))
        print("   Output image width = {}".format(result.segmentation_result.image_width))

        output_image_file = "newimage.jpg"
        with open(output_image_file, 'wb') as binary_file:
            binary_file.write(image_buffer)
        print("   File {} written to disk".format(output_image_file))

    else:

        error_details = sdk.ImageAnalysisErrorDetails.from_result(result)
        print(" Analysis failed.")
        print("   Error reason: {}".format(error_details.reason))
        print("   Error code: {}".format(error_details.error_code))
        print("   Error message: {}".format(error_details.message))
        print(" Did you set the computer vision endpoint and key?")


if __name__ == "__main__":
    main()