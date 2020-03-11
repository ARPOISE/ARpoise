![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment
# -- Creating Asset Bundles --
## Overview
This is a step by step tutorial to create an asset bundle that can be used by **ARpoise** or **AR-vos**.
In the tutorial an asset bundle for Android is created.
This is done on Windows 10.
The process for creating an iOS asset bundle or doing the steps on a Mac are the similar.
See the last section of this document for further information.

## Unity Install
The tutorial uses Unity 2018.3.14.f1 on Windows 10.
### Image - Unity Install:
![CreateAssetBundle1](/documentation/images/CreateAssetBundle1.PNG)

Unity is installed with the following modules.
### Image - Unity Modules:
![CreateAssetBundle2](/documentation/images/CreateAssetBundle2.PNG)

Microsoft Visual Studio is also installed. The free version is good enough.

## CreateAssetBundle Project
Start out by creating a new Unity project, call the project **CreateAssetBundle**.
### Image - Unity Project:
![CreateAssetBundle3](/documentation/images/CreateAssetBundle3.PNG)

## Android Build
The build platform needs to be changed to Android.

Click: **File / Build Settings…**

Select the **Android** Platform and click **Switch Platform**.

### Image - Android Platform:
![CreateAssetBundle4](/documentation/images/CreateAssetBundle4.PNG)

This will take a while, afterwards you can end the Build Settings view.

## Folders
In the Project panel right click on **Assets** and select **Create / Folder** from the context menu. Create the folders **AssetBundles**, **Editor**, **Materials**, and **Prefabs**.
### Image - Assets Folders:
![CreateAssetBundle5](/documentation/images/CreateAssetBundle5.PNG)

## 3D Assets
Sorry, this is not a Unity tutorial for creating 3D assets.
For the tutorial two game objects have been created in the SampleScene, a red cube and a blue sphere.
### Image - Game Objects:
![CreateAssetBundle6](/documentation/images/CreateAssetBundle6.PNG)

## 3D Prefabs
Turn the two objects into prefabs by dragging them from the **SampleScene** panel to the **Prefabs** folder created above.
### Image - Prefabs:
![CreateAssetBundle7](/documentation/images/CreateAssetBundle7.PNG)

Make sure both prefabs do not have any transformations, e.g.:
### Image - Transform:
![CreateAssetBundle8](/documentation/images/CreateAssetBundle8.PNG)

## CreateAssetBundles Script
Right click on the **Editor** folder, from the context menu select **Create / C# Script**.
Call the script **CreateAssetBundles**. Repeat the step and call the second script **CreateiOSAssetBundles**.
### Image - Scripts:
![CreateAssetBundle9](/documentation/images/CreateAssetBundle9.PNG)

Double click on the **CreateAssetBundles** script. Visual Studio or your favorite C# editor should open.

Empty the file and copy the following lines into the file and save it.
```
using System.IO;
using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
```
Copy the following lines into **CreateiOSAssetBundles** and save it.
```
using System.IO;
using UnityEditor;

public class CreateiOSAssetBundles
{
    [MenuItem("Assets/Build iOSAssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.iOS);
    }
}
```

## Prefab - Context Menu Entries
Now when you right click on the **BlueSphere** prefab in the Prefabs folder you should see a context menu with the two entries
**Build AssetBundles** and **Build iOSAssetBundles** at the bottom.
### Image - Menu Options:
![CreateAssetBundle10](/documentation/images/CreateAssetBundle10.PNG)

## Create an Asset Bundle
Select the **BlueSphere** prefab.

On the right bottom of the Unity window you should see the label **AssetBundle** with **None** selected.
Click on **None** and select **New** from the context menu. Enter the name of the new asset bundle,
for the tutorial that is **exampleassetbundle**.
### Image - Select Asset Bundle:
![CreateAssetBundle11](/documentation/images/CreateAssetBundle11.PNG)

Select the **RedCube** prefab and also select the **exampleassetbundle** for it.

Select the **RedCube** prefab, Right Click and select the **Build AssetBundles** option from the bottom of the context menu.

### The asset bundle should be created!!!!!!!!

Select the **AssetBundles** folder created earlier. You should see four entries.
### Image - Asset Bundles:
![CreateAssetBundle12](/documentation/images/CreateAssetBundle12.PNG)

The first **exampleassetbundle** file is the actual asset bundle that needs to be made available on the web.
The second file is a manifest file describing what is in the bundle. The file should contain something like:
```
ManifestFileVersion: 0
CRC: 1652422628
Hashes:
  AssetFileHash:
    serializedVersion: 2
    Hash: 912661529e9af10c64a43b680d62010f
  TypeTreeHash:
    serializedVersion: 2
    Hash: 6597594720de07d06673fc08865c772c
HashAppended: 0
ClassTypes:
- Class: 1
  Script: {instanceID: 0}
- Class: 4
  Script: {instanceID: 0}
- Class: 21
  Script: {instanceID: 0}
- Class: 23
  Script: {instanceID: 0}
- Class: 33
  Script: {instanceID: 0}
- Class: 43
  Script: {instanceID: 0}
- Class: 48
  Script: {instanceID: 0}
- Class: 65
  Script: {instanceID: 0}
- Class: 135
  Script: {instanceID: 0}
Assets:
- Assets/Prefabs/BlueSphere.prefab
- Assets/Prefabs/RedCube.prefab
Dependencies: []
```
It is good idea to keep the manifest around with the asset bundle so you can find out what is in the bundle.

The actual asset bundle file needs to be uploaded to the web and needs to be made available via http.
The url of that file needs to be entered into the porpoise configuration of the layer
that wants to show the red cube or the blue sphere as points of interest,
but that topic is covered in a different tutorial.

## iOS Asset Bundles
In order to build an iOS asset bundle the iOS Unity module needs to be installed, see the second image above. Then the script CreateiOSAssetBundles can be used to create the asset bundles for iOS.

**Important Note**: In order for the assets to work in ARpoise and AR-vos on Android and iOS both the Android and the iOS asset bundle need to be created and made available on the web via http.

Furthermore, the following naming convention needs to be applied.
If the name of the Android asset bundle is **exampleassetbundle**,
the name of the iOS asset bundle needs to be **exampleassetbundlei**.
I.e. the same name followed by a lower case letter ‘**i**’.



