![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise -Mobile- Client App

## This folder is actually obsolete, use the Unity project folders instead.

## Overview
The ARpoise Client App has been submitted to the Google Play Store and the Apple App Store. 
You do not need to build it yourself.

## Functionality
- Connects to the ARpoise-Directory front-end and supplies the location of the user's device.

- Receives the response from the ARpoise-Directory service.

- If there are two or more layers available at the user's location, the list of layers is shown to the user
  in order for the user to select a layer to be shown.
  
- If there is exactly one layer, this layer is selected, or if there is no layer at all, a default layer is selected.

- Connects to the porpoise service of the selected layer and downloads the list of **POI**s of the layer.

- Downloads the Unity asset bundle of each **POI** of the layer and loads the **POI**'s Unity prefab from the asset bundle.

- Places the **POI**s' Unity prefabs into the 3D scene and shows the camera image as the background of the scene.

## Building the App
If you want to build the app you should, 

- get a version of Unity, including Android and iOS build capabilities and Vuforia. We tested it on 2018.2.2 and 2018.3.3.
- start a new Unity project and replace the main camera in the default scene with a Vuforia ArCamera.
- change the Build properties so that the build either builds Android or iOS, depending on your goals.
- make the app work! So you can see the camera image when running on your test device. Note, emulators will not work!
- close Unity.
- copy the Assets from this repository to your local assets folder.
- start Unity again.
- replace your scene with the ArScene from the assets.
- once more, make the app work! So you can see the camera image and some artifacts or messages from ARpoise.
