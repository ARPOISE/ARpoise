# ![ARpoise Logo](/images/arvos_logo-sprite_rounded128sq.png) AR-vos Augmented Reality App
# Image Trigger and SLAM AR Examples

## Overview

**AR-vos** is an open source app that can do **geolocative**, **SLAM** and **image trigger** AR. It is part of the [**ARpoise** (**A**ugmented **R**eality **p**oint **o**f **i**nterest **s**ervice **e**nvironment)](http://arpoise.com/) open-source Augmented Reality platform.

**Download the AR-vos app** from the [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARvos) Store and the Apple [App Store](https://apps.apple.com/us/app/ar-vos/id1483218444). 

- **Image trigger AR** uses A.I. computer vision technology to recognize images (e.g. posters, postcards, or even photos of an outdoor facade, etc.) and place augments relative to those images. 
  - It uses Apple's ARKit and Google Android's ARCore technologies, which only run on newer models.
  - For iPhones, it only works on the iPhone SE or iPhone **6s** (NOT iPhone 6) and higher. See [ARKit supporting iOS devices](https://developer.apple.com/library/archive/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html).
  - For Android it is harder to say, please see [ARCore supporting Android devices](https://developers.google.com/ar/discover/supported-devices).
  - Note that if lighting conditions vary, for instance cast shadows on outside trigger images, you should set up the same augment with multiple trigger images for the different lighting conditions.

- **SLAM based AR** employs [Simultaneous localization and mapping](https://en.wikipedia.org/wiki/Simultaneous_localization_and_mapping) technology to discover planes in the physical space around the user, and then  place augments on these planes. The augments will pretty much stay in place, and the user can then walk around them in 3D space.
  - It uses Apple's ARKit and Google Android's ARCore technologies, which only run on newer models.
  - For iPhones, it only works on the iPhone SE or iPhone **6s** (NOT iPhone 6) and higher. See [ARKit supporting iOS devices](https://developer.apple.com/library/archive/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html).
  - For Android it is harder to say, please see [ARCore supporting Android devices](https://developers.google.com/ar/discover/supported-devices).
  
- **Geolocative AR** is an simpler technology that uses the GPS coordinates of the augments as the sole way to determine where an augment is viewable. Due to the imprecision of civilian GPS systems in smartphones, the location and orientation of the augments can vary by +/- 20 meters. The ARpoise app can only do geolocative AR, but runs on a very large variety of older smartphones. We expect to merge the two client apps soon, as the older smartphones become defunct. 

- **Geofencing:** Image trigger and SLAM augments can be restricted to a certain area if desired. Therefore the AR-vos app always checks your device's GPS coordinates and sends a request to the **ARpoise** back end server to see whether there are specific art works at your location. If there are you will see them; if there are none, a default layer will be shown to you. We will change this default layer periodically.

## Example Image Triggers

In order to see the triggered augment layers, start **AR-vos** and select "Tamiko Thiel's AR" layer with the example triggers. You should then get a frame as in the image below:

. 

![AR-vosExamples1](/images/AR-vosExamples1a_800w.png)

. 

- Now point your device at the **AR-vos** logo, which we have set up as an example trigger.

![AR-vos Logo](/images/AR-vosExamples2a_logo800x600.png)

The following example augment should appear - some colorful animated cubes:

![AR-vosExamples2](/images/AR-vosExamples2a_800w.png)

. 

- Now point your device at this example trigger, the Japanese character for "Zen":

.

![Zen Kanji](/images/AR-vosExamples3a_Zen800x600.png)

.

A mandala with [Eisai](https://en.wikipedia.org/wiki/Eisai) should appear: the Japanese monk who brought Zen (and green tea!) to Japan.

![AR-vosExamples3](/images/AR-vosExamples3a_800w.png)

. 

- Now try this doorway, which we set as a trigger image for King Ludwig II of Bavaria. 
  - Note that this is a north-facing facade, and therefore has constant lighting with no shadows. 
  - Also, although there are reflections on the glass panes, they are small relative to the rest of the doorway and therefore should not disturb the overall reading of the trigger image.

. 

![doorwayExample4](/images/AR-vosExamples4a_doorTrigger_800h.png)

. 

The [Mad King Ludwig II](https://en.wikipedia.org/wiki/Ludwig_II_of_Bavaria) should appear, larger than life! 

. 

![AR-vosExamples4](/images/AR-vosExamples4a_800h.png)


## SLAM Example
The layer 'SLAM Boxes' is currently one of the default AR-vos layers.

- Tap the AR-vos logo once to go back to the layers list, or restart **AR-vos** completely and let the default layer list load.

- From the layer list select 'SLAM Boxes' and let it load, your screen should look like this:

![AR-vosExamples1](/images/SLAM_1_800h.PNG)

. 

- Now move your device until it detects a plane. (If you are on Android, it might look slightly different - a grid without the blue frame.)

![AR-vos Logo](/images/SLAM_2_800h.PNG)

. 

- Every time you tap on a plane, more colorful animated cubes appear.
- If you instead tap on the yellow center cube of a group of cubes, the group will start to spin.

![AR-vosExamples2](/images/SLAM_3_800h.PNG)

 .

The SLAM technology will make sure the boxes stay where you put them by tapping. So you can walk around the cubes and see them from the other side.





