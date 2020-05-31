/*
UnityARHitTestExample.cs - Hit test of the ARKit based version of SLAM ARpoise, aka AR-vos.

ARPOISE - Augmented Reality Point Of Interest Service 

This file is part of Arpoise. 

This file is derived from slam example of the Unity-ARKit-Plugin

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
using System.Collections.Generic;
using System.Linq;
using com.arpoise.arpoiseapp;

namespace UnityEngine.XR.iOS
{
    public class UnityARHitTestExample : MonoBehaviour
	{
		private readonly List<GameObject> _gameObjects = new List<GameObject>();

		public ArBehaviourSlam ArBehaviour { get; set; }
		public Dictionary<int, TriggerObject> SlamObjects { get; set; }
        public GameObject SceneAnchor { get; set; }

		public float maxRayDistance = 30.0f;
		public LayerMask collisionLayer = 1 << 10;  //ARKitPlane layer

        private int _slamHitCount = 0;

        bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
            if (hitResults.Count > 0)
            {
                foreach (var hitResult in hitResults)
                {
                    //Debug.Log ("Got a hit!");

                    int index = _slamHitCount++ % SlamObjects.Count;
                    TriggerObject triggerObject = null;
                    if (!SlamObjects.TryGetValue(index, out triggerObject))
                    {
                        return true;
                    }

                    var arObjectState = ArBehaviour.ArObjectState;
                    if (arObjectState != null && triggerObject != null)
                    {
                        GameObject newGameObject;
                        var result = ArBehaviour.CreateArObject(
                            arObjectState,
                            triggerObject.gameObject,
                            null,
                            SceneAnchor.transform,
                            triggerObject.poi,
                            triggerObject.poi.id,
                            out newGameObject
                            );
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            ArBehaviour.ErrorMessage = result;
                            return true;
                        }
                        _gameObjects.Add(newGameObject);
                        newGameObject.transform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                        newGameObject.transform.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                    }

                    return true;
                }
            }
            return false;
        }

		// Update is called once per frame
		void Update () {

            if (!ArBehaviour.IsSlam)
            {
				_slamHitCount = 0;
                foreach (var slamGameObject in _gameObjects)
                {
					GameObject.Destroy(slamGameObject);
                }
				_gameObjects.Clear();
                //Debug.Log("Not IsSlam");
                return;
            }

            if (!SlamObjects.Any())
            {
                //Debug.Log("No SlamObjects");
                return;
            }

            if (ArBehaviour.HasHitOnObject)
            {
                //Debug.Log("HasHitOnObject");
                return;
            }

            /*
			#if UNITY_EDITOR   //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
			if (Input.GetMouseButtonDown (0)) {

				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				
				//we'll try to hit one of the plane collider gameobjects that were generated by the plugin
				//effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
				if (Physics.Raycast (ray, out hit, maxRayDistance, collisionLayer)) {

					//we're going to get the position from the contact point
					m_HitTransform.position = hit.point;
                    Debug.Log("Got ray hit!");
                    Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}",
                        /*m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

					//and the rotation from the transform of the plane collider
					m_HitTransform.rotation = hit.transform.rotation;
				}
			}
			#else
            */
            if (Input.touchCount > 0)
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
				{
					var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
					ARPoint point = new ARPoint {
						x = screenPosition.x,
						y = screenPosition.y
					};

                    // prioritize reults types
                    ARHitTestResultType[] resultTypes = {
						//ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                        // if you want to use infinite planes use this:
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane, 
						//ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane, 
						//ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    }; 
					
                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType (point, resultType))
                        {
                            return;
                        }
                    }
				}
			}
            //#endif

            if (!_gameObjects.Any() && ArBehaviour.IsSlam
                && ArBehaviour.DetectAPlaneOverLay != null && !ArBehaviour.DetectAPlaneOverLay.gameObject.activeSelf)
            {
                ArBehaviour.SetInfoText("Please tap on a plane.");
            }
		}
	}
}
