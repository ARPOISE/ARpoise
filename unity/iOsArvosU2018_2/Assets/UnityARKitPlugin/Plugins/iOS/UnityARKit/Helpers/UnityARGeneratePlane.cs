/*
UnityARGeneratePlane.cs - Generate Planes for the ARKit based version of SLAM ARpoise, aka AR-vos.

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

namespace UnityEngine.XR.iOS
{
    public class UnityARGeneratePlane : MonoBehaviour
	{
		public GameObject planePrefab;
        private UnityARAnchorManager unityARAnchorManager;

		// Use this for initialization
		void Start () {
            unityARAnchorManager = new UnityARAnchorManager();
			UnityARUtility.InitializePlanePrefab (planePrefab);
		}

        public void Update()
        {
            if (unityARAnchorManager != null)
            {
                foreach (var planeAnchor in unityARAnchorManager.GetCurrentPlaneAnchors())
                {
                    if (planeAnchor.gameObject.activeSelf != gameObject.activeSelf)
                    {
                        planeAnchor.gameObject.SetActive(gameObject.activeSelf);
                    }
                }
            }
        }

        void OnDestroy()
        {
            unityARAnchorManager.Destroy ();
        }

        void OnGUI()
        {
            /*
			IEnumerable<ARPlaneAnchorGameObject> arpags = unityARAnchorManager.GetCurrentPlaneAnchors ();
			foreach(var planeAnchor in arpags)
			{
                //ARPlaneAnchor ap = planeAnchor;
                //GUI.Box (new Rect (100, 100, 800, 60), string.Format ("Center: x:{0}, y:{1}, z:{2}", ap.center.x, ap.center.y, ap.center.z));
                //GUI.Box(new Rect(100, 200, 800, 60), string.Format ("Extent: x:{0}, y:{1}, z:{2}", ap.extent.x, ap.extent.y, ap.extent.z));
            }
            */
        }
	}
}

