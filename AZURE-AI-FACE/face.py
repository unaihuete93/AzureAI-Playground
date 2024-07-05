# Python app calling Azure AI Face service using latest SDK
# The app will offer detect, identify and verify functionalities
# Identify training done using REST API calls (other file)

import asyncio
import io
import os
import sys
import time
import uuid
import requests
from urllib.parse import urlparse
from io import BytesIO
# To install this module, run:
# python -m pip install Pillow
from PIL import Image, ImageDraw
from azure.cognitiveservices.vision.face import FaceClient
from msrest.authentication import CognitiveServicesCredentials
from azure.cognitiveservices.vision.face.models import TrainingStatusType, Person, QualityForRecognition


# This key will serve all examples in this document.
KEY = os.environ["AIFACE_SECRET"]

# This endpoint will be used in all examples in this quickstart.
ENDPOINT = os.environ["AIFACE_END"]

# Base url for the Verify and Facelist/Large Facelist operations
IMAGE_BASE_URL = 'https://raw.githubusercontent.com/Azure-Samples/cognitive-services-sample-data-files/master/Face/images/'

# Create an authenticated FaceClient.
face_client = FaceClient(ENDPOINT, CognitiveServicesCredentials(KEY))

# Detect a face 
image1_url = 'https://th.bing.com/th/id/R.af8213b382b4ba18fd237f9d2c8ae956?rik=TsVObSO%2bIxq0ag&pid=ImgRaw&r=0'

def detect_face(face_client, url):
    # We use detection model 3 to get better performance, recognition model 4 to support quality for recognition attribute.
    # SAME AS USED ON PERSONGROUP TRAINING!!
    detected_faces = face_client.face.detect_with_url(url,detection_model='detection_03', recognition_model='recognition_04')
    if not detected_faces:
        raise Exception('No face detected from image')
    print (f'Face detected with id: {detected_faces[0].face_id}')
    return detected_faces[0].face_id

def verify_face(face_client, face_id1, face_id2):
    verify_result_same = face_client.face.verify_face_to_face(face_id1, face_id2)
    return verify_result_same.is_identical, verify_result_same.confidence


def identify_face(face_client, faceid, person_group_id):
    face_ids = [faceid]
    identify_result = face_client.face.identify(face_ids, person_group_id)
    if not identify_result:
        print('No person identified in the person group')
    for identifiedFace in identify_result:
        if len(identifiedFace.candidates) > 0:
            # print personid found
            print(f'PersonId identified: {identifiedFace.candidates[0].person_id}')
            # Get person name
            person = face_client.person_group_person.get(person_group_id, identifiedFace.candidates[0].person_id)
            print(f'Person name: {person.name}')
    return identify_result

if __name__ == "__main__":
    
    
    while True:
        print("Select an option:")
        print("1. Detect face")
        print("2. Verify faces")
        print("3. Identify (based on trained Person Group)")
        print("4. Exit")
        option = int(input("Enter your option: "))

        if option == 1:
            image1_url = str(input("Provide image URL to detect face: "))
            face_id = detect_face(face_client, image1_url)

        elif option == 2:
            image1_url = str(input("Provide image URL to detect face 1: "))
            face_id1 = detect_face(face_client, image1_url)
            image1_url = str(input("Provide image URL to detect face 2: "))
            face_id2 = detect_face(face_client, image1_url)
            is_identical, confidence = verify_face(face_client, face_id1, face_id2)
            print(f'Faces are identical: {is_identical}')
            print(f'Confidence: {confidence}')

        elif option == 3:
            personGroupId = "real-sociedad" #change if needed
            image1_url = str(input("Provide image URL face to identify: "))
            face_id1 = detect_face(face_client, image1_url)
            identify_result = identify_face(face_client, face_id1, personGroupId)

        elif option == 4:
            print("Exiting the application.")
            break

        else:
            print("Invalid option")
            
# TEST pictures

# Martin https://estaticosgn-cdn.deia.eus/clip/b6135d22-af11-4aee-b485-861fa9e668be_16-9-discover-aspect-ratio_default_0.jpg


