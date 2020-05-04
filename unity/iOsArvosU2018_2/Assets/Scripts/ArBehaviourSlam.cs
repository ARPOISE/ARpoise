/*
ArBehaviourSlam.cs - MonoBehaviour for Arpoise, slam handling.

Copyright (C) 2019, Tamiko Thiel and Peter Graf - All Rights Reserved

ARPOISE - Augmented Reality Point Of Interest Service 

This file is part of Arpoise.

    Arpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Arpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Arpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
Arpoise, see www.Arpoise.com/

*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if HAS_AR_KIT
using UnityEngine.XR.iOS;
#endif
namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourSlam : ArBehaviourImage
    {
        private readonly List<GameObject> _imageSceneObjects = new List<GameObject>();
        private readonly List<GameObject> _slamSceneObjects = new List<GameObject>();

        #region Globals
#if HAS_AR_KIT
        public GameObject AnchorManager;

        public GameObject PointCloudParticleExample;
        public GameObject GeneratePlanes;
        public GameObject HitAnchor;
        public GameObject DetectAPlaneOverLay;

        private bool _myIsSlam = false;
#endif

#if HAS_AR_CORE
        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject DetectedPlanePrefab;

        // Slam example game objects
        public GameObject PlaneGenerator;
        public GameObject PlaneDiscovery;
        public GameObject PointCloud;
#endif
        #endregion

        #region Start
        protected override void Start()
        {
            base.Start();

#if HAS_AR_KIT
            if (!_imageSceneObjects.Any())
            {
                _imageSceneObjects.Add(AnchorManager);
            }
            if (!_slamSceneObjects.Any())
            {
                _slamSceneObjects.Add(PointCloudParticleExample);
                _slamSceneObjects.Add(GeneratePlanes);
                _slamSceneObjects.Add(HitAnchor);
            }
#endif
#if HAS_AR_CORE
            if (!_slamSceneObjects.Any())
            {
                _slamSceneObjects.Add(DetectedPlanePrefab);
                _slamSceneObjects.Add(PlaneGenerator);
                _slamSceneObjects.Add(PlaneDiscovery);
                _slamSceneObjects.Add(PointCloud);
            }
#endif
        }
        #endregion

        #region Update
        protected override void Update()
        {
            base.Update();

            foreach (var sceneObject in _imageSceneObjects)
            {
                if (sceneObject != null && sceneObject.activeSelf != HasTriggerImages)
                {
                    sceneObject.SetActive(HasTriggerImages);
                    //Debug.Log($"{sceneObject.name} {HasTriggerImages}");
                }
            }
            foreach (var sceneObject in _slamSceneObjects)
            {
                if (sceneObject != null && sceneObject.activeSelf != IsSlam)
                {
                    sceneObject.SetActive(IsSlam);
                    //Debug.Log($"{sceneObject.name} {IsSlam}");

#if HAS_AR_KIT
                    if (sceneObject == GeneratePlanes)
                    {
                        var component = GeneratePlanes.GetComponent<UnityARGeneratePlane>();
                        component?.Update();
                    }
                    if (sceneObject == PointCloudParticleExample)
                    {
                        var component = PointCloudParticleExample.GetComponent<PointCloudParticleExample>();
                        component?.Update();
                    }
#endif
                }
            }
#if HAS_AR_KIT
            if (_myIsSlam != IsSlam)
            {
                _myIsSlam = IsSlam;

                if (DetectAPlaneOverLay != null && DetectAPlaneOverLay.gameObject.activeSelf != IsSlam)
                {
                    DetectAPlaneOverLay.SetActive(IsSlam);
                }
            }
#endif
        }
        #endregion
    }
}
