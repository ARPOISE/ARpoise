![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment

## Overview
This document describes the user experience of the **ARpoise** client apps for iOS and Android.

## Experience
The user visits a venue showing some augmented reality (AR) art work built with **ARpoise**.

Information given with the art work at the venue informs the user that the art work can be visited with the user's own smart phone or tablet device.

The user enters the url of arpoise.com into the web browser on the device, or scans a QR-code to get to the web site www.arpoise.com.

The web site www.arpoise.com allows the user to download the **ARpoise** client app from the Google Play Store for Android or the Apple App Store for iOS.

The user starts the download and the installation of the app.

During installation the app asks the user for permission to access the GPS location service of the device and the device camera.

The user has to grant both permissions and can then start the app.

At startup the app retrieves the GPS location latitude and longitude value from the device and sends this information via a https request to the **ARpoise** backend service. This is a standard https request, no private data of the user is transfered.

The **ARpoise** backend service determines that there is indeed an **ARpoise** enabled AR art work at the user's location, i.e. the venue the user is visiting. The **ARpoise** backend service sends a description of the art work back to the **ARpoise** client app. This description contains the URLs of the content of the art work, i.e. a Unity asset bundle file containing the 3D objects comprising the art work.

The **ARpoise** client app downloads the 3D art work via https and shows it as an AR art work with the live image of the device camera as the background.

Using the device like a window the user looks and walks around at the venue to experience the AR art work.

Once the user is finished looking at the art work, she/he closes the app.

Aside from the https web requests decribed above, no further requests are being sent to any site on the web. No private user data is collected and not such data is being sent over the internet.
