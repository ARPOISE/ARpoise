![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# ARpoise-PorPOISe Back End

## Overview
Use porPOISe to place and adjust **POI**s of your own AR **layers.**
- A **POI** (Point Of Interest) is an asset or a group of assets (such as 3D models, sounds etc.), that make up your AR experience.
- Each individual AR experience or project exists on a separate ARpoise **layer.** In the ARpoise app, a single location in the real world can have multiple projects at exactly the same site, but if each one is in a separate layer, they will not interfere with each other and will show up in the ARpoise app as separate entries in a list of available projects.

If you do not have access to a porPOIse server, you will need to set up your own:
- On the page [https://github.com/ARPOISE/ARpoise](https://github.com/ARPOISE/ARpoise) click on the green **Code** button and then select **Download ZIP**.

    ![Arpoise-Download](/images/Arpoise-Download.PNG)
    
- The zip file **Arpoise-master.zip** contains two sub-directories, **ARpoise-master\config** and **ARpoise-master\php\porpoise**, you need to install them onto **your own** web server!
- Follow the instructions in the file [INSTALL](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/INSTALL)
- For the Google maps based click-and-drag interface as shown in the screen shots below you need to get **your own** Google-maps ID.
- The Google maps based click-and-drag interface for us only works when the porpoise site is accessed via http, if accessed via https, the map is not visible.

## Application

If you are setting up your own porPOIse, contact the ARpoise administrators public@arpoise.com with the following:
- The **URL** of your PorPOISe installation
- The **name** of the layer, and approximate **longitude** and **latitude** of where you want the layer to be available.
- If you want to have the same content visible in different regions of the world, create multiple copies of the same layer with different names, e.g. MyLayerLouvre, MyLayerSFMoMA, etc.

**If you already have a porPOIse and example layer, you can use this tutorial to learn how to modify it.**

- Once you understand how to use porPOIse to set up and modify your layer, you can add new **assets** (2d images, 3D models, sounds, etc.) to it by creating your own AssetBundle in Unity.
- See our tutorial on creating AssetBundles for porPOIse here: https://github.com/ARPOISE/ARpoise/blob/master/documentation/CreatingAssetBundles.md

NOTES:
- The content of each of the **POI**s of your layer needs to be created as a Unity prefab and needs to be put into an Unity asset bundle made available on the web. The asset bundle is referenced by the **POI**'s definition in your layer.
- One thing we found is that asset bundles created for Android do not work on iOS and vice versa. Therefore you need to provide **two** asset bundles, one for Android and one for iOS. As you can only enter one asset bundle url in the ARpoise PorPOISe configuration of any POI, ARpoise assumes that the asset bundle name given is the one of the Android asset bundle. The name of the iOS asset bundle has to be the Android name followed by the letter 'i'. 
- Thus if the file name in the url of your Android asset bundle is, e.g. ".../MyAssetBundle", you also need to create and make available the iOS asset bundle with the url ".../MyAssetBundlei". 
- **Note: you HAVE to make asset bundles with your assets for both Android and iOS, otherwise your layer will not work!**
- **Note: The asset bundles and trigger images you make available on the web and reference in the POI definitions need to be accessable via https**.
- **Note: The trigger images you make available on the web and reference in the POI definitions need to be in jpg format**, see https://en.wikipedia.org/wiki/JPEG.

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
* **Refresh distance**: The refresh distance is optional, if given, it defines the distance in meter after which the client application will reload the layer information. I.e. if a user starts the app and then moves more than **Refresh distance** meters, the layer is reloaded.
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

### Optional Layer Parameters:
---
- **AllowTakeScreenshot**:

An optional layer parameter **AllowTakeScreenshot** can be added to a layer by clicking on the **New action** button shown in the screen shot above.

![LayerAction-AllowTakeScreenshot](/images/LayerAction-AllowTakeScreenshot.png)

**Explanation:**

The values set above would enable the Unity Method ScreenCapture.CaptureScreenshot, public static void CaptureScreenshot(string filename, int superSize); with a superSize value of 1. See https://docs.unity3d.com/ScriptReference/ScreenCapture.CaptureScreenshot.html.

**Note:** Use this parameter if you want to create super sized screen captures.
A screenshot is taken and stored on the user's device **EVERY** time the user tabs the screen, so use this feature with caution. ARpoise also has no way to copy the screenshots from the device to a different computer.

On Android the screenshots are stored in the directory '\Phone\Android\data\com.arpoise.ARpoise\files' and can be accessed once the device is connected to a PC.

For iOS devices an application like iMazing can be used to copy the screenshots, see https://imazing.com/.

---
- **OcclusionEnvironmentDepthMode**, **OcclusionPreferenceMode**, **OcclusionHumanSegmentationStencilMode**, **OcclusionHumanSegmentationDepthMode**:

These optional layer parameters can be added to a layer by clicking on the **New action** button as shown in the screen shot above.

![LayerAction-OcclusionEnvironmentDepthMode](/images/LayerAction-OcclusionEnvironmentDepthMode.png)

**Explanation:**

Most modern iOS devices and some Android devices offer 3D occlusion in AR layers. AR Foundation enables this functionality,
see https://forum.unity.com/threads/environmental-occlusion-and-depth-in-arfoundation.919076/

The parameters **OcclusionPreferenceMode**, **OcclusionEnvironmentDepthMode**, **OcclusionHumanSegmentationStencilMode**, and **OcclusionHumanSegmentationDepthMode** allow to set the parameters of the **AR Occlusion Manager (Script)**.

Values that can be used are:

**OcclusionPreferenceMode**                    

    NoOcclusion
    PreferHumanOcclusion
    PreferEnvironmentalOcclusion

**OcclusionEnvironmentDepthMode**    

    Disabled
    Fastest
    Medium
    Best

**OcclusionHumanSegmentationDepthMode**

    Disabled
    Fastest
    Medium
    Best

**OcclusionHumanSegmentationDepthMode**

    Disabled
    Fastest
    Best


For further information, please refer to the documentation of the **AR Occlusion Manager (Script)**,
see  https://forum.unity.com/threads/environmental-occlusion-and-depth-in-arfoundation.919076/

---
- **PositionUpdateInterval**:

An optional layer parameter **PositionUpdateInterval** can be added to a layer by clicking on the **New action** button shown in the screen shot above.

![LayerAction-PositionUpdateInterval](/images/LayerAction-PositionUpdateInterval.PNG)

**Explanation:**

The values set above would only update the location information every 5.5 seconds.

The GPS coordinates received by the apps sometimes show really wide fluctuations of the location.
As a consequence, all pois with absolute locations move around very often, very quickly and very far.
This layer parameter allows to restrict the time intervals after which the device position is updated into the app,
so the movements can be restricted to happen only each time when the interval expires.

---
- **RemoteServerUrl**, **SceneUrl**:

An optional layer parameter **RemoteServerUrl** and **SceneUrl** can be added to a layer by clicking on the **New action** button shown in the screen shot above.

![LayerAction-RemoteServerUrl](/images/LayerAction-RemoteServerUrl.png)

**Explanation:**

ARpoise allows to share porpoise level animation events via a back-end multi-user server.
The parameters **RemoteServerUrl** and **SceneUrl** configure the access of such sharing.
Events are shared by connecting to the multi-user back-end specified via **RemoteServerUrl**,
all events of all layers having the same **SceneUrl** value are shared.

E.g. when an onClick animation event of a poi is clicked
and the name of the animation event contains the string **Remoted** and event sharing is enabled for the layer the poi is in,
the event is not handled locally but sent to the back-end.
The back-end forwards the event to all ARpoise clients currently connected that use the same **SceneUrl** value,
including the original sender.
When ARpoise receives the forwarded event, it handles the event as if the click happened locally.

Two other animations are triggered when they exist and a remote event is received, a **TriggeredLocally** animation and a **TriggeredRemotely** animation.

E.g. If a shared animation called **ButtonClick_Remoted** is triggered in ARpoise, and event sharing is enabled for the layer the poi is in,
the event is not handled locally but sent to the back-end and forwarded by the back-end to all ARpoise instances connected. 

All ARpoise instances will then activate the animation **ButtonClick_Remoted**. 
Only the ARpoise instance that initially triggered the animation will activate the animation **ButtonClick_TriggeredLocally**, 
all other instances will activate the animation **ButtonClick_TriggeredRemotely**.

---
- **DirectionalLightN_Intensity**, **DirectionalLightSEE_Intensity**, **DirectionalLightSWW_Intensity**, **DirectionalLightN_IsActive**, **DirectionalLightSEE_IsActive**, **DirectionalLightSWW_IsActive**:

These optional layer parameters can be added to a layer by clicking on the **New action**, they allow to control the lights inside ARpoise.

![LayerAction-RemoteServerUrl](/images/LayerAction-LightControl.png)

**Explanation:**

By default ARpoise has three directional lights, 'Directional Light N', 'Directional Light SEE' and 'Directional Light SWW'.
The intensity of the lights can be set on the Porpoise level via an Action on the layer level with the information message set to a positive value, e.g. '0.5'.
The IsActive property of the lights can be set on the Porpoise level via an Action on the layer level with the information message set to a 'true' or 'false'.

---

## ARpoise Back-End POI Configuration
### Screen Shot:
![BackEndImg3](/images/BackEnd3.png)
### Explanation:
The following properties of a POI can be edited:
* **Title**: The title is optional, it can be used to make a POI relative to the camera of the device. If the title of a POI contains the string **CameraChild**, the POI is always displayed relative to the camera of the device. This feature can be used to show a 'Heads up Display' that is alway visible to the user, or to show some introduction text that the user should see. The **Relative location** property of the POI allows to place the object, e.g. **0,0,2** would show it in the middle of the screen, two meters away from the camera.
* **Lat/Lon**: Allows to change the latitude and longitude of the POI. The POI's location can also be changed by dragging the POI's marker in the placement map.
* **Is visible**: This combo box defines whether the POI is visible.
* **Absolute altitude**: Allows to set the absolute altitude of the POI.
* **Relative altitude**: Allows to set the altitude of the POI relative to the user's device.
* **URL of asset bundle**: The POI's geometry will be taken from a Unity asset bundle downloaded from this web location. One thing we found is, asset bundles created for either Android or iOS do not work on the "other" platform.
Therefore you need to provide **two** asset bundles, one for Android and one for iOS.
As you can only enter one asset bundle url in the ARpoise PorPOISe configuration, 
ARpoise assumes that the asset bundle name given is the one of the Android asset bundle.
The name of the iOS asset bundle has to be the Android name followed by 'i'.
* **Caching of the 'URL of asset bundle'**: Starting with version 2.0.12, 25040300, ARpoise caches the asset bundles it downloads. For development purposes you can disable the caching for all POIs is a layer, or for an individual POI. In order to disable the caching you have to add an Action to the layer or POI with the name **AssetBundleCacheVersion** and set its value to **0**. Once you successfully updated your asset bundle and want to turn on caching again, you should set the value of the **AssetBundleCacheVersion** to an 8 digit integer of the form YYMMDD00 of the current date. E.g., on June 3rd 2025 you should set the value to **25060300**. ARpoise uses its bundle identifier which is set to the release date of he ARpoise version in question, e.g. **25040300** for Version 2.0.12, the number you set should be bigger than that number. 

* **Prefab name**: The POI's geometry is loaded from the asset bundle with this prefab name.
* **Layer name**: If this value is set, the entire layer mentioned will be loaded along with the POI. The document [SubLayers](/documentation/SubLayers.md) explains how such a layer can be used to construct a complex POI from simple POIs.
* **Relative location**: The location of the POI relative to the user's device. Comma separated list of the X-east-west, Y-up-down, Z-north-south values.
* **Scaling factor**: This values allows to scale the POI's geometry in all three dimensions.
* **Vertical rotation**: This values allows to rotate the POI's geometry around the Y axis.
* **Relative angle**: If this value is set to Yes, the POI's geometry will always be turned so that the same side always faces the user.
* **URL for trigger image**: Used only in AR-vos app!

  * **Image Trigger** - If the URL of an image file in jpg format is entered into this field, the POI will be treated as an image trigger POI. The POI will be shown once the trigger image is detected in the environment. You may use any image you like as an image trigger. The url entered has to be a valid web url like "**www.arpoise.com/TI/arvos_logo_rgb-weiss1024.jpg**".
  * **Image Detection** - The AR-vos app uses apple's [ARKit](https://developer.apple.com/augmented-reality/) and Google's [ARCore](https://developers.google.com/ar/) for image detection. Please refer to their documentation for recommendations on the type of images you can use.
  * **Image - Tracking Timeout** - Once a trigger image is detected in the environment, the POI is shown. By default, the POI will be kept visible forever, even if the device cannot detect the trigger image in the environment anymore. Optionally, the number of milliseconds a POI will be kept visible after the device stopped tracking the trigger image can be restricted using an Action with the Parameter set to **TrackingTimeout** and it's Value set to a positive number, as shown below.

    ![BackEndImg3TrackingTimeout](/images/BackEndImg3TrackingTimeout.png)

  * **SLAM** - If the URL contains only the word **SLAM**, the POI is treated as SLAM object and will be shown every time the user taps on a vertical or horizontal plane detected in the enviroment. 
  * **SLAM - Maximum Count** - For SLAM POIs, the number of times an object can be placed into the environment can be restricted using an Action with the Parameter set to **MaximumCount** and it's Value set to a positive number, as shown below.

    ![BackEndImg3MaximumCount](/images/BackEndImg3MaximumCount.png)

* **Width of trigger image:** Used only in AR-vos app. This is an approximate with of your trigger image in real life (e.g. a poster might be 0.3 meters = 1 foot wide, a doorway might be 1 meter = 3 feet wide, a house facade might be 10 meters = 30 feet wide.)
* Animation-list-**Remove**: Delete the animation from the POI.
* **New animation** button: Allows to add a new animation to the POI.
* **Save** button: Saves the POI's attributes to the layer's xml file.

The following properties of an animation can be edited:
* **Name**: The name is optional, it is used when one animation specifies that it should be followed by other animations.
  
   The animation can also [open a web page](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#opening-a-web-page) in a browser using the name.

  The **Name** can also be used to make a POI a child of the camera node, so that it alway stays visible.
  If the **Name** contains the string **CameraChild** and its **Relative location** is set,
  the relative location is treated relative to the device camera.
  This can used to display some explanation images when the layer is loaded,
  which then disappears when the user clicks on it.

  The **Name** can also be used to share an event via the multi-user back-end.
  See **Optional Layer Parameters** **RemoteServerUrl**, **SceneUrl** above.
  When the **Name** of the animation event contains the string **Remoted** and event sharing is enabled
  for the layer the poi is in,
  the event is not handled locally but sent to the back-end.
  The back-end forwards the event to all ARpoise clients currently connected that use the same **SceneUrl** value,
  including the original sender.
  When ARpoise receives the forwarded event, it handles the event as if the click happened locally.

  The **Name** can also be used to specify the start and end time of an **inMinutes** animation.
  For this to work the name should be of the form **Time: hh:mm - hh:mm**, e.g. **Time: 10:00 - 10:01**.
  This would activate the animation at 10 am.
  
* **Event**: 
  * **onCreate** - the animation is started when the POI is loaded.
  * **onFollow** - the animation is started when it's predecessor animation ends.
  * **onClick** - the animation is started when the POI is clicked by the user; In order for the **onClick**, **onFocus**, and **inFocus** animations to work, the POI's Unity game object needs to include a Collider component.
  * **onFocus** - the animation is started when the POI is looked at by the user;
  * **inFocus** - the animation is started when the POI is looked at by the user and is stopped once the POI loses the focus;
  * **inMinutes** - the animation is started during a given minute interval every day, see **Name** above;
  * **whenActivated** - the animation is started when the POI is placed into the layer because it's trigger image has been found, the first time the image is found also the **onCreate** animations are run.
  * **whenDeactivated** - the animation is started when the POI is removed from a layer because it's trigger image is no longer visible to the device.
  
* **Type**: 
  * **rotate**, rotate the POI around an axis.
  * **transform**, transform the POI to another location.
  * **scale**, scale the size of the POI.
  * **volume**, animate the volume of sound played via the POI.
  * **buzz**, vibrate the hand held device. In general only phones have the ability to vibrate.
  * **fade**, fade the POI between full visibility and invisibility, e.g. the animation shown below fades a POI within 10 seconds from 1 (full visibility) to 0 (invisible) and back again.
  
  ![BackEndImg4](/images/BackEnd4.PNG)
  
  **Note:** In order for a fade animation to work, the rendering mode of the material of the POI's Unity game object needs to be set to 'Fade'.
  
  ![BackEndImg5](/images/BackEnd5.PNG)
  * **destroy**, destroy the POI.
* **Length**: Length of the animation in seconds.
* **Delay**: Delay of the animation in seconds, onCreate-animations will only start after this delay.
* **Interpolation**:
  * **linear** - the value is changed linearly from **From** to **To**;
  * **cyclic** - the value is changed linearly from **From** to **To** and then back to **From**;
  * **sine** - the value swings between **From** and **To** like a pendulum;
  * **halfsine** - the value is changed from **From** to **To** and then back to **From**.
  * **smooth** - the value is smoothly changed from **From** to **To** and then back to **From**.
* **Persist**:
  * **Yes** - at the end of the animation the POI will stay as the animation leaves it;
  * **No** - at the end of the animation the POI will snap back to its original state.
* **Repeat**:
  * **Yes** - the animation is repeated forever;
  * **No** - the animation is only run once.
* **From**: Start value of the animation.
* **To**: End or middle value of the animation, depending on the **Interpolation**.
* **Axis (x,y,z)**: Axis to apply the animation to. E.g.: A rotation with Axis 1,0,0 is only around the X axis.
* **Followed by**: If one or more comma separated animation names are given in this field, the animations mentioned are started once this animation ends. Animation names are global for all POIs of a layer, the end of an animation of one POI can start an animation of the same POI or of another POI. The animation can also [open a web page](https://github.com/ARPOISE/ARpoise/blob/master/php/porpoise/README.md#opening-a-web-page) in a browser using the field.

### Playing a Sound
If an animation is started and the Unity-prefab of the POI contains an AudioSource component, the audio source is played.
The volume of the sound can be animated via a animation of type **volume**.

### Opening a WEB Page
Animations allow opening a web page in a browser on the user's device. In order to do so, either the **Name** or the **Followed by** value of the animation must be of the form "**openUrl:https://www.arpoise.com/**". If an animation with such a **Name** is **started**, or an animation with such a **Followed by** value **ends**, the app will open the URL given after the "**openUrl:**" tag in a web browser. 

### Setting the Game Object State
This feature has been implemented as an enhancement with the releases done in August of 2021.
If an animation whose name is equal to or ends with **SetActive** or **SetInActive** is **started**, or an animation with such a **Followed by** value **ends**, the game object of the POI is set to active/inactive.

![GameObject-SetInActive](/images/GameObject-SetInActive.png)

The animations shown above would make a game object blink on/off with a frequency of one second.

### Animating the Same Values with two Animations
This feature has been implemented as an enhancement with the releases done in August of 2021.
In order to allow to have two animations to change the same wrapper object, animations whose name starts with the same sequence of characters before the first **/** in the name, e.g. **scaleObject/up** and **scaleObject/down**, are animating the same transform.

### Remoting of Animation Events
This feature has been implemented as an enhancement with the releases done in 2023.

An optional layer parameter **RemoteServerUrl** and **SceneUrl** can be added as described above.
ARpoise allows to share porpoise level animation events via a back-end multi-user server. The parameters RemoteServerUrl and SceneUrl configure the access of such sharing. Events are shared by connecting to the multi-user back-end specified via RemoteServerUrl, all events of all layers having the same SceneUrl value are shared.

E.g. when an onClick animation event of a poi is clicked and the **name** of the animation event contains the string **Remoted** and event sharing is enabled for the layer the poi is in, the event is not handled locally but sent to the back-end. The back-end forwards the event to all ARpoise clients currently connected that use the same SceneUrl value, including the original sender. When ARpoise receives the forwarded event, it handles the event as if the click happened locally.

It is also possible to have different animations for the user that started an animation and the users that are just receiving the animation event from the back-end multi-user server. For this to work you would need three animations on the poi. One to just remote the event, lets call it **MyAnimation_Remoted**, if this animation is triggered locally, the trigger is forwarded to the back-end multi-user server. If there is an animation called **MyAnimation_TriggeredLocally**, it will only be activated for the user that triggered the animation and sent the value to the back-end multi-user server. If there is an animation called **MyAnimation_TriggeredRemotely**, it will be activated for the users that only received the event from the back-end multi-user server. 

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
  * http://layar.com/ is, of course, the reason this project exists
  
