![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment

## Overview
ARpoise is an open-source Augmented Reality service environment that allows AR content designers to create and distribute AR experiences, and users to view location-based, image trigger or SLAM AR content that is created in [Unity](http://unity3d.com). The goal of ARpoise is to provide an open-source, community-supported AR app for artists and other people who might not be able to develop their own apps, and as an alternative to commercial AR platforms that may go out of business at any time without warning.

Two different client applications have both been implemented for Android and iOS.

- The **ARpoise** client is a geolocative AR app running on most available phones.
The **ARpoise** app can be downloaded from the [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARpoise) Store and the Apple [App Store](https://itunes.apple.com/de/app/arpoise/id1451460843). The source code of the apps is available [here](unity/).

- The **AR-vos** client app provides geolocative, image trigger and SLAM AR functionality, using ARKit on iOS and ARCore on Android. It therefore only runs on [ARKit supporting iOS devices](https://developer.apple.com/library/archive/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html) and [ARCore supporting Android devices](https://developers.google.com/ar/discover/supported-devices). We hope to merge the two client apps at some point, when the majority of smartphones support ARKit and ARCore technology. 
The **AR-vos** app can be downloaded from the [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARvos) Store and the Apple [App Store](https://apps.apple.com/us/app/ar-vos/id1483218444). The source code of the apps is available [here](unity/).

## Getting Started
If you are a content creator wanting to use **ARpoise** or **AR-vos** to deliver your own 3D content, you should start by reading this document. As you also will have to run you own version of the **ARpoise-PorPOISe Back End** you should also read the documentation for it in [/php/porpoise](php/porpoise/README.md).
Also see the [documentation](/documentation/README.md) folder and read the [Creating Asset Bundles](/documentation/CreatingAssetBundles.md) document.

## ARpoise Examples
The document [ARpoise-examples](/unity/ARpoise-examples.md) explains how to see some examples for **ARpoise**.
The examples include versions of the following pieces:
- [Reign of Gold](https://www.tamikothiel.com/AR/reign-of-gold.html) by Tamiko Thiel.
- [Evolution of Fish](https://www.tamikothiel.com/evolutionoffish/index.html) by Tamiko Thiel and /p.
- [Lotus Meditation](https://www.tamikothiel.com/AR/lotus-meditation.html) by Tamiko Thiel and /p.
- [Gardens of the Anthropocene](https://tamikothiel.com/gota/index.html) by Tamiko Thiel.

## AR-vos Examples
The document [AR-vos-examples](/unity/AR-vos-examples.md) explains how to see some examples for **AR-vos**.
The examples include various image triggered AR objects.

## Functionality
- Content designers can create SLAM, image trigger or location-based AR experiences in Unity3d and request that we add them as project layers to the general ARpoise directory. These AR experiences can either be visible all over the world, or geo-fenced to be viewable only within areas defined by specific GPS coordinates.

- These AR experiences can be viewed by anyone who downloads one of the free client apps, **ARpoise** or **AR-vos**, onto Android and iOS devices. Since the AR experiences are individual project "layers" within an existing app, content designers do not have to create individual apps for each experience, saving them the trouble of pushing their projects into the App and Play stores.

- The assets making up an AR experience viewable in ARpoise are downloaded from the content creator's web server as needed, rather than being part of the client app downloaded from the Google Play Store or the Apple App Store. The content creators have to build Unity3D asset bundles for Android and iOS containing their assets. See the document [Creating Asset Bundles](/documentation/CreatingAssetBundles.md).

- A layer containing an AR experience is made up of one or more individual augments or **POI**s (Points Of Interest). These are specified in an ARpoise layer definition containing the URL of an Unity AssetBundle. When showing a **POI**, the ARpoise app downloads this asset bundle and loads the 3D Unity prefab of the **POI** from it.

- Using the porPOIse back end, the functions available to content creators include dynamically adding **POI**s, removing them, placing them within world coordinates or relative to the user, and animations described in the next point.

- Unity-created 3D content of ARpoise can include animations in FBX files created in 3D modeling and animation programs. Furthermore, using the porPOIse back end, content creators can give **POI**s ARpoise level animations like scale, rotation, and transformation to be started 'on create', 'on focus', 'in focus', or 'on click' of a **POI**.

- Image trigger based **POI**s are shown to the user once the app discovers a trigger image. The image trigger files are also downloaded via an assigned asset bundle, and therefore do not need to be built into the client apps.

- SLAM based **POI**s are shown to the user once the app discovers a planes and the user taps on one of them.

- Geolocative **POI**s can have an absolute world location defined by longitude and latitude or they can have a position relative to the user's device defined in meters of distance in the X, Y, and Z directions. **POI**s with absolute locations can additionally be confined into an area around the user, so as the user moves through the real world, they will be dynamically moved to be within a given area around the user.

- **POI**s can be added to and removed from an ARpoise layer via a PHP based web interface similar to the [PorPOISe for Layar](https://code.google.com/archive/p/porpoise/) web service. The placement of individual **POI**s can be performed through a Google-maps-based click-and-drag map interface. The ARpoise version of PorPOISe converts your data sets of **POI**s into responses to the ARpoise client. Things like JSON formatting and distance calculation are all done for you. ARpoise supports XML as data stores.

- One or more ARpoise layers can be shown at any location in the world. If there are two or more ARpoise layers located within the range of the client's location, a list of all available layers is shown to the user to allow them to select a layer. If there is exactly one ARpoise layer located within the range, only this layer is shown to the user. If there is no layer within the range of the client's location, a list of default layers is shown.

## Components
### Content creation for iOS and Android Client Application
See [unity](unity/).
This folder contains Unity projects for the different clients, **ARpoise** and **AR-vos**.
### Content publishing and management with ARpoise PorPOISe Back End
See [/php/porpoise](php/porpoise/README.md).
### ARpoiseDirectory Front End (We manage this for you, unless you set up your own system.)
See [ARpoiseDirectory](ArpoiseDirectory).
### ARpoiseDirectory Back End (We manage this for you, unless you set up your own system.)
See [/php/dir](php/dir/README.md).


## Restrictions
- Unity behaviour scripts written in C# cannot be included in Unity-created 3D content of ARpoise.

- Unless you set up your own complete system, to add, remove and geolocate layers within the ARpoise-Directory you will need to contact the administrators of [www.arpoise.com](http://www.arpoise.com).
