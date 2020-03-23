![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoiseDirectory Back End

## Overview
The **porPOIse Back End user interface** is an open source project originally developed in 2009 by Jens de Smit for use with the AR platform LAYAR (now defunct). After using it for years with LAYAR, we created this modified **ARpoise porPOIse** version for use with ARpoise. It requires a directory back end as registry for the AR projects that it services.

We maintain our **ARpoiseDirectory Back End** on www.arpoise.com. The ArpoiseDirectory is like a single "metalayer" in which each 
"**POI**" (Point Of Interest) defines the name, URL, attributes, and location of a **layer** (an AR project) that is visible in ARpoise.
Content creators who want to use the ARpoise or AR-vos apps to deliver their 3D content do NOT need to run this service themselves. They only need to set up the [porPOIse Back End](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md) user interface, then request that we (as ARpoise administrators at public@arpoise.com) register their layers in the directory.

## Functionality
When the [ARpoise Directory Front End](https://github.com/ARPOISE/ARpoise/tree/master/ArpoiseDirectory#arpoisedirectory) receives a request from a client ARpoise or AR-vos app, it contacts the ARpoise Directory Back End web service with the client's GPS location as parameter. 

- The ARpoise Directory Back End service then returns all definitions of layers that are within a specified range of the client's GPS location.

- If there are no layers within range of the client's location, an empty list of layer definitions is returned.

- If the same augment should be visible in different locations around the world, a separate copy of that layer needs to be created and placed at the desired GPS locations.

- To configure this in the ARpoiseDirectory Back End, the layers can be placed at the desired GPS coodinates either by typing in the coordinates directly, or with a Google Maps based click-and-drag web interface.

.
## Documentation:

.
## ARpoiseDirectory Back End Layer Configuration

.
### Screen Shot: Arpoise Directory Service - Overview page

When you log in to the ARpoiseDirectory Back End, you will see one entry for geolocative layers serviced by the **ARpoise 
app**, and another entry for both geolocative and image trigger layers serviced by the **AR-vos app**. The older ARpoise 
app runs on all iOS and Android smartphones, whereas the newer AR-vos app only runs on recent smartphones that support the 
AR functionality of [Android ARCore](https://developers.google.com/ar/discover/supported-devices) or [Apple ARKit](https://developer.apple.com/library/archive/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html). We expect to merge the two apps in the future when the newer smartphone 
hardware is widespread.

![DirectoryImg0](/images/Directory0.png)

.
### Screen Shot: Arpoise Directory Service - Arpoise-Directory

The Arpoise-Directory entry lists all **layers** that are serviced by the ARpoise app client, and for each layer specifies a base GPS position that defines the area of the world in which the layer is visible. If the same augment should be visible in different locations around the world, a separate copy of that layer needs to be created and placed at each of the desired GPS locations.

To add a new layer, click on the "New Layer" link above the list of current layers.

![DirectoryImg0a](/images/Directory0a.png)

.
### Screen Shot: Add new layer

In the next screen just click on "Create" ...

![DirectoryImg0b](/images/Directory0b.png)

### Screen Shot: New empty layer added to directory

... and a new empty layer will be added to the directory.

Click on the new layer name "no title" to configure the new layer.

![DirectoryImg0c](/images/Directory0c.png)

### Screen Shot: Configure new layer

See the description of possible attributes beneath this example image.

Enter the configuration for the new layers and then click the "Save" button

![DirectoryImg1](/images/Directory1.png)
### Explanation:
The following attributes of a layer can be edited.
* **Layer Name**: The name of the layer, for internal purposes only.
* **Lat/Lon**: The base GPS location of the layer **in decimal form only.** Can be changed by typing in the latitude and longitude, or by dragging the layer's marker in the placement map. If the same augment should be visible in different locations around the world, a separate copy of that layer needs to be created and placed at each of the desired GPS locations.
* **Is visible**: Use this box to turn the augment on or off for testing purposes.
* **Porpoise URL**: The URL of the ARpoise PorPOISe Back End serving the layer.
* **Layer Title**: The title of the layer shown in the client application when a list of layers is displayed for selection. Please give this a unique name to make it easier for the public to identify which artist and artwork it is!
* **Line 2**: The second line of text shown in the client application when a list of layers is displayed for selection.
* **Line 3**: The third line of text shown in the client application when a list of layers is displayed for selection.
* **Icon name**: The name of the icon shown in the client application when a list of layers is displayed for selection. This can only be done by the ARpoise adminstrators - please contact us at public@arpoise.com.
* **Save** button: Saves the layers's attributes to the ARpoise Directory's xml file.

.
## Testing 

Stand at the GPS location where you set the new layer, open the ARpoise app and see if you can see it there. Make sure "Is visible" is YES! ;-)

.
## Next Steps

### Play around in porPOIse with the assets in your test layer
To learn what you can do in porPOIse, including animations, play around with your existing test layer using the tutorial on the ARpoise porPOISe Back End:
https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md

### Exporting Unity assets into AssetBundles for ARpoise:
To create a new AR experience for your layer, you need to set up the assets in Unity (2D and 3D objects, sounds) and then export them into asset bundles. Learning to use Unity goes beyond the scope of our tutorials, but here is our tutorial for turning Unity assets into AssetBundles:
https://github.com/ARPOISE/ARpoise/blob/master/documentation/CreatingAssetBundles.md

.
## Original porPOIse Documentation: to set up your own porPOIse service

The **porPOIse Back End user interface** is an open source project originally developed in 2009 by Jens de Smit for use with the AR platform LAYAR. Layar is now defunct, but porPOIse continues to work well.

===PorPOISe for Layar===
Portable Point-of-Interest Server for LAYAR AR app (www.layar.com, now defunct)

===Administrative contact===
Jens de Smit, jens@layar.com (email might be out of date, as LAYAR when out of business in 2019)

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
  * http://layar.com/ wa the target platform for this project was developed (defunct since 2019)
