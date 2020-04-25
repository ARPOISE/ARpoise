![ARpoise Logo](/images/arvos_logo-sprite_rounded128sq.png)
# AR-vos Augmented Reality App: Examples

## Overview

**AR-vos** is an open source app that can do both **geolocative** and **image trigger** AR. It is part of the [**ARpoise** (**A**ugmented **R**eality **p**oint **o**f **i**nterest **s**ervice **e**nvironment)](http://arpoise.com/) open-source Augmented Reality platform.

**Download the AR-vos app** from the [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARvos) Store and the Apple [App Store](https://apps.apple.com/us/app/ar-vos/id1483218444). 

- **Image trigger AR** uses A.I. computer vision technology to recognize images (e.g. posters, postcards, or even photos of an outdoor facade, etc.) and place augments relative to those images. 
  - It uses Apple's ARKit and Google Android's ARCore technologies, which only run on newer models. (See [ARKit supporting iOS devices](https://developer.apple.com/library/archive/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html) and [ARCore supporting Android devices](https://developers.google.com/ar/discover/supported-devices).)
  - Note that if lighting conditions vary, for instance cast shadows on outside trigger images, you should set up the same augment with multiple trigger images for the different lighting conditions.

- **Geolocative AR** is an older technology that uses the GPS coordinates of the augments as the sole way to determine where an augment is viewable. Due to the imprecision of civilian GPS systems in smartphones, the location and orientation of the augments can vary by +/- 20 meters. The older ARpoise app can currently only do geolocative AR, but runs on a very large variety of older smartphones. We expect to merge the two client apps soon, as the older smartphones become defunct. 

- **Geofencing:** Even image trigger augments can be restricted to a certain area if desired. Therefore the AR-vos app always checks your device's GPS coordinates and send a request to the **ARpoise** back end server to see whether there are specific art works at your location. If there are you will see them; if there are none, a default layer will be shown to you. We will change this default layer periodically.


## Example Image Triggers
The layer 'Tamiko Thiel's AR' is currently the default AR-vos layer, but just ignore this fact for now!

In order to see the triggered augment layers, you have to

- Start **AR-vos** and let the default layer load, your screen should look like this:

![AR-vosExamples1](/images/AR-vosExamples1_600h.png)

. 

- Now point your device at the **AR-vos** logo, which we have set up as an example trigger.

![AR-vos Logo](/images/arvos_logoOnWhite_800x600.png)

The following example augment should appear - some colorful animated cubes:

![AR-vosExamples2](/images/AR-vosExamples2a_800w.png)

. 

- Now point your device at this example trigger, the Japanese character for "Zen":

.

![AR-vos Logo](/images/arvos_ZenOnWhite_800x600.png)

.

The lotus meditaion animation should appear:

![AR-vosExamples3](/images/AR-vosExamples3.PNG)

- The trigger image for King Ludwig.

![AR-vos Logo](https://www.arpoise.com/TI/flag.jpg)

King Ludwig should appear:

![AR-vosExamples4](/images/AR-vosExamples4.PNG)


