![ARpoise Logo](/images/arpoise_logo_rgb-128.png)
# *A*ugmented *R*eality *p*oint *o*f *i*nterest *s*ervice *e*nvironment

## Overview
This is a step by step tutorial to create an asset bundle that can be used by **ARpoise** or **AR-vos**.
In the tutorial creates an asset bundle for Android. It is done on Windows 10. The process for an iOS asset bundle or doing the steps on a Mac are the similar.

## Unity Install
The tutorial uses Unity 2018.3.14.f1 on Windows 10.
### Image - Unity Install:
![CreateAssetBundle1](/documentation/images/CreateAssetBundle1.PNG)

Unity is installed with the following modules.
### Image - Unity Modules:
![CreateAssetBundle2](/documentation/images/CreateAssetBundle2.PNG)

Microsoft Visual Studio is also installed. The free version works.

## CreateAssetBundle Project
Start out by creating a new Unity project.
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
For the tutorial two game objects are created in the SampleScene, a red cube and a blue sphere.

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
Copy the following lines into CreateiOSAssetBundles and save it.
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
