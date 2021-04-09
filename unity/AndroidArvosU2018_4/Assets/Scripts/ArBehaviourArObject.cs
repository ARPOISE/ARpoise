/*
ArBehaviourArObject.cs - MonoBehaviour for Arpoise AR-object handling.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if HAS_AR_CORE
using GoogleARCore;
#else
#endif

namespace com.arpoise.arpoiseapp
{
    public class TriggerObject
    {
        public bool isActive;
        public int index;
        public string triggerImageURL;
        public Texture2D texture;
        public float width;
        public GameObject gameObject;
        public Poi poi;
        public string layerWebUrl;

        /// <summary>
        /// Record the last time this image was tracked.
        /// </summary>
        public DateTime LastUpdateTime = DateTime.Now;
    }

    public class ArBehaviourArObject : ArBehaviourPosition
    {
        #region Globals
        public string LayerWebUrl { get; protected set; }
        public readonly Dictionary<int, TriggerObject> TriggerObjects = new Dictionary<int, TriggerObject>();
        public GameObject Wrapper = null;
        #endregion

        #region Protecteds
#if HAS_AR_CORE
        protected AugmentedImageDatabase AugmentedImageDatabase;
#endif
        protected bool HasTriggerImages = false;
        protected string InformationMessage = null;
        protected bool ShowInfo = false;
        protected float RefreshInterval = 0;
        protected readonly Dictionary<string, List<ArLayer>> InnerLayers = new Dictionary<string, List<ArLayer>>();
        protected readonly Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();
        protected readonly Dictionary<string, Texture2D> TriggerImages = new Dictionary<string, Texture2D>();
        protected readonly List<TriggerObject> SlamObjects = new List<TriggerObject>();
        #endregion

        public GameObject CreateObject(GameObject objectToAdd)
        {
            return Instantiate(objectToAdd);
        }

        #region ArObjects
        private int _bleachingValue = -1;

        // Link ar object to ar object state or to parent object
        private string LinkArObject(ArObjectState arObjectState, ArObject parentObject, Transform parentTransform, ArObject arObject, GameObject arGameObject, Poi poi)
        {
            if (parentObject == null)
            {
                // Add to ar object state
                arObjectState.Add(arObject);

                List<ArLayer> innerLayers = null;
                if (!string.IsNullOrWhiteSpace(poi.InnerLayerName) && InnerLayers.TryGetValue(poi.InnerLayerName, out innerLayers))
                {
                    foreach (var layer in innerLayers.Where(x => x.hotspots != null))
                    {
                        var result = CreateArObjects(arObjectState, arObject, parentTransform, layer.hotspots);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            else
            {
                parentObject.GameObjects.Add(arGameObject);
                parentObject.ArObjects.Add(arObject);
            }
            return null;
        }

        private int _abEvolutionOfFishIndex = 0;

        // Create ar object for a poi and link it
        public string CreateArObject(
            ArObjectState arObjectState,
            GameObject objectToAdd,
            ArObject parentObject,
            Transform parentObjectTransform,
            Poi poi,
            long arObjectId,
            out GameObject createdObject
            )
        {
            createdObject = null;
            var objectName = objectToAdd.name;

            // Create a copy of the object
            if (string.IsNullOrWhiteSpace(poi.LindenmayerString))
            {
                objectToAdd = CreateObject(objectToAdd);
            }
            else
            {
                GameObject leafToAdd = null;
                var leafPrefab = poi.LeafPrefab;
                if (!string.IsNullOrWhiteSpace(leafPrefab) && !string.IsNullOrWhiteSpace(poi.BaseUrl))
                {
                    AssetBundle assetBundle;
                    if (AssetBundles.TryGetValue(poi.BaseUrl, out assetBundle))
                    {
                        leafToAdd = assetBundle.LoadAsset<GameObject>(leafPrefab);
                    }
                }

                objectToAdd = ArCreature.Create(
                    poi.LindenmayerDerivations,
                    poi.LindenmayerString,
                    Wrapper,
                    objectToAdd,
                    leafToAdd,
                    poi.LindenmayerAngle,
                    poi.LindenmayerFactor,
                    parentObjectTransform
                    );
            }

            if (objectToAdd == null)
            {
                return "Instantiate(" + objectName + ") failed";
            }
            
            if ("EvolutionOfFish".Equals(objectName))
            {
                var evolutionOfFish = objectToAdd.GetComponent<EvolutionOfFish>();
                if (evolutionOfFish != null)
                {
                    evolutionOfFish.ArCamera = ArCamera;
                }
            }
            else if ("AB_EvolutionOfFish".Equals(objectName))
            {
                var evolutionOfFish = objectToAdd.GetComponent<AbEvolutionOfFish>();
                if (evolutionOfFish != null)
                {
                    evolutionOfFish.Index = _abEvolutionOfFishIndex++ % 2;
                    evolutionOfFish.ArCamera = ArCamera;

                    foreach (var action in poi.actions)
                    {
                        evolutionOfFish.SetParameter(action.showActivity, action.label, action.activityMessage);
                    }
                }
            }

            // All objects are below the scene anchor or the parent
            var parentTransform = parentObjectTransform;

            // Wrap the object into a wrapper, so it can be moved around when the device moves
            var wrapper = Instantiate(Wrapper);
            if (wrapper == null)
            {
                return "Instantiate(TransformWrapper) failed";
            }
            wrapper.name = "TransformWrapper";
            createdObject = wrapper;
            wrapper.transform.parent = parentTransform;
            parentTransform = wrapper.transform;

            // Add a wrapper for scaling
            var scaleWrapper = Instantiate(Wrapper);
            if (scaleWrapper == null)
            {
                return "Instantiate(ScaleWrapper) failed";
            }
            scaleWrapper.name = "ScaleWrapper";
            scaleWrapper.transform.parent = parentTransform;
            parentTransform = scaleWrapper.transform;

            // Prepare the relative rotation of the object - billboard handling
            if (poi.transform != null && poi.transform.rel)
            {
                var billboardWrapper = Instantiate(Wrapper);
                if (billboardWrapper == null)
                {
                    return "Instantiate(BillboardWrapper) failed";
                }
                billboardWrapper.name = "BillboardWrapper";
                billboardWrapper.transform.parent = parentTransform;
                parentTransform = billboardWrapper.transform;
                arObjectState.AddBillboardAnimation(new ArAnimation(arObjectId, billboardWrapper, objectToAdd, null, true));
            }

            // Prepare the rotation of the object
            GameObject rotationWrapper = null;
            if (poi.transform != null && poi.transform.angle != 0)
            {
                rotationWrapper = Instantiate(Wrapper);
                if (rotationWrapper == null)
                {
                    return "Instantiate(RotationWrapper) failed";
                }
                rotationWrapper.name = "RotationWrapper";
                rotationWrapper.transform.parent = parentTransform;
                parentTransform = rotationWrapper.transform;
            }

            // Look at the animations present for the object
            if (poi.animations != null)
            {
                if (poi.animations.onCreate != null)
                {
                    foreach (var poiAnimation in poi.animations.onCreate)
                    {
                        // Put the animation into a wrapper
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(OnCreateWrapper) failed";
                        }
                        animationWrapper.name = "OnCreateWrapper";
                        arObjectState.AddOnCreateAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, true));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }

                if (poi.animations.onFocus != null)
                {
                    foreach (var poiAnimation in poi.animations.onFocus)
                    {
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(OnFocusWrapper) failed";
                        }
                        animationWrapper.name = "OnFocusWrapper";
                        arObjectState.AddOnFocusAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, false));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }

                if (poi.animations.inFocus != null)
                {
                    foreach (var poiAnimation in poi.animations.inFocus)
                    {
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(InFocusWrapper) failed";
                        }
                        animationWrapper.name = "InFocusWrapper";
                        arObjectState.AddInFocusAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, false));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }

                if (poi.animations.onClick != null)
                {
                    foreach (var poiAnimation in poi.animations.onClick)
                    {
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(OnClickWrapper) failed";
                        }
                        animationWrapper.name = "OnClickWrapper";
                        arObjectState.AddOnClickAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, false));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }

                if (poi.animations.onFollow != null)
                {
                    foreach (var poiAnimation in poi.animations.onFollow)
                    {
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(OnFollowWrapper) failed";
                        }
                        animationWrapper.name = "OnFollowWrapper";
                        arObjectState.AddOnFollowAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, false));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }
            }

            // Put the game object into the scene or link it to the parent
            objectToAdd.transform.parent = parentTransform;

            // Set the name of the instantiated game object
            objectToAdd.name = poi.title;

            // Scale the scaleWrapper
            if (poi.transform != null && poi.transform.scale != 0.0)
            {
                scaleWrapper.transform.localScale = new Vector3(poi.transform.scale, poi.transform.scale, poi.transform.scale);
            }
            else
            {
                return "Could not set scale " + ((poi.transform == null) ? "null" : string.Empty + poi.transform.scale);
            }

            // Rotate the rotationWrapper
            if (rotationWrapper != null)
            {
                rotationWrapper.transform.localEulerAngles = new Vector3(0, poi.transform.angle, 0);
            }

            // Relative to user, parent or with absolute coordinates
            var relativePosition = poi.poiObject.relativeLocation;

            if (parentObject != null || !string.IsNullOrWhiteSpace(relativePosition))
            {
                // Relative to user or parent
                var relativeLocation = poi.poiObject.RelativeLocation;
                var xOffset = relativeLocation[0];
                var yOffset = relativeLocation[1];
                var zOffset = relativeLocation[2];
                var arObject = new ArObject(
                    poi, arObjectId, poi.title, objectToAdd.name, poi.BaseUrl, wrapper, objectToAdd,
                    poi.Latitude, poi.Longitude, poi.relativeAlt + yOffset, true);

                var result = LinkArObject(arObjectState, parentObject, parentTransform, arObject, objectToAdd, poi);
                if (result != null)
                {
                    return result;
                }

                arObject.WrapperObject.transform.position = arObject.TargetPosition = new Vector3(xOffset, arObject.RelativeAltitude, zOffset);

                if ((!string.IsNullOrWhiteSpace(poi?.title) && poi.title.Contains("bleached"))
                    || (!string.IsNullOrWhiteSpace(parentObject?.Text) && parentObject.Text.Contains("bleached")))
                {
                    arObject.SetBleachingValue(85);
                }
                else if (_bleachingValue >= 0)
                {
                    arObject.SetBleachingValue(_bleachingValue);
                }
            }
            else
            {
                // Absolute lat/lon coordinates
                float filteredLatitude = UsedLatitude;
                float filteredLongitude = UsedLongitude;

                var distance = CalculateDistance(poi.Latitude, poi.Longitude, filteredLatitude, filteredLongitude);
                if (distance <= ((poi.ArLayer != null) ? poi.ArLayer.visibilityRange : 1500))
                {
                    var arObject = new ArObject(
                        poi, arObjectId, poi.title, objectToAdd.name, poi.BaseUrl, wrapper, objectToAdd,
                        poi.Latitude, poi.Longitude, poi.relativeAlt, false);

                    var result = LinkArObject(arObjectState, parentObject, parentTransform, arObject, objectToAdd, poi);
                    if (result != null)
                    {
                        return result;
                    }

                    if (!string.IsNullOrWhiteSpace(poi?.title) && poi.title.Contains("bleached"))
                    {
                        arObject.SetBleachingValue(85);
                    }
                    else if (_bleachingValue >= 0)
                    {
                        arObject.SetBleachingValue(_bleachingValue);
                    }
                }
            }
            return null;
        }

        protected string CreateArObject(ArObjectState arObjectState, ArObject parentObject, Transform parentObjectTransform, Poi poi, long arObjectId)
        {
            string assetBundleUrl = poi.BaseUrl;
            if (string.IsNullOrWhiteSpace(assetBundleUrl))
            {
                return "Poi with id " + poi.id + ", empty asset bundle url";
            }

            AssetBundle assetBundle = null;
            if (!AssetBundles.TryGetValue(assetBundleUrl, out assetBundle))
            {
                return "?: '" + assetBundleUrl + "'";
            }

            string objectName = poi.GameObjectName;
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            var objectToAdd = assetBundle.LoadAsset<GameObject>(objectName);
            if (objectToAdd == null)
            {
                return "Poi with id " + poi.id + ", unknown game object: '" + objectName + "'";
            }

            var triggerImageURL = poi.TriggerImageURL;
            if (!string.IsNullOrWhiteSpace(triggerImageURL))
            {
                try
                {
                    var isSlamUrl = IsSlamUrl(triggerImageURL);
                    Texture2D texture = null;
                    if (!TriggerImages.TryGetValue(triggerImageURL, out texture) || texture == null)
                    {
                        if (!isSlamUrl)
                        {
                            return "?t " + triggerImageURL;
                        }
                    }

                    var t = isSlamUrl ? null
                        : TriggerObjects.Values.FirstOrDefault(x => x.triggerImageURL == triggerImageURL);
                    if (t == null)
                    {
                        int newIndex = isSlamUrl ? SlamObjects.Count : TriggerObjects.Count;
#if HAS_AR_CORE
                        if (!isSlamUrl)
                        {
                            newIndex = AugmentedImageDatabase.Count;
                        }
#endif
                        var width = poi.poiObject.triggerImageWidth;
                        t = new TriggerObject
                        {
                            isActive = true,
                            index = newIndex,
                            triggerImageURL = triggerImageURL,
                            texture = texture,
                            width = width,
                            gameObject = objectToAdd,
                            poi = poi,
                            layerWebUrl = LayerWebUrl
                        };
                        if (isSlamUrl)
                        {
                            SlamObjects.Add(t);
                        }
                        else
                        {
                            TriggerObjects[t.index] = t;
                        }
#if HAS_AR_CORE
                        if (!isSlamUrl)
                        {
                            AugmentedImageDatabase.AddImage(triggerImageURL, texture, width);
                        }
#endif
                    }
                    else
                    {
                        t.isActive = true;
                        t.triggerImageURL = triggerImageURL;
                        t.gameObject = objectToAdd;
                        t.poi = poi;
                        t.layerWebUrl = LayerWebUrl;
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                GameObject newObject;
                var result = CreateArObject(
                    arObjectState,
                    objectToAdd,
                    parentObject,
                    parentObjectTransform,
                    poi,
                    arObjectId,
                    out newObject
                    );
                if (!string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }
            }
            return null;
        }

        // Create ar objects for the pois and link them
        protected string CreateArObjects(ArObjectState arObjectState, ArObject parentObject, Transform parentObjectTransform, IEnumerable<Poi> pois)
        {
            foreach (var poi in pois.Where(x => x.isVisible && !string.IsNullOrWhiteSpace(x.GameObjectName)))
            {
                long arObjectId = poi.id;
                if (parentObject != null)
                {
                    arObjectId = -1000000 * parentObject.Id - arObjectId;
                }

                var result = CreateArObject(arObjectState, parentObject, parentObjectTransform, poi, arObjectId);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }
            }
            foreach (var triggerObject in TriggerObjects.Values)
            {
                triggerObject.isActive = triggerObject.layerWebUrl == LayerWebUrl;
            }
            HasTriggerImages = TriggerObjects.Values.Any(x => x.isActive);
            return null;
        }

        // Create ar objects from layers
        protected ArObjectState CreateArObjectState(List<ArObject> existingArObjects, List<ArLayer> layers)
        {
            var arObjectState = new ArObjectState();
            var pois = new List<Poi>();

            bool showInfo = false;
            string informationMessage = null;
            float refreshInterval = 0;
            int bleachingValue = -1;
            int areaSize = -1;
            int areaWidth = -1;
            bool applyKalmanFilter = true;

            foreach (var layer in layers)
            {
                if (applyKalmanFilter && !layer.applyKalmanFilter)
                {
                    applyKalmanFilter = layer.applyKalmanFilter;
                }

                if (bleachingValue < layer.bleachingValue)
                {
                    bleachingValue = layer.bleachingValue;
                }

                if (areaSize < layer.areaSize)
                {
                    areaSize = layer.areaSize;
                }
                if (areaWidth < layer.areaWidth)
                {
                    areaWidth = layer.areaWidth;
                }

                if (refreshInterval <= 0 && layer.refreshInterval >= 1)
                {
                    refreshInterval = layer.refreshInterval;
                }

                if (layer.actions != null)
                {
                    if (!showInfo)
                    {
                        showInfo = layer.actions.FirstOrDefault(x => x.showActivity) != null;
                    }
                    if (informationMessage == null)
                    {
                        informationMessage = layer.actions.Select(x => x.activityMessage).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                    }
                }

                if (layer.hotspots == null)
                {
                    continue;
                }
                var layerPois = layer.hotspots.Where(x => x.isVisible && !string.IsNullOrWhiteSpace(x.GameObjectName) && (x.ArLayer = layer) == layer);
                pois.AddRange(layerPois.Where(x => CalculateDistance(x.Latitude, x.Longitude, UsedLatitude, UsedLongitude) <= layer.visibilityRange));
            }

            ApplyKalmanFilter = applyKalmanFilter;
            InformationMessage = informationMessage;
            ShowInfo = showInfo;
            if (refreshInterval >= 1)
            {
                RefreshInterval = refreshInterval;
            }

            bool setBleachingValues = false;
            if (_bleachingValue != bleachingValue)
            {
                if (bleachingValue >= 0)
                {
                    setBleachingValues = true;
                    _bleachingValue = bleachingValue;
                    if (_bleachingValue > 100)
                    {
                        _bleachingValue = 100;
                    }
                }
                else
                {
                    _bleachingValue = -1;
                }
            }

            if (AreaSize != areaSize)
            {
                AreaSize = areaSize;
            }
            if (AreaWidth != areaWidth)
            {
                AreaWidth = areaWidth;
            }

            if (existingArObjects != null)
            {
                foreach (var arObject in existingArObjects)
                {
                    var poi = pois.FirstOrDefault(x => arObject.Id == x.id
                                               && arObject.GameObjectName.Equals(x.GameObjectName)
                                               && (string.IsNullOrWhiteSpace(x.BaseUrl) || arObject.BaseUrl.Equals(x.BaseUrl))
                              );
                    if (poi == null)
                    {
                        arObjectState.ArObjectsToDelete.Add(arObject);
                    }
                    else
                    {
                        if (setBleachingValues && _bleachingValue >= 0)
                        {
                            arObject.SetBleachingValue(_bleachingValue);
                        }

                        if (poi.Latitude != arObject.Latitude)
                        {
                            arObject.Latitude = poi.Latitude;
                            arObject.IsDirty = true;
                        }
                        if (poi.Longitude != arObject.Longitude)
                        {
                            arObject.Longitude = poi.Longitude;
                            arObject.IsDirty = true;
                        }
                    }
                }
            }

            foreach (var poi in pois)
            {
                if (existingArObjects != null)
                {
                    string objectName = poi.GameObjectName;
                    if (string.IsNullOrWhiteSpace(objectName))
                    {
                        continue;
                    }

                    string baseUrl = poi.BaseUrl;
                    if (!string.IsNullOrWhiteSpace(baseUrl))
                    {
                        while (baseUrl.Contains('\\'))
                        {
                            baseUrl = baseUrl.Replace("\\", string.Empty);
                        }
                    }

                    if (existingArObjects.Any(
                        x => poi.id == x.Id
                        && objectName.Equals(x.GameObjectName)
                        && baseUrl.Equals(x.BaseUrl)))
                    {
                        continue;
                    }
                }
                arObjectState.ArPois.Add(poi);
            }
            return arObjectState;
        }
        #endregion

        #region Misc

        protected bool IsSlamUrl(string url)
        {
            return !string.IsNullOrWhiteSpace(url) && "slam".Equals(url.ToLower().Trim());
        }
        #endregion
    }
}
