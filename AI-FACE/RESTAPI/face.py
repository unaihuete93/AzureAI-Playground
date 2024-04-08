# Python app calling Azure AI Face service using latest SDK
# The app will offer detect, identify and verify functionalities

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
ENDPOINT = "https://ai102faceunai.cognitiveservices.azure.com/"

# Base url for the Verify and Facelist/Large Facelist operations
IMAGE_BASE_URL = 'https://raw.githubusercontent.com/Azure-Samples/cognitive-services-sample-data-files/master/Face/images/'

# Create an authenticated FaceClient.
face_client = FaceClient(ENDPOINT, CognitiveServicesCredentials(KEY))

# Detect a face 
image1_url = 'https://th.bing.com/th/id/R.af8213b382b4ba18fd237f9d2c8ae956?rik=TsVObSO%2bIxq0ag&pid=ImgRaw&r=0'

def detect_face(face_client, url):
    detected_faces = face_client.face.detect_with_url(url)
    if not detected_faces:
        raise Exception('No face detected from image')
    print (f'Face detected with id: {detected_faces[0].face_id}')
    return detected_faces[0].face_id

def verify_face(face_client, face_id1, face_id2):
    verify_result_same = face_client.face.verify_face_to_face(face_id1, face_id2)
    return verify_result_same.is_identical, verify_result_same.confidence

def identify_face(face_client, faceid, person_group_id):
    identify_result = face_client.face.identify([faceid], person_group_id)
    print (f'Identify result: {identify_result}')

if __name__ == "__main__":
    print("Select an option:")
    print("1. Detect face")
    print("2. Verify faces")
    # Trained using Rest API calls
    print("3. Identify (based on trained Person Group)")
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
    # elif option == 3:
    #     personGroupId = "real-sociedad" #change if needed
    #     image1_url = str(input("Provide image URL face to identify: "))
    #     face_id1 = detect_face(face_client, image1_url)
        
    #     identify_result = identify_face(face_client, face_id1, personGroupId)
        
        
    else:
        print("Invalid option")



