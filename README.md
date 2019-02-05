![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise - *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment

## Overview
ARpoise is an open-source Augmented Reality service environment allowing
to distribute and view location based AR content created in
[Unity](http://unity3d.com). Client applications for Android, 
see the [Google Play Store](http://play.google.com/store/apps/details?id=com.arpoise.ARpoise),
and iOS,
see the [Apple App Store](https://www.apple.com/lae/ios/app-store/),
have been implemented.

The goal of ARpoise is to provide an open-source, community-supported, location-based AR App as a replacement for the 
[layar app](https://www.layar.com/).

## Functionality
- Location-based, Unity-created AR-content can be shown on Android and iOS devices at any location in the world.

- The 3D content visible in ARpoise is independent of the ARpoise app download from the
[Play Store](http://play.google.com/store/apps/details?id=com.arpoise.ARpoise)
or the
[App Store](https://www.apple.com/lae/ios/app-store/).
The point of interrests, (**POI**)s, specified in an ARpoise layer definition contain the URL of an
Unity AssetBundle.
When showing the **POI** the ARpoise app downloads this asset bundle
and loads the 3D Unity prefab of the **POI** from it.

- Unity-created 3D content of ARpoise can include all your scripts and animations. Furthermore **POI**s can have
ARpoise level animations like scale, rotation, and transformation to be started
'on load', 'on focus', or 'on click' of a **POI**.

- **POI**s can have an absolute world location defined by their longitude and latitude
or they can have a position relative to the user' device defined in meters of distance in the X, Y, and Z directions.
**POI**s with an absolute location can additionally be confined into an area around the user,
so they will always be around the user.

- **POI**s can be added to and removed from an ARpoise layer via a PHP based web interface similar to the
[PorPOISe for Layar](https://code.google.com/archive/p/porpoise/) web service.
The placement of individual **POI**s can be performed through a Google-maps-based click-and-drag map interface.
The ARpoise version of PorPOISe converts your data sets of **POI**s into responses to the ARpoise client.
Things like JSON formatting and distance calculation are all done for you. ARpoise supports XML as data stores.

## Components
### IOS and Android client application
See [unity/AndroidApp](unity/AndroidApp/).
The two applications do not differ in script code or assets. Therefore only one version is kept here on GitHub.
### ARpoise Directory FrontEnd
See [ARpoiseDirectory](ArpoiseDirectory/).
### ARpoise Directory Backend
See [/php/dir](php/dir/).
### ARpoise PorPOISe Backend
See [/php/porpoise](php/porpoise/).

## Restrictions
- The current client implementations do not have any user interface. The ARpoise application clients simply show the content served by the back-end.

- Only one ARpoise layer is shown at any location in the world.
If there is one or more ARpoise layers placed within 1500 meters of the client location, the nearest one of them is shown.
if there is no layer within 1500 meters of the client location, the default layer is shown.
Currently
[Tamiko Thiel's *Reign of Gold*](http://tamikothiel.com/AR/reign-of-gold.html).

- Adding, removing and placing layers within ARpoise is an email-based process involving the administrators of
[www.arpoise.com](http://www.arpoise.com).
