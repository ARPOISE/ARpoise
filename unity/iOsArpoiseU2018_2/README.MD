![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise -iOS- App

## Overview
This directory contains the Unity 3D project used to build the iOs ARpoise iOS App with Unity 3D version 2018.2.2.
If you clone and open this project with Unity 3D, version 2018.2.2, you should be able to build ARpoise.

The ARpoise iOS App has been submitted to the Apple App Store. You do not need to build it yourself.

## Functionality
- Connects to the ARpoise-Directory front-end and supplies the location of the user's device.

- Receives the response from the ARpoise-Directory service.

- If there are two or more layers available at the user's location, the list of layers is shown to the user
  in order for the user to select a layer to be shown.
  
- If there is exactly one layer, this layer is selected, or if there is no layer at all, a default layer is selected.

- Connects to the porpoise service of the selected layer and downloads the list of **POI**s of the layer.

- Downloads the Unity asset bundle of each **POI** of the layer and loads the **POI**'s Unity prefab from the asset bundle.

- Places the **POI**s' Unity prefabs into the 3D scene and shows the camera image as the background of the scene.
