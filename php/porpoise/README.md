![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise-PorPOISe Back End

## Overview
Use porPOISe to place and adjust **POI**s of your own AR **layers.**
- A **POI** (Point Of Interest) is an asset or a group of assets (such as 3D models, sounds etc.), that make up your AR experience.
- Each individual AR experience or project exists on a separate ARpoise **layer.** In the ARpoise app, a single location in the real world can have multiple projects at exactly the same site, but if each one is in a separate layer, they will not interfere with each other and will show up in the ARpoise app as separate entries in a list of available projects.

**If you already have access to a porPOIse, please proceed to the description below.**

If you do not have access to a porPOIse server, you will need to set up your own:
- Download and install the [porPOIse package](https://github.com/ARPOISE/ARpoise/tree/master/php/porpoise) onto **your own** web server!
- Follow the instructions in the file [INSTALL](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/INSTALL)
- For the Google maps based click-and-drag interface as shown in the screen shots below you need to get **your own** Google-maps ID.

## Application

If you are setting up your own porPOIse, contact the ARpoise administrators public@arpoise.com with the following:
- The **URL** of your PorPOISe installation
- The **name** of the layer, and approximate **longitude** and **latitude** of where you want the layer to be available.
- If you want to have the same content visible in different regions of the world, create multiple copies of the same layer with different names, e.g. MyLayerLouvre, MyLayerSFMoMA, etc.

. 
**If you already have a porPOIse and example layer, you can use this tutorial to learn how to modify it.**

- One you understand how to use porPOIse to set up and modify your layer, you can add new **assets** (2d images, 3D models, sounds, etc.) to it by creating your own AssetBundle in Unity.
- See our tutorial on creating AssetBundles for porPOIse here: https://github.com/ARPOISE/ARpoise/blob/master/documentation/CreatingAssetBundles.md

NOTES: (This should be moved to the AssetBundle tutorial):
- The content of each of the **POI**s of your layer needs to be created as a Unity prefab and needs to be put into an Unity asset bundle made available on the web. The asset bundle is referenced by the **POI**'s definition in your layer.
- One thing we found is that asset bundles created for Android do not work on iOS and vice versa. Therefore you need to provide **two** asset bundles, one for Android and one for iOS. As you can only enter one asset bundle url in the ARpoise PorPOISe configuration of any POI, ARpoise assumes that the asset bundle name given is the one of the Android asset bundle. The name of the iOS asset bundle has to be the Android name followed by the letter 'i'. 
- Thus if the file name in the url of your Android asset bundle is, e.g. ".../MyAssetBundle", you also need to create and make available the iOS asset bundle with the url ".../MyAssetBundlei". 
- **Note: you HAVE to make assets for both Android and iOS, otherwise your layer will not work!**
- **Note: The asset bundles and trigger images you make available on the web and reference in the POI definitions need to be accessable via https**.

## Functionality

If you are setting up your own porPOIse:
- The ARpoise client app contacts this web service with the client location and the layer name as parameters.
- The service has to return the definitions of the **POI**s of the layer.
- The placing of the **POI**s of the layer into world coordinates can be performed in a Google-maps-based click-and-drag interface.

. 
## Documentation:

. 
## ARpoise Back-End Layers List

. 
### Screen Shot: Arpoise Directory Service - Overview page

![BackEndImg1](/images/BackEnd1.png)

### Explanation:
A list of all your layers is shown. In order to add a new layer you will have to edit the file **config.xml** in your configuration directory and also create an **.xml** file for your new layer in the configuration directory.

## ARpoise Back-End Layer Configuration
### Screen Shot:
![BackEndImg2](/images/BackEnd2.png)
### Explanation:
The following properties of a layer can be edited:
* **Layer title**: The layer title is optional, if given, it is displayed by the application in the top center of the screen.
* **Refresh interval**: The refresh interval is optional, if given, it defines the seconds after which the client application will reload the layer information.
* **Redirect to layer**: The redirection layer is optional, if given, the layer redirected to is displayed by the client instead of the current one.
* **Visibility in meters**: The range in meters inside which the layer is visible to clients, 1500m is the maximum.
* **Area size in meters**: The area size is optional, if given, POIs having an absolute geo-location are kept within this area.
* **Area width in meters**: The area width is optional, if given, POIs having an absolute geo-location are kept within this area.
* **Show menu button**: This combo box defines whether the ARpoise menu button is shown by the client application.
* **Apply Kalman filter**: This combo box defines whether a Kalman filter is applied to the device location by the client application.
* **No pois message**: This message is shown by the client application, if there are no POIs in range of the device location.
* Action **Show information**: This combo box defines whether information messages are displayed by the client application.
* Action **Information message**: This allows to set a static information message to be displayed by the client application.
* **Save** button: Saves the attributes to the layer's xml file.
* **New POI** link: Allows to add a new POI to the layer.
* POI-list-**Lat**: Allows to change the latitude of the POI. 
* POI-list-**Lon**: Allows to change the longitude of the POI. The POI's location can also be changed by dragging the POI's marker in the placement map.
* POI-list-**Save**: Save changes to the POI's location to the layer's xml file.
* POI-list-**DEL**: Delete the POI from the layer.

## ARpoise Back-End POI Configuration
### Screen Shot:
![BackEndImg3](/images/BackEnd3.png)
### Explanation:
The following properties of a POI can be edited:
* **Title**: The title is optional, it is not used by the client application.
* **Lat/Lon**: Allows to change the latitude and longitude of the POI. The POI's location can also be changed by dragging the POI's marker in the placement map.
* **Is visible**: This combo box defines whether the POI is visible.
* **Absolute altitude**: Allows to set the absolute altitude of the POI.
* **Relative altitude**: Allows to set the altitude of the POI relative to the user's device.
* **URL of asset bundle**: The POI's geometry will be taken from a Unity asset bundle downloaded from this web location. One thing we found is, asset bundles created for either Android or iOS do not work on the "other" platform.
Therefore you need to provide **two** asset bundles, one for Android and one for iOS.
As you can only enter one asset bundle url in the ARpoise PorPOISe configuration, 
ARpoise assumes that the asset bundle name given is the one of the Android asset bundle.
The name of the iOS asset bundle has to be the Android name followed by 'i'.
* **Prefab name**: The POI's geometry is loaded from the asset bundle with this prefab name.
* **Layer name**: If this value is set, the entire layer will be loaded instead of the POI.
* **Relative location**: The location of the POI relative to the user's device. Comma separated list of the X-east-west, Y-up-down, Z-north-south values.
* **Scaling factor**: This values allows to scale the POI's geometry in all three dimensions.
* **Vertical rotation**: This values allows to rotate the POI's geometry around the Y axis.
* **Relative angle**: If this value is set to Yes, the POI's geometry will always be turned so that the same side always faces the user.
* Animation-list-**Remove**: Delete the animtion from the POI.
* **New animation** button: Allows to add a new animation to the POI.
* **Save** button: Saves the POI's attributes to the layer's xml file.

The following properties of an animation can be edited:
* **Name**: The name is optional, it is used when one animation specifies that it should be followed by other animations.
* **Event**: 
  * **onCreate** - the animation is started when the POI is loaded;
  * **onFollow** - the animation is started when it's predecessor animation ends.
  * **onClick** - the animation is started when the POI is clicked by the user; In order for the **onClick**, **onFocus**, and **inFocus** animations to work, the POI's Unity game object needs to include a Collider component.
  * **onFocus** - the animation is started when the POI is looked at by the user;
  * **inFocus** - the animation is started when the POI is looked at by the user and is stopped once the POI loses the focus;
  
* **Type**: **rotate**, **transform**, **scale**.
* **Length**: Length of the animation in seconds.
* **Delay**: Delay of the animation in seconds, onCreate-animations will only start after this delay.
* **Interpolation**:
  * **linear** - the value is changed linearly from **From** to **To**;
  * **cyclic** - the value is changed linearly from **From** to **To** and then back to **From**;
  * **sine** - the value swings between **From** and **To** like a pendulum;
  * **halfsine** - the value is changed from **From** to **To** and then back to **From**.
* **Persist**:
  * **Yes** - at the end of the animation the POI will stay as the animation leaves it;
  * **No** - at the end of the animation the POI will snap back to its original state.
* **Repeat**:
  * **Yes** - the animation is repeated forever;
  * **No** - the animation is only run once.
* **From**: Start value of the animation.
* **To**: End or middle value of the animation, depending on the **Interpolation**.
* **Axis (x,y,z)**: Axis to apply the animation to. E.g.: A rotation with Axis 1,0,0 is only around the X axis.
* **Followed by**: If one or more comma separated animation names are given in this field. The animations mentioned are started once this animation ends. Animation names are global for all POIs of a layer. So the end of an animation of one POI can start an animation of the same POI or of another POI.

If an animation is started and the Unity-prefab of the POI contains an AudioSource component, the audio source is played.
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
  
