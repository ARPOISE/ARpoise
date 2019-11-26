![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment

## Overview
ARpoise is an open-source Augmented Reality service environment that allows AR content designers to create and distribute and AR users to view location based AR content that is created in [Unity](http://unity3d.com). 

Two different client applications have both been implemented for Android and iOS.

- The **ARpoise** client is a geolocative AR app running on most available phones.
The **ARpoise** app can be downloaded from the [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARpoise) Store and the Apple [App Store](https://itunes.apple.com/de/app/arpoise/id1451460843). The source code of the apps is available [here](unity/).

- The **AR-vos** client is an image trigger and a geolocative AR app using ARKit on iOS and ARCore on Android.
The **AR-vos** app can be downloaded from the [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARvos) Store and the Apple [App Store](https://apps.apple.com/us/app/ar-vos/id1483218444). The source code of the apps is available [here](unity/).

The goal of ARpoise is to provide an open-source, community-supported, location-based AR app as a replacement for the 
[Layar app](https://www.layar.com/) and other commercial geolocative AR platforms.

## Getting Started
If you are a content creator wanting to use **ARpoise** or **AR-vos** to deliver your own 3D content,
you should start by reading this document.
As you also will have to run you own version of the **ARpoise-PorPOISe Back End** you should also read
the documentation [/php/porpoise](php/porpoise/README.md) for it.

## Functionality
- Content designers can create image trigger or location-based AR experiences in Unity3d and add them as project layers to a general directory. This AR experiences can either be visible all over the world, or geo-fenced to be viewable only within areas defined by specific GPS coordinates.

- These AR experiences can be viewed by anyone who downloads one of the free client apps, **ARpoise** or **AR-vos**, onto Android and iOS devices. Since the AR experiences are individual project layers within an existing app, content designers do not have to create individual apps for each project and bring each project into the App and Play stores themselves.

- Artist define layers of points of interest, (**POI**)s, can be added to,
removed from, or placed within the world coordinates of ARpoise dynamically.

- The 3D content visible in ARpoise is independent of the app downloaded from the Google Play Store or the Apple App Store.
The **POI**s specified in an ARpoise layer definition contain the URL of an Unity AssetBundle.
When showing a **POI**, the ARpoise app downloads this asset bundle
and loads the 3D Unity prefab of the **POI** from it.

- Unity-created 3D content of ARpoise can include your animations. Furthermore, **POI**s can have
ARpoise level animations like scale, rotation, and transformation to be started
'on create', 'on focus', 'in focus', or 'on click' of a **POI**.

- Image trigger based **POI**s are shown to the user once the app discovers a trigger image.

- Geolocative **POI**s can have an absolute world location defined by longitude and latitude
or they can have a position relative to the user's device defined in meters of distance in the X, Y, and Z directions.
**POI**s with an absolute location can additionally be confined into an area around the user,
so they will always be around the user.

- **POI**s can be added to and removed from an ARpoise layer via a PHP based web interface similar to the
[PorPOISe for Layar](https://code.google.com/archive/p/porpoise/) web service.
The placement of individual **POI**s can be performed through a Google-maps-based click-and-drag map interface.
The ARpoise version of PorPOISe converts your data sets of **POI**s into responses to the ARpoise client.
Things like JSON formatting and distance calculation are all done for you. ARpoise supports XML as data stores.

- One or more ARpoise layers can be shown at any location in the world.
If there are two or more ARpoise layers located within the range of the client's location,
a list of all available layers is shown to the user for selecting a layer.
If there is exactly one ARpoise layer located within the range, this layer is shown to the user.
If there is no layer within the range of the client's location, the default layer is shown.
Currently Tamiko Thiel's
[*Lotus Meditation*](http://www.tamikothiel.com/AR/lotus-meditation.html).
## Components
### iOS- and Android-Client Application
See [unity](unity/).
This folder contains Unity projects for the different clients, **ARpoise** and **AR-vos**.
### ARpoise-Directory Front End
See [ARpoiseDirectory](ArpoiseDirectory).
### ARpoise-Directory Back End
See [/php/dir](php/dir/README.md).
### ARpoise-PorPOISe Back End
See [/php/porpoise](php/porpoise/README.md).

## Restrictions
- Unity behaviour scripts written in C# cannot be included in Unity-created 3D content of ARpoise.

- Adding, removing and placing layers within the ARpoise-Directory is an email-based process involving the administrators of
[www.arpoise.com](http://www.arpoise.com).
