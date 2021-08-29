/*
ArBehaviourArObject.cs - MonoBehaviour for ARpoise ArObject handling.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

ARpoise - Augmented Reality point of interest service environment 

This file is part of ARpoise.

    ARpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ARpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ARpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

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
        public static int FramesPerSecond = 30;
        public GameObject SceneAnchor = null;
        public string LayerWebUrl { get; protected set; }
        public readonly Dictionary<int, TriggerObject> TriggerObjects = new Dictionary<int, TriggerObject>();
        public GameObject Wrapper = null;
        public void RequestRefresh(RefreshRequest refreshRequest) { RefreshRequest = refreshRequest; }
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
        protected volatile RefreshRequest RefreshRequest = null;
        protected long NowTicks { get; private set; }
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

        private GameObject GetWrapper(Dictionary<string, GameObject> wrappers, PoiAnimation animation)
        {
            int index = animation.name.IndexOf("/");
            if (index >= 0)
            {
                GameObject wrapper;
                string key = animation.name.Substring(0, index + 1);
                if (wrappers.TryGetValue(key, out wrapper))
                {
                    return wrapper;
                }
                wrapper = Instantiate(Wrapper);
                if (wrapper != null)
                {
                    wrappers[key] = wrapper;
                }
                return wrapper;
            }
            return Instantiate(Wrapper);
        }

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
                        evolutionOfFish.SetParameter(action.showActivity, action.label.Trim(), action.activityMessage);
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
                var wrappers = new Dictionary<string, GameObject>();

                if (poi.animations.onCreate != null)
                {
                    foreach (var poiAnimation in poi.animations.onCreate)
                    {

                        // Put the animation into a wrapper
                        var animationWrapper = GetWrapper(wrappers, poiAnimation);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(OnCreateWrapper) failed";
                        }
                        arObjectState.AddOnCreateAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, true));
                        if (animationWrapper.transform.parent == null)
                        {
                            animationWrapper.name = "OnCreateWrapper";
                            animationWrapper.transform.parent = parentTransform;
                            parentTransform = animationWrapper.transform;
                        }
                    }
                }

                if (poi.animations.onFocus != null)
                {
                    foreach (var poiAnimation in poi.animations.onFocus)
                    {
                        var animationWrapper = GetWrapper(wrappers, poiAnimation);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(OnFocusWrapper) failed";
                        }
                        arObjectState.AddOnFocusAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, false));
                        if (animationWrapper.transform.parent == null)
                        {
                            animationWrapper.name = "OnFocusWrapper";
                            animationWrapper.transform.parent = parentTransform;
                            parentTransform = animationWrapper.transform;
                        }
                    }
                }

                if (poi.animations.inFocus != null)
                {
                    foreach (var poiAnimation in poi.animations.inFocus)
                    {
                        var animationWrapper = GetWrapper(wrappers, poiAnimation);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(InFocusWrapper) failed";
                        }
                        arObjectState.AddInFocusAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, false));
                        if (animationWrapper.transform.parent == null)
                        {
                            animationWrapper.name = "InFocusWrapper";
                            animationWrapper.transform.parent = parentTransform;
                            parentTransform = animationWrapper.transform;
                        }
                    }
                }

                if (poi.animations.onClick != null)
                {
                    foreach (var poiAnimation in poi.animations.onClick)
                    {
                        var animationWrapper = GetWrapper(wrappers, poiAnimation);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(OnClickWrapper) failed";
                        }
                        arObjectState.AddOnClickAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, false));
                        if (animationWrapper.transform.parent == null)
                        {
                            animationWrapper.name = "OnClickWrapper";
                            animationWrapper.transform.parent = parentTransform;
                            parentTransform = animationWrapper.transform;
                        }
                    }
                }

                if (poi.animations.onFollow != null)
                {
                    foreach (var poiAnimation in poi.animations.onFollow)
                    {
                        var animationWrapper = GetWrapper(wrappers, poiAnimation);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(OnFollowWrapper) failed";
                        }
                        arObjectState.AddOnFollowAnimation(new ArAnimation(arObjectId, animationWrapper, objectToAdd, poiAnimation, false));
                        if (animationWrapper.transform.parent == null)
                        {
                            animationWrapper.name = "OnFollowWrapper";
                            animationWrapper.transform.parent = parentTransform;
                            parentTransform = animationWrapper.transform;
                        }
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
                if (distance <= PositionTolerance * ((poi.ArLayer != null) ? poi.ArLayer.visibilityRange : 1500))
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

        private string CreateArObject(ArObjectState arObjectState, ArObject parentObject, Transform parentObjectTransform, Poi poi, long arObjectId)
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
            float positionUpdateInterval = 0;
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
                        showInfo = layer.ShowInfo;
                    }
                    if (informationMessage == null)
                    {
                        informationMessage = layer.InformationMessage;
                    }
                    if (positionUpdateInterval <= 0)
                    {
                        positionUpdateInterval = layer.PositionUpdateInterval;
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
            PositionUpdateInterval = positionUpdateInterval;
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

        #region Update
        private static long _arObjectId = -1000000000;
        private static readonly System.Random _random = new System.Random();
        protected override void Update()
        {
            NowTicks = DateTime.Now.Ticks;
            base.Update();
        }
        protected bool UpdateArObjects()
        {
            var arObjectState = ArObjectState;
            if (arObjectState.IsDirty)
            {
                if (arObjectState.ArObjectsToDelete.Any())
                {
                    arObjectState.DestroyArObjects();
                }
                if (arObjectState.ArPois.Any())
                {
                    CreateArObjects(arObjectState, null, SceneAnchor.transform, arObjectState.ArPois);
                    arObjectState.ArPois.Clear();
                }
                arObjectState.SetArObjectsToPlace();
                arObjectState.IsDirty = false;
                foreach (var triggerObject in TriggerObjects.Values)
                {
                    triggerObject.isActive = triggerObject.layerWebUrl == LayerWebUrl;
                }
                HasTriggerImages = TriggerObjects.Values.Any(x => x.isActive);
            }
            var result = arObjectState.HandleAnimations(this, StartTicks, NowTicks);
            DuplicateArObjects(arObjectState);

            // Place the ar objects
            PlaceArObjects(arObjectState);
            return result;
        }

        protected bool CheckDistance()
        {
            var filteredLatitude = UsedLatitude;
            var filteredLongitude = UsedLongitude;

            var absoluteArObjects = ArObjectState.ArObjectsToPlace;
            if (absoluteArObjects != null)
            {
                if (absoluteArObjects.Where(x => x.WrapperObject.activeSelf).Any())
                {
                    return true;
                }
            }
            var relativeArObjects = ArObjectState.ArObjectsRelative;
            if (relativeArObjects != null)
            {
                var activeObjects = relativeArObjects.Where(x => x.WrapperObject.activeSelf).ToList();
                foreach (var arObject in activeObjects)
                {
                    if (arObject.Poi.visibilityRange > 0)
                    {
                        var distance = CalculateDistance(arObject.Latitude, arObject.Longitude, filteredLatitude, filteredLongitude);
                        if (Math.Abs(distance) <= PositionTolerance * arObject.Poi.visibilityRange)
                        {
                            return true;
                        }
                    }
                }
                foreach (var arLayer in activeObjects.Select(x => x.Poi?.ArLayer).Distinct())
                {
                    if (arLayer != null && arLayer.visibilityRange > 0)
                    {
                        var distance = CalculateDistance(arLayer.Latitude, arLayer.Longitude, filteredLatitude, filteredLongitude);
                        if (Math.Abs(distance) <= PositionTolerance * arLayer.visibilityRange)
                        {
                            return true;
                        }
                    }
                }
            }
            var arvosObjects = TriggerObjects.Values.Union(SlamObjects);
            foreach (var poi in arvosObjects.Select(x => x.poi).Distinct())
            {
                if (poi != null && poi.visibilityRange > 0)
                {
                    var distance = CalculateDistance(poi.Latitude, poi.Longitude, filteredLatitude, filteredLongitude);
                    if (Math.Abs(distance) <= PositionTolerance * poi.visibilityRange)
                    {
                        return true;
                    }
                }
            }
            foreach (var arLayer in arvosObjects.Select(x => x.poi?.ArLayer).Distinct())
            {
                if (arLayer != null && arLayer.visibilityRange > 0)
                {
                    var distance = CalculateDistance(arLayer.Latitude, arLayer.Longitude, filteredLatitude, filteredLongitude);
                    if (Math.Abs(distance) <= PositionTolerance * arLayer.visibilityRange)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void DuplicateArObjects(ArObjectState arObjectState)
        {
            var toBeDuplicated = arObjectState.ArObjectsToBeDuplicated();
            if (toBeDuplicated != null)
            {
                foreach (var arObject in toBeDuplicated)
                {
                    var poi = arObject.Poi.Clone();
                    if (IsSlamUrl(poi.TriggerImageURL))
                    {
                        poi.poiObject.triggerImageURL = string.Empty;

                        var relativeLocation = poi.poiObject.RelativeLocation;
                        relativeLocation[0] += 0.001f * ((_random.Next(2001) - 1000) / 100f);
                        relativeLocation[2] += 0.001f * ((_random.Next(2001) - 1000) / 100f);
                        poi.poiObject.RelativeLocation = relativeLocation;
                        CreateArObject(arObjectState, arObject, arObject.GameObjects.First().transform, poi, _arObjectId--);
                    }
                    else if (!string.IsNullOrWhiteSpace(poi.TriggerImageURL))
                    {
                        poi.poiObject.triggerImageURL = string.Empty;

                        var relativeLocation = poi.poiObject.RelativeLocation;
                        relativeLocation[0] += 0.001f * ((_random.Next(2001) - 1000) / 100f);
                        relativeLocation[2] += 0.001f * ((_random.Next(2001) - 1000) / 100f);
                        poi.poiObject.RelativeLocation = relativeLocation;
                        CreateArObject(arObjectState, arObject, arObject.GameObjects.First().transform, poi, _arObjectId--);
                    }
                    else if (!string.IsNullOrWhiteSpace(poi?.poiObject?.relativeLocation))
                    {
                        var relativeLocation = poi.poiObject.RelativeLocation;
                        relativeLocation[0] += (_random.Next(2001) - 1000) / 100f;
                        relativeLocation[2] += (_random.Next(2001) - 1000) / 100f;
                        poi.poiObject.RelativeLocation = relativeLocation;
                        CreateArObject(arObjectState, null, SceneAnchor.transform, poi, _arObjectId--);
                    }
                    else
                    {
                        poi.lat += _random.Next(201) - 100;
                        poi.lon += _random.Next(201) - 100;
                        CreateArObject(arObjectState, null, SceneAnchor.transform, poi, _arObjectId--);
                    }
                }
            }
        }

        private void PlaceArObjects(ArObjectState arObjectState)
        {
            var arObjectsToPlace = arObjectState.ArObjectsToPlace;
            if (arObjectsToPlace != null)
            {
                foreach (var arObject in arObjectsToPlace)
                {
                    var jump = false;

                    // Linearly interpolate from current position to target position
                    Vector3 position;
                    if (AreaSize > 0 && AreaWidth > 0
                        && (Math.Abs(arObject.WrapperObject.transform.position.x - arObject.TargetPosition.x) > AreaWidth * .75
                        || Math.Abs(arObject.WrapperObject.transform.position.z - arObject.TargetPosition.z) > AreaSize * .75))
                    {
                        // Jump if area handling is active and distance is too big
                        position = new Vector3(arObject.TargetPosition.x, arObject.TargetPosition.y, arObject.TargetPosition.z);
                        jump = true;
                    }
                    else
                    {
                        position = Vector3.Lerp(arObject.WrapperObject.transform.position, arObject.TargetPosition, .5f / FramesPerSecond);
                    }
                    arObject.WrapperObject.transform.position = position;

                    if (AreaSize > 0)
                    {
                        // Scale the objects at the edge of the area
                        var scale = arObject.Scale;
                        if (scale < 0)
                        {
                            scale = 1;
                        }
                        Vector3 localScale;
                        if (jump)
                        {
                            if (scale < 1)
                            {
                                scale = 0.01f;
                            }
                            localScale = new Vector3(scale, scale, scale);
                        }
                        else
                        {
                            localScale = new Vector3(scale, scale, scale);
                            localScale = Vector3.Lerp(arObject.WrapperObject.transform.localScale, localScale, 1f / FramesPerSecond);
                        }
                        arObject.WrapperObject.transform.localScale = localScale;
                    }
                }
            }
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
