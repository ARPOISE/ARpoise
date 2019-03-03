![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise-PorPOISe Back End

## Overview
This is the version of PorPOISe used for placing of the **POI**s of **your own** layers.

In order to add your layers to ARpoise you need to download and install this package onto **your own** web server!

For using the Google-maps-based click-and-drag interface interface you need to use **your own** Google-maps id.

## Application
In order for any of your layers to be visible in ARpoise
you need to supply the **url** of your PorPOISe installation that is serving the layer's content,
and the **name**, and the **longitude** and **latitude** coordinates of the layer.

The 3D content of all of the **POI**s of your layer needs to be put as an Unity prefab into an Unity asset bundle
made available on the net and referenced by the **POI** definition in your layer.

Creation of asset bundles in Unity is beyond this documentation. One thing we found is,
asset bundles created for either Android or iOS do not work on the "other" platform.
Therefore you need to provide **two** asset bundles, one for Android and one for iOS.

As you can only enter one asset bundle url in the ARpoise PorPOISe configuration, 
ARpoise assumes that the asset bundle name given is the one of the Android asset bundle.
The name of the iOS asset bundle has to be the Android name followed by 'i'.

Thus if the file name in the url of your Android asset bundle is, e.g. ".../MyAssetBundle_",
you also need to avail the iOS asset bundle with the url ".../MyAssetBundle_i". 


## Functionality
The ARpoise client contacts this web service with the client location and the layer name as parameter.

The service has to return the definitions of of the **POI**s of the layer.

The placing of the **POI**s of the layer into world coordinates can be performed in a Google-maps-based click-and-drag interface.

## Documentation
### Arpoise Back-End Layers List
![BackEndImg1](/images/BackEnd1.png)
### Arpoise Back-End Layer Configuration
![BackEndImg2](/images/BackEnd2.png)
### Arpoise Back-End POI Configuration
![BackEndImg3](/images/BackEnd3.png)
## Original Documentation
===PorPOISe for Layar===
Portable Point-of-Interest Server for Layar

===Administrative contact===
Jens de Smit, jens@layar.com

===Introduction===
PorPOISe is a server for Layar clients. It converts your data sets of POIs
(Points of Interest) into responses to the Layar client. Things like JSON
formatting and distance calculation are all done for you. PorPOISe supports
XML files as data stores.

===Getting started===
Read INSTALL for installation instructions. Once properly installed, you can
use the dashboard to create your first POIs. The interface is pretty spartan
but this will generate correct output format. Study the format if you intend to
generate your own XML files.

From here on you're on your own. Build a better interface for the dashboard or
expand PorPOISe to have more features if you need more.

===History===
PorPOISe originated at SURFnet in 2009 as a spin-off from a small layer-
building experiment. Over 2010 functionality expanded and feature support
grew with Layar's feature support. In 2011, PorPOISe's primary author moved
from SURFnet to Layar and took the project with him.

===More information===
  * http://www.surfnet.nl/en The home of the creator of PorPOISe
  * http://teknograd.wordpress.com/2009/10/19/augmented-reality-create-your-own-layar-layer/ An explanation on how to build the most minimal of Layar servers. Very useful to get started
  * http://layar.com/ is, of course, the reason this project exists
