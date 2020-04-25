![ARpoise Logo](/images/arpoise_logo_rgb-128.png) ![ARpoise Logo](/images/arvos_logo-sprite_rounded128sq.png)
# AR-vos Augmented Reality App: Examples

## Overview

**AR-vos** is an open source app that can do both geolocative and image trigger AR. It is part of the [**ARpoise** (**A**ugmented **R**eality **p**oint **o**f **i**nterest **s**ervice **e**nvironment)](http://arpoise.com/) open-source Augmented Reality platform.

- **Image trigger AR** uses A.I. computer vision technology to recognize images (e.g. posters, postcards, or even photos of an outdoor facade, etc.) and place augments relative to those images. It uses Apple's ARKit and Google Android's ARCOre technologies, which only run on newer models. (See [ARKit supporting iOS devices](https://developer.apple.com/library/archive/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html) and [ARCore supporting Android devices](https://developers.google.com/ar/discover/supported-devices).)

We expect to merge the two client apps soon, as the older smartphones become defunct. The **AR-vos** app can be downloaded from the [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARvos) Store and the Apple [App Store](https://apps.apple.com/us/app/ar-vos/id1483218444). 

app, once you download, install and start it on your device, it will use your
device's GPS coordinates and send a request to the **ARpoise** back end to see whether there are specific art works
shown at you location. If there are specific artworks at your location, you will see them. If not, a default layer will
be shown to you. We will change this default layer periodically.


## Example Image Triggers
The layer 'Tamiko Thiel's AR' is currently the default layer.

In order to see the triggered objects, you have to

- Start **AR-vos** and let the default layer load, your screen should look like:

![AR-vosExamples1](/images/AR-vosExamples1.PNG)

- Now point the device at the different trigger images, first the **AR-vos** logo for an example layer for **AR-vos**.

![AR-vos Logo](https://www.arpoise.com/TI/arvos_logo_rgb-weiss1024.jpg)

The following example objects should appear:

![AR-vosExamples2](/images/AR-vosExamples2.PNG)

- The trigger image for lotus meditation.

![AR-vos Logo](https://www.arpoise.com/TI/zen_512sq.jpg)

The lotus meditaion animation should appear:

![AR-vosExamples3](/images/AR-vosExamples3.PNG)

- The trigger image for King Ludwig.

![AR-vos Logo](https://www.arpoise.com/TI/flag.jpg)

King Ludwig should appear:

![AR-vosExamples4](/images/AR-vosExamples4.PNG)


