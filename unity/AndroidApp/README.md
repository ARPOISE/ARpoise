![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise -Mobile- Client App

## Overview
The ARpoise Client App has been submitted to the Google Play Store and the Apple App Store. 
You do not need to build it yourself.

## Functionality
- Connects to the ARpoise-Directory Frontend and supplies the location of the user's device.

- Receives back the porpoise url and name of the layer closest to the user or of the default layer.

- Connects to the porpoise of the layer and downloads the list of **POI**s of the layer.

- Downloads the Unity AssetBundle of each **POI** and loads the **POI**'s Unity prefab from the asset bundle.

- Places the **POI**s into the 3D scene and shows the camera image as the background of the scene.

## Building the App
In order to build the app you should, 

- get a version of Unity, including Android and iOS build capabilities and Vuforia. We tested it on 2018.2.2 and 2018.3.3.
- start a new Unity project and replace the main camera in the default scene with a Vuforia ArCamera.
- make the app work! So you can see the camera image when running on your test device, note that emulators will not work!
- close Unity.
- copy the assets from this repository to your assets folder.
- start Unity again.
- replace your scene with the ArScene from the assets.
- once more, make the app work! So you can see the camera image and some artifacts or messages from ARpoise.
