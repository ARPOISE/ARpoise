![ARpoise Logo](/images/arvos_logo_rgb-weiss128.png)
# AR-vos -iOS- App

## Overview
This directory contains the Unity 3D project used to build the iOS AR-vos Client App with Unity 3D version 2018.3.14.
If you clone and open this project with Unity 3D, version 2018.3.14, you should be able to build AR-vos.

The AR-vos -iOS- App has been submitted to the Apple App Store. 
You do not need to build it yourself.

## Functionality
- Connects to the ARpoise-Directory front-end and supplies the location of the user's device.

- AR-vos currently only has one default layer shown everywhere in the world.

- Downloads the list of **POI**s of the default layer.

- Downloads the trigger image for each **POI** and registers the image with iOS's ARKit as image trigger.

- Downloads the Unity asset bundle of each **POI** of the layer and loads the **POI**'s Unity prefab from the asset bundle.

- Displays a 'Fit the image you're scanning' frame and shows the camera image as the background of the frame.

- Once iOS's ARKit reports back that an trigger image has been found in the camera view, the app shows the Unity prefab of the **POI** at the image trigger location.
