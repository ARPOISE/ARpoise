/*
ArKitAnchorManager.cs - Anchor manager of the ARKit based version of image trigger ARpoise, aka AR-vos.

ARPOISE - Augmented Reality Point Of Interest Service 

This file is part of Arpoise. 

This file is derived from image trigger example of the Unity-ARKit-Plugin

https://bitbucket.org/Unity-Technologies/unity-arkit-plugin

The license of this project says:

All contents of this repository 
except for the contents of  the /Assets/UnityARKitPlugin/Examples/FaceTracking/SlothCharacter folder and its subfolders 
are released under the MIT License, which is listed under /LICENSES/MIT_LICENSE file.

The MIT License (MIT)

Copyright (c) 2017, Unity Technologies

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
Arpoise, see www.Arpoise.com/

*/

using com.arpoise.arpoiseapp;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if HAS_AR_KIT
using UnityEngine.XR.iOS;
#endif
public class ArKitAnchorManager : MonoBehaviour
{
#if HAS_AR_KIT
    private readonly List<GameObject> _gameObjects = new List<GameObject>();

    public ArBehaviourImage ArBehaviour { get; set; }
    public GameObject FitToScanOverlay { get; set; }

    // Use this for initialization
    protected void Start()
    {
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;
    }

    private void UpdateImageAnchor(ARImageAnchor arImageAnchor)
    {
        if (ArBehaviour.IsSlam)
        {
            return;
        }
        //Debug.LogFormat("Anchor updated[{0}] : tracked {1}, name '{2}' GOs {3}",
        //    arImageAnchor.Identifier, arImageAnchor.IsTracked, arImageAnchor.ReferenceImageName, _gameObjects.Count);
        int index;
        if (arImageAnchor.ReferenceImageName != null && int.TryParse(arImageAnchor.ReferenceImageName, out index) && index >= 0)
        {
            GameObject gameObjectToAHandle = null;
            if (index < _gameObjects.Count)
            {
                gameObjectToAHandle = _gameObjects[index];
            }
                    
            TriggerObject triggerObject = null;
            ArBehaviour.TriggerObjects.TryGetValue(index, out triggerObject);
            //Debug.LogFormat("Index {0}, GO {1}, TO {2}, tracked {3}", index, gameObjectToAHandle != null, triggerObject != null, arImageAnchor.IsTracked);

            if (gameObjectToAHandle != null)
            {
                if (triggerObject == null || !triggerObject.isActive || triggerObject.layerWebUrl != ArBehaviour.LayerWebUrl) 
                {
                    if (gameObjectToAHandle.activeSelf)
                    {
                        gameObjectToAHandle.SetActive(false);
                    }
                }
                else if (arImageAnchor.IsTracked)
                {
                    gameObjectToAHandle.transform.localPosition = UnityARMatrixOps.GetPosition(arImageAnchor.Transform);
                    gameObjectToAHandle.transform.localRotation = UnityARMatrixOps.GetRotation(arImageAnchor.Transform);
                    if (!gameObjectToAHandle.activeSelf)
                    {
                        gameObjectToAHandle.SetActive(true);
                    }
                    triggerObject.LastUpdateTime = DateTime.Now;
                }
            }
            else if (triggerObject != null)
            {
                if (!triggerObject.isActive || triggerObject.layerWebUrl != ArBehaviour.LayerWebUrl)
                {
                    return;
                }
                while (index >= _gameObjects.Count)
                {
                    _gameObjects.Add(null);
                }

                var arObjectState = ArBehaviour.ArObjectState;
                if (arObjectState != null && _gameObjects[index] == null)
                {
                    GameObject newGameObject;
                        var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        triggerObject.gameObject,
                        null,
                        transform,
                        triggerObject.poi,
                        triggerObject.poi.id,
                        out newGameObject
                        );
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        ArBehaviour.ErrorMessage = result;
                        return;
                    }
                    _gameObjects[index] = newGameObject;
                    newGameObject.SetActive(true);
                    triggerObject.LastUpdateTime = DateTime.Now;
                }
            }
        }
    }

    private void RemoveImageAnchor(ARImageAnchor arImageAnchor)
    {
        //Debug.LogFormat("Anchor removed[{0}] : tracked {1}, name '{2}'", arImageAnchor.Identifier, arImageAnchor.IsTracked, arImageAnchor.ReferenceImageName);
        int index;
        if (arImageAnchor.ReferenceImageName != null && int.TryParse(arImageAnchor.ReferenceImageName, out index) && index >= 0)
        {
            GameObject gameObjectToAHandle = null;
            if (index < _gameObjects.Count)
            {
                gameObjectToAHandle = _gameObjects[index];
            }
            if (gameObjectToAHandle != null)
            {
                if (gameObjectToAHandle.activeSelf)
                {
                    gameObjectToAHandle.SetActive(false);
                }
            }
        }
    }

    private void OnDestroy()
    {
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent -= UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveImageAnchor;
    }

    public void Update()
    {
        var fitToScanOverlay = FitToScanOverlay;
        if (fitToScanOverlay != null)
        {
            var hasActiveObjects = false;
            var hasTriggerObjects = !ArBehaviour.IsSlam && ArBehaviour.TriggerObjects.Values.Any(x => x.isActive);
            if (hasTriggerObjects)
            {
                hasActiveObjects = _gameObjects.Any(x => x != null && x.activeSelf);
            }
            var setActive = hasTriggerObjects && !hasActiveObjects && !ArBehaviour.LayerPanelIsActive && !ArBehaviour.IsSlam;
            if (fitToScanOverlay.activeSelf != setActive)
            {
                fitToScanOverlay.SetActive(setActive);
            }
        }

        for (int index = 0; index < _gameObjects.Count; index++)
        {
            var gameObjectToAHandle = _gameObjects[index];
            if (gameObjectToAHandle == null || !gameObjectToAHandle.activeSelf)
            {
                continue;
            }
            //Debug.LogFormat("Index {0}, GO {1}", index, gameObjectToAHandle.activeSelf);

            TriggerObject triggerObject = null;
            if (!ArBehaviour.IsSlam && ArBehaviour.TriggerObjects.TryGetValue(index, out triggerObject))
            {
                var isActive = true;
                if (!triggerObject.isActive || triggerObject.layerWebUrl != ArBehaviour.LayerWebUrl)
                {
                    isActive = false;
                }
                else if (triggerObject?.poi != null)
                {
                    var trackingTimeout = triggerObject.poi.TrackingTimeout;
                    if (trackingTimeout > 0)
                    {
                        if (triggerObject.LastUpdateTime.AddMilliseconds(trackingTimeout) < DateTime.Now)
                        {
                            isActive = false;
                        }
                    }
                }
                if (!isActive)
                {
                    gameObjectToAHandle.SetActive(false);
                }
            }
            else
            {
                gameObjectToAHandle.SetActive(false);
            }
        }
    }
#endif
}
