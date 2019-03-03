![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment

## Overview
ARpoise is an open-source Augmented Reality service environment that allows AR content designers to create and distribute and AR users to view location based AR content that is created in [Unity](http://unity3d.com). Client applications have been implemented for Android in the 
[Google Play](http://play.google.com/store/apps/details?id=com.arpoise.ARpoise) Store, and iOS in the 
Apple [App Store](https://itunes.apple.com/de/app/arpoise/id1451460843).

The goal of ARpoise is to provide an open-source, community-supported, location-based AR App as a replacement for the 
[layar app](https://www.layar.com/).

## Functionality
- Your location-based, unity-created AR-content can be shown on Android and iOS devices at any location in the world.

- Artist definded layers of points of interest, (**POI**)s, can be added to,
removed from, or placed within the world coordinates of ARpoise dynamically.

- The 3D content visible in ARpoise is independent of the ARpoise app downloaded from the
[Google Play](http://play.google.com/store/apps/details?id=com.arpoise.ARpoise) Store
or the
Apple [App Store](https://itunes.apple.com/de/app/arpoise/id1451460843).
The **POI**s specified in an ARpoise layer definition contain the URL of an
Unity AssetBundle.
When showing a **POI**, the ARpoise app downloads this asset bundle
and loads the 3D Unity prefab of the **POI** from it.

- Unity-created 3D content of ARpoise can include your animations. Furthermore, **POI**s can have
ARpoise level animations like scale, rotation, and transformation to be started
'on load', 'on focus', or 'on click' of a **POI**.

- **POI**s can have an absolute world location defined by longitude and latitude
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
See [unity/AndroidApp](unity/AndroidApp/README.md).
The two applications do not differ in script code or assets. Therefore only one version is kept here on GitHub.
### ARpoise-Directory Front End
See [ARpoiseDirectory](ArpoiseDirectory/README.md).
### ARpoise-Directory Back End
See [/php/dir](php/dir/README.md).
### ARpoise-PorPOISe Back End
See [/php/porpoise](php/porpoise/README.md).

## Restrictions
- Unity behaviour scripts written in C# cannot be included in Unity-created 3D content of ARpoise.

- Adding, removing and placing layers within ARpoise is an email-based process involving the administrators of
[www.arpoise.com](http://www.arpoise.com).
