![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise - *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment

## Overview
ARpoise is an open-source Augmented Reality service environment allowing to view location based AR content created in
[Unity](http://unity3d.com). Client applications for Android, 
[see the Google Play Store](http://play.google.com/store/apps/details?id=com.arpoise.ARpoise),
and iOS,
[see the Apple App Store](https://www.apple.com/lae/ios/app-store/),
have been implemented.

The goal of ARpoise is to provide an open-source community based replacement for the 
[layar app](https://www.layar.com/).

## Functionality
- The 3D content shown in the ARpoise app is not included in the app download from the
[Play Store](http://play.google.com/store/apps/details?id=com.arpoise.ARpoise)
or the
[App Store](https://www.apple.com/lae/ios/app-store/).
The point of interrests (**POIS**) specified in an ARpoise layer definitions contain the URL of an
Unity AssetBundle.
When showing the **POI** the ARpoise app downloads this asset bundle
and loads the 3D Unity prefab of the **POI** from it.

- Unity created 3D content can include scripts and animations. Furthermore **POI**s allow also ARpoise level anymations like scale, rotation, and transformation to be started on load, on focus, or on click of a **POI**.

## Components

## Restrictions
- The current client implementations do not have any user interface. The client simply shows the content served by the back-end.

- Only one ARpoise layer is show at any location in the world.
If there is at least one ARpoise layer placed within 1500 meters of the client location, the nearest one of those is shown.
if there is no layer within 1500 meters of the client location, a default layer is shown.
Currently
[Tamiko Thiel's *Reign of Gold*](http://tamikothiel.com/AR/reign-of-gold.html).

- Adding, removing and placing layers within ARpoise is an email-based process involving the administrators of
[www.arpoise.com](http://www.arpoise.com).
