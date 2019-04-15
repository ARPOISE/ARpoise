/*
ArKitAnchorManager.cs - Anchor manager of the ARKit based version of image trigger ARpoise, i.e. ARvos.

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
using UnityEngine;

#if HAS_AR_KIT
using UnityEngine.XR.iOS;
#endif
public class ArKitAnchorManager : MonoBehaviour
{
    private List<GameObject> _gameObjects = new List<GameObject>();

    public Dictionary<int, TriggerObject> TriggerObjects { get; set; }
    public ArBehaviourImage ArBehaviour { get; set; }

    // Use this for initialization
    private void Start()
    {
#if HAS_AR_KIT
        UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
        UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;
#endif
    }
#if HAS_AR_KIT
    private void AddImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor added[{0}] : tracked => {1}", arImageAnchor.Identifier, arImageAnchor.IsTracked);
        int index;
        if (arImageAnchor.ReferenceImageName != null && int.TryParse(arImageAnchor.ReferenceImageName, out index) && index >= 0)
        {
            TriggerObject triggerObject;
            if (TriggerObjects != null && TriggerObjects.TryGetValue(index, out triggerObject))
            {
                Debug.LogFormat("Index {0} trigger object {1}", index, triggerObject != null);

                while (index >= _gameObjects.Count)
                {
                    _gameObjects.Add(null);
                }

                var arObjectState = ArBehaviour.ArObjectState;
                if (arObjectState != null && _gameObjects[index] == null)
                {
                    lock (arObjectState)
                    {
                        GameObject gameObject;
                        var result = ArBehaviour.CreateArObject(
                            arObjectState,
                            triggerObject.gameObject,
                            null,
                            transform,
                            triggerObject.poi,
                            triggerObject.poi.id,
                            out gameObject
                            );
                        if (!ArBehaviourPosition.IsEmpty(result))
                        {
                            ArBehaviour.ErrorMessage = result;
                            return;
                        }
                        _gameObjects[index] = gameObject;
                    }
                }
            }
        }
    }

    private void UpdateImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor updated[{0}] : tracked => {1}", arImageAnchor.Identifier, arImageAnchor.IsTracked);
        int index;
        if (arImageAnchor.ReferenceImageName != null && int.TryParse(arImageAnchor.ReferenceImageName, out index) && index >= 0)
        {
            GameObject gameObject = null;
            if (index < _gameObjects.Count)
            {
                gameObject = _gameObjects[index];
            }
            if (gameObject != null)
            {
                if (arImageAnchor.IsTracked)
                {
                    gameObject.transform.position = UnityARMatrixOps.GetPosition(arImageAnchor.Transform);
                    gameObject.transform.rotation = UnityARMatrixOps.GetRotation(arImageAnchor.Transform);
                    if (!gameObject.activeSelf)
                    {
                        gameObject.SetActive(true);
                    }
                }
                else if (gameObject.activeSelf)
                {
                    //_imageAnchorGO.SetActive(false);
                }
            }
        }
    }

    private void RemoveImageAnchor(ARImageAnchor arImageAnchor)
    {
        Debug.LogFormat("image anchor removed[{0}] : tracked => {1}", arImageAnchor.Identifier, arImageAnchor.IsTracked);
        int index;
        if (arImageAnchor.ReferenceImageName != null && int.TryParse(arImageAnchor.ReferenceImageName, out index) && index >= 0)
        {
            GameObject gameObject = null;
            if (index < _gameObjects.Count)
            {
                gameObject = _gameObjects[index];
            }
            if (gameObject != null)
            {
                GameObject.Destroy(gameObject);
                gameObject = null;
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
    }
#endif
}
