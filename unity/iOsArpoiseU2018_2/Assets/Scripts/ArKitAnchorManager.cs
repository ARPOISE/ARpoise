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
    public Dictionary<int, TriggerObject> TriggerObjects { get; set; }
    public GameObject FitToScanOverlay { get; set; }
#endif
    // Use this for initialization
    protected void Start()
    {
#if HAS_AR_KIT
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;
#endif
    }

#if HAS_AR_KIT
    private bool _hasTriggerObjects = false;

    private void AddImageAnchor(ARImageAnchor arImageAnchor)
    {
        if (ArBehaviour.IsSlam)
        {
            return;
        }
        //Debug.LogFormat("Anchor added[{0}] : tracked {1}, name '{2}'", arImageAnchor.Identifier, arImageAnchor.IsTracked, arImageAnchor.ReferenceImageName);
        int index;
        if (arImageAnchor.ReferenceImageName != null && int.TryParse(arImageAnchor.ReferenceImageName, out index) && index >= 0)
        {
            TriggerObject triggerObject;
            var triggerObjects = TriggerObjects;
            if (triggerObjects != null && triggerObjects.TryGetValue(index, out triggerObject))
            {
                //Debug.LogFormat("Index {0} trigger object {1}", index, triggerObject != null);

                while (index >= _gameObjects.Count)
                {
                    _gameObjects.Add(null);
                }
                if (triggerObject.layerWebUrl != ArBehaviour.LayerWebUrl)
                {
                    _gameObjects.Add(null);
                    return;
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
                }
            }
        }
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
            //Debug.LogFormat("Index {0} game object {1}", index, gameObjectToAHandle != null);
            if (gameObjectToAHandle != null)
            {
                if (arImageAnchor.IsTracked)
                {
                    gameObjectToAHandle.transform.localPosition = UnityARMatrixOps.GetPosition(arImageAnchor.Transform);
                    gameObjectToAHandle.transform.localRotation = UnityARMatrixOps.GetRotation(arImageAnchor.Transform);
                    if (!gameObjectToAHandle.activeSelf)
                    {
                        gameObjectToAHandle.SetActive(true);
                    }
                }
                else if (gameObjectToAHandle.activeSelf)
                {
                    //gameObjectToAHandle.SetActive(false);
                }
            }
            else if (_hasTriggerObjects)
            {
                //Debug.LogFormat("Anchor re-added[{0}] : tracked {1}, name '{2}'", arImageAnchor.Identifier, arImageAnchor.IsTracked, arImageAnchor.ReferenceImageName);
                if (arImageAnchor.ReferenceImageName != null && int.TryParse(arImageAnchor.ReferenceImageName, out index) && index >= 0)
                {
                    TriggerObject triggerObject;
                    var triggerObjects = TriggerObjects;
                    if (triggerObjects != null && triggerObjects.TryGetValue(index, out triggerObject))
                    {
                        //Debug.LogFormat("Index {0} trigger object {1}", index, triggerObject != null);

                        while (index >= _gameObjects.Count)
                        {
                            _gameObjects.Add(null);
                        }
                        if (triggerObject.layerWebUrl != ArBehaviour.LayerWebUrl)
                        {
                            _gameObjects.Add(null);
                            return;
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
                        }
                    }
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
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent -= AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent -= UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveImageAnchor;
    }

    private void Update()
    {
        var fitToScanOverlay = FitToScanOverlay;
        if (fitToScanOverlay != null)
        {
            var hasActiveObjects = false;
            var triggerObjects = TriggerObjects;
            _hasTriggerObjects = triggerObjects != null && triggerObjects.Values.Any(x => x.isActive);
            if (_hasTriggerObjects)
            {
                hasActiveObjects = _gameObjects.Any(x => x != null && x.activeSelf);
            }
            var setActive = _hasTriggerObjects && !hasActiveObjects && !ArBehaviour.LayerPanelIsActive() && !ArBehaviour.IsSlam;
            if (fitToScanOverlay.activeSelf != setActive)
            {
                fitToScanOverlay.SetActive(setActive);
            }
        }
    }
#endif
}
