![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise and AR-vos -Mobile- Apps

## Overview
This directory contains the Unity 3D projects used to build the **ARpoise** and **AR-vos** apps for Android and iOS.

## Downloads
- The **ARpoise** client is a geolocative AR app running on most available phones.
Download: [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARpoise) Store, Apple [App Store](https://itunes.apple.com/de/app/arpoise/id1451460843). The source code for the apps is available in the folders [AndroidArpoiseU2018_4](AndroidArpoiseU2018_4) and [iOsArpoiseU2018_2](iOsArpoiseU2018_2).

- The **AR-vos** client is an image trigger, a SLAM and a geolocative AR app using ARKit on iOS and ARCore on Android.
Download: [Google Play](https://play.google.com/store/apps/details?id=com.arpoise.ARvos) Store, Apple [App Store](https://apps.apple.com/us/app/ar-vos/id1483218444). The source code for the apps is available in the folders [AndroidArvosU2018_4](AndroidArvosU2018_4) and [iOsArvosU2018_2](iOsArvosU2018_2).

The goal of ARpoise is to provide an open-source, community-supported, location-based AR app as a replacement for the 
[Layar app](https://www.layar.com/) and other commercial geolocative AR platforms.

## ARpoise Examples
The document [ARpoise-examples](ARpoise-examples.md) explains how to see some examples for **ARpoise**.
The examples include versions of the following pieces:
- [Reign of Gold](https://www.tamikothiel.com/AR/reign-of-gold.html) by Tamiko Thiel.
- [Evolution of Fish](https://www.tamikothiel.com/evolutionoffish/index.html) by Tamiko Thiel and /p.
- [Lotus Meditation](https://www.tamikothiel.com/AR/lotus-meditation.html) by Tamiko Thiel and /p.
- [Gardens of the Anthropocene](https://tamikothiel.com/gota/index.html) by Tamiko Thiel.

## AR-vos Examples
The document [AR-vos-examples](AR-vos-examples.md) explains how to see some examples for **AR-vos**.
The examples include various image triggered and SLAM AR objects.

## Functionality
- **ARpoise** is a location based AR application.

- **ARpoise** allows the prefabs shown to be either purely location based or to be relative to the user.

- **ARpoise** dynamically downloads the Unity 3d prefabs shown.

- **AR-vos** is an image trigger, a SLAM, and a geolocative AR application. It uses ARCore on Android and ARKit on iOS.

- **AR-vos** dynamically downloads the trigger images used and the Unity 3d prefabs shown once the image triggers.

- **AR-vos** allows the prefabs shown to either be triggered by an image or to be placed by SLAM or to be location based or to be relative to the user.

## Release Notes
- **iOS ARpoise 1.8 (20200522), May 28, 2020 at 10:13 PM**
  - Fixed Bug #8.
  - Added two new animation types 'Fade' and 'Destroy'.

- **iOS AR-vos 1.2 (20200522), May 28, 2020 at 10:45 PM**
  - Fixed Bug #8.
  - iOS AR-vos now uses Unity 2018.4.23 LTS as build environment.
  - Added two new animation types 'Fade' and 'Destroy'.

