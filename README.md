![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise - *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment

## Overview
ARpoise is an open-source Augmented Reality service environment allowing to view location based AR content created in
[Unity](http://unity3d.com). Client applications for Android, 
[see the Play Store](http://play.google.com/store/apps/details?id=com.arpoise.ARpoise),
and iOS,
[see the App Store](https://www.apple.com/lae/ios/app-store/),
have been implemented.

The goal of ARpoise is to provide an open-source community based replacement for the 
[lay ar](https://www.layar.com/)
app.

## Functionality

## Components

## Restrictions
- The current client implementations do not have any user interface. The client simply shows the content served by the back-end.

- Only one ARpoise layer can be show at any location in the world.
If there is at least one ARpoise layer placed within 1500 meters of the client location, the nearest one of those is shown.
if there is no layer within 1500 meters of the client location, a default layer is shown.
urrently
[Tamiko Thiel's *Reign of Gold*](http://tamikothiel.com/AR/reign-of-gold.html).

- Adding, removing and placing layers within ARpoise is an email-based process involving the administrators of
[www.arpoise.com](http://www.arpoise.com).
