![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise Mobile Client App

## Overview
This app has been submitted to the Google Play Store and the Apple App Store. 
You do not need to build it yourself.

## Functionality
- Connects to the ARpoise Directory Front end and supplies the user's location.

- Receives back the porpoise url and name of the closest layer or the default layer.

- Connects to the porpoise of the layer and downloads the list of **POI**s of the layer.

- Downloads the Unity AssetBundle of each **POI** and loads the **POI**s Unity prefab from the asset bundle.

- Places the **POI** into the 3D scene and shows the camera image as the background of the scene.

## Building the App
In order to build the app you should 

- get a version of Unity, we tested it on 2018.2.2 and 2018.3.3.
- start with a new Unity project and replace the main camera in the scene with a Vuforia ArCamera.
- make the app work, so you can see the camera image when running on your test device.
- close Unity.
- copy the assets from here to your assets folder.
- start Unity again.
- replace your scene with the ArScene from the assets.
