![ARpoise Logo](/images/arvos_logo_rgb-weiss128.png)
# AR-vos -iOS- App

## Overview
This directory contains the Unity 3D project used to build the **AR-vos** -iOS- app with Unity 3D version 2018.4.23.
If you clone and open this project with Unity 3D, version 2018.4.23, you should be able to build **AR-vos**.

The **AR-vos** -iOS- app has been submitted to the Apple App Store. You do not need to build it yourself.

## Functionality
- Connects to the ARpoise-Directory front-end and supplies the location of the user's device.

- Receives the response from the ARpoise-Directory service.

- If there are two or more layers available at the user's location, the list of layers is shown to the user
  in order for the user to select a layer to be shown.
  
- If there is exactly one layer, this layer is selected, or if there is no layer at all, a default layer is selected.

- Connects to the porpoise service of the selected layer and downloads the list of **POI**s of the layer.

- The **AR-vos** -iOS- app can handle image trigger based layers, SLAM layers and geolocative layers.

- The app downloads the Unity asset bundle of each **POI** of the layer and loads the **POI**'s Unity prefab from the asset bundle if needed.

- For image trigger based layers, the app downloads the trigger image for each **POI** and registers the image with iOS's ARKit as image trigger.

  - Then the app displays a 'Fit the image you're scanning' frame and shows the camera image as the background of the frame.

  - Once iOS's ARKit reports back that an trigger image has been found in the camera view, the app shows the Unity prefab of the **POI** at the image trigger location.

- If a SLAM layer is selected, the app tries to detect vertical and horizontal planes in the environment. Once a plane ist detected, the user can place objects on it by tapping on the plane.
