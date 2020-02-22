/*
ArBehaviourData.cs - MonoBehaviour for Arpoise, data handling.

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

#if HAS_AR_CORE
using GoogleARCore;
#else
#endif

namespace com.arpoise.arpoiseapp
{
    public interface IActivity
    {
        void Execute();
    }

    public class ArpoiseCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public class RefreshRequest
    {
        public string url;
        public string layerName;
        public float? latitude;
        public float? longitude;
    }

    public class TriggerObject
    {
        public bool isActive;
        public int index;
        public string triggerImageURL;
        public Texture2D texture;
        public float width;
        public GameObject gameObject;
        public Poi poi;
    }

    public class HeaderSetActiveActivity : IActivity
    {
        public ArBehaviourData ArBehaviour;
        public string LayerTitle;

        public void Execute()
        {
            ArBehaviour.SetHeaderActive(LayerTitle);
        }
    }

    public class MenuButtonClickActivity : IActivity
    {
        public ArBehaviourData ArBehaviour;

        public void Execute()
        {
            ArBehaviour.HandleMenuButtonClick();
        }
    }

    public class MenuButtonSetActiveActivity : IActivity
    {
        public ArBehaviourData ArBehaviour;
        public List<ArLayer> Layers;

        public void Execute()
        {
            ArBehaviour.SetMenuButtonActive(Layers);
        }
    }

    public class ArBehaviourData : ArBehaviourPosition
    {
        public const string ArvosApplicationName = "Arvos";
        public const string ArpoiseApplicationName = "Arpoise";
        public const string ArslamApplicationName = "Arslam";

        #region Globals

        public GameObject SceneAnchor = null;
        public GameObject Wrapper = null;

        #endregion

        #region Protecteds

        protected List<ArItem> LayerItemList = null;
        protected bool IsNewLayer = false;
        protected bool HasTriggerImages = false;
        protected Dictionary<int, TriggerObject> TriggerObjects = new Dictionary<int, TriggerObject>();
#if HAS_AR_CORE
        protected AugmentedImageDatabase AugmentedImageDatabase;
#endif
        protected bool? MenuEnabled = null;
        protected string InformationMessage = null;
        protected bool ShowInfo = false;
        protected volatile RefreshRequest RefreshRequest = null;
        protected float RefreshInterval = 0;

        protected MenuButtonSetActiveActivity MenuButtonSetActive;
        protected HeaderSetActiveActivity HeaderSetActive;
        protected MenuButtonClickActivity MenuButtonClick;

        protected static readonly string ArpoiseDirectoryLayer = "Arpoise-Directory";
        protected static readonly string ArpoiseDirectoryUrl = "http://www.arpoise.com/cgi-bin/ArpoiseDirectory.cgi";
        #endregion

        #region ArObjects

#if HAS_AR_CORE
        private readonly string _clientApplicationName = ArvosApplicationName;
#else
#if HAS_AR_KIT
#if IS_SLAM_APP
        private readonly string _clientApplicationName = ArslamApplicationName;
#else
        private readonly string _clientApplicationName = ArvosApplicationName;
#endif
#else
        private readonly string _clientApplicationName = ArpoiseApplicationName;
#endif
#endif
        private readonly Dictionary<string, List<ArLayer>> _innerLayers = new Dictionary<string, List<ArLayer>>();
        private readonly Dictionary<string, AssetBundle> _assetBundles = new Dictionary<string, AssetBundle>();
        private readonly Dictionary<string, Texture2D> _triggerImages = new Dictionary<string, Texture2D>();
        private int _bleachingValue = -1;

        // Link ar object to ar object state or to parent object
        private string LinkArObject(ArObjectState arObjectState, ArObject parentObject, Transform parentTransform, ArObject arObject, GameObject arGameObject, Poi poi)
        {
            if (parentObject == null)
            {
                // Add to ar object state
                arObjectState.Add(arObject);

                List<ArLayer> innerLayers = null;
                if (!IsEmpty(poi.InnerLayerName) && _innerLayers.TryGetValue(poi.InnerLayerName, out innerLayers))
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
                // Add to parent object
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
            long poiId,
            out GameObject createdObject
            )
        {
            createdObject = null;
            var objectName = objectToAdd.name;

            // Create a copy of the object
            objectToAdd = Instantiate(objectToAdd);
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
                return "Instantiate(Wrapper) failed";
            }
            wrapper.name = "TransformWrapper";
            createdObject = wrapper;
            wrapper.transform.parent = parentTransform;
            parentTransform = wrapper.transform;

            // Add a wrapper for scaling
            var scaleWrapper = Instantiate(Wrapper);
            if (scaleWrapper == null)
            {
                return "Instantiate(Wrapper) failed";
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
                    return "Instantiate(Wrapper) failed";
                }
                billboardWrapper.name = "BillboardWrapper";
                billboardWrapper.transform.parent = parentTransform;
                parentTransform = billboardWrapper.transform;
                arObjectState.AddBillboardAnimation(new ArAnimation(poiId, billboardWrapper, objectToAdd, null, true));
            }

            // Prepare the rotation of the object
            GameObject rotationWrapper = null;
            if (poi.transform != null && poi.transform.angle != 0)
            {
                rotationWrapper = Instantiate(Wrapper);
                if (rotationWrapper == null)
                {
                    return "Instantiate(Wrapper) failed";
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
                            return "Instantiate(Wrapper) failed";
                        }
                        animationWrapper.name = "OnCreateWrapper";
                        arObjectState.AddOnCreateAnimation(new ArAnimation(poiId, animationWrapper, objectToAdd, poiAnimation, true));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }

                if (poi.animations.onFocus != null)
                {
                    foreach (var poiAnimation in poi.animations.onFocus)
                    {
                        // Put the animation into a wrapper
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(Wrapper) failed";
                        }
                        animationWrapper.name = "OnFocusWrapper";
                        arObjectState.AddOnFocusAnimation(new ArAnimation(poiId, animationWrapper, objectToAdd, poiAnimation, false));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }

                if (poi.animations.inFocus != null)
                {
                    foreach (var poiAnimation in poi.animations.inFocus)
                    {
                        // Put the animation into a wrapper
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(Wrapper) failed";
                        }
                        animationWrapper.name = "InFocusWrapper";
                        arObjectState.AddInFocusAnimation(new ArAnimation(poiId, animationWrapper, objectToAdd, poiAnimation, false));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }

                if (poi.animations.onClick != null)
                {
                    foreach (var poiAnimation in poi.animations.onClick)
                    {
                        // Put the animation into a wrapper
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(Wrapper) failed";
                        }
                        animationWrapper.name = "OnClickWrapper";
                        arObjectState.AddOnClickAnimation(new ArAnimation(poiId, animationWrapper, objectToAdd, poiAnimation, false));
                        animationWrapper.transform.parent = parentTransform;
                        parentTransform = animationWrapper.transform;
                    }
                }

                if (poi.animations.onFollow != null)
                {
                    foreach (var poiAnimation in poi.animations.onFollow)
                    {
                        // Put the animation into a wrapper
                        var animationWrapper = Instantiate(Wrapper);
                        if (animationWrapper == null)
                        {
                            return "Instantiate(Wrapper) failed";
                        }
                        animationWrapper.name = "OnFollowWrapper";
                        arObjectState.AddOnFollowAnimation(new ArAnimation(poiId, animationWrapper, objectToAdd, poiAnimation, false));
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
                return "Could not set scale " + ((poi.transform == null) ? "null" : "" + poi.transform.scale);
            }

            // Rotate the rotationWrapper
            if (rotationWrapper != null)
            {
                rotationWrapper.transform.localEulerAngles = new Vector3(0, poi.transform.angle, 0);
            }

            // Relative to user, parent or with absolute coordinates
            var relativePosition = poi.poiObject.relativeLocation;

            if (parentObject != null || !IsEmpty(relativePosition))
            {
                // Relative to user or parent
                if (IsEmpty(relativePosition))
                {
                    relativePosition = "0,0,0";
                }
                var parts = relativePosition.Split(',');

                double value;
                var xOffset = (float)(parts.Length > 0 && double.TryParse(parts[0].Trim(), out value) ? value : 0);
                var yOffset = (float)(parts.Length > 1 && double.TryParse(parts[1].Trim(), out value) ? value : 0);
                var zOffset = (float)(parts.Length > 2 && double.TryParse(parts[2].Trim(), out value) ? value : 0);

                var arObject = new ArObject(
                    poiId, poi.title, objectToAdd.name, poi.BaseUrl, wrapper, objectToAdd, poi.Latitude, poi.Longitude, poi.relativeAlt + yOffset, true);

                var result = LinkArObject(arObjectState, parentObject, parentTransform, arObject, objectToAdd, poi);
                if (result != null)
                {
                    return result;
                }

                arObject.WrapperObject.transform.position = arObject.TargetPosition = new Vector3(xOffset, arObject.RelativeAltitude, zOffset);

                if (_bleachingValue >= 0)
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
                        poiId, poi.title, objectToAdd.name, poi.BaseUrl, wrapper, objectToAdd, poi.Latitude, poi.Longitude, poi.relativeAlt, false);

                    var result = LinkArObject(arObjectState, parentObject, parentTransform, arObject, objectToAdd, poi);
                    if (result != null)
                    {
                        return result;
                    }

                    if (_bleachingValue >= 0)
                    {
                        arObject.SetBleachingValue(_bleachingValue);
                    }
                }
            }

            return null;
        }

        // Create ar objects for the pois and link them
        protected string CreateArObjects(ArObjectState arObjectState, ArObject parentObject, Transform parentObjectTransform, IEnumerable<Poi> pois)
        {
            foreach (var poi in pois.Where(x => x.isVisible && !IsEmpty(x.GameObjectName)))
            {
                long poiId = poi.id;
                if (parentObject != null)
                {
                    poiId = -1000000 * parentObject.Id - poiId;
                }

                string assetBundleUrl = poi.BaseUrl;
                if (IsEmpty(assetBundleUrl))
                {
                    return "Poi with id " + poiId + ", empty asset bundle url'";
                }

                AssetBundle assetBundle = null;
                if (!_assetBundles.TryGetValue(assetBundleUrl, out assetBundle))
                {
                    return "?: '" + assetBundleUrl + "'";
                }

                string objectName = poi.GameObjectName;
                if (IsEmpty(objectName))
                {
                    continue;
                }

                var objectToAdd = assetBundle.LoadAsset<GameObject>(objectName);
                if (objectToAdd == null)
                {
                    return "Poi with id " + poiId + ", unknown game object: '" + objectName + "'";
                }

                var triggerImageURL = poi.TriggerImageURL;
                if (!IsEmpty(triggerImageURL))
                {
                    try
                    {
                        Texture2D texture = null;
                        if (!_triggerImages.TryGetValue(triggerImageURL, out texture) || texture == null)
                        {
                            return "?t " + triggerImageURL;
                        }

                        var t = TriggerObjects.Values.FirstOrDefault(x => x.triggerImageURL == triggerImageURL);
                        if (t == null)
                        {
                            int newIndex = TriggerObjects.Count;
#if HAS_AR_CORE
                            newIndex = AugmentedImageDatabase.Count;
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
                                poi = poi
                            };
                            TriggerObjects[t.index] = t;
#if HAS_AR_CORE
                            AugmentedImageDatabase.AddImage(triggerImageURL, texture, width);
#endif
                        }
                        else
                        {
                            t.isActive = true;
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
                        poiId,
                        out newObject
                        );
                    if (!IsEmpty(result))
                    {
                        return result;
                    }
                }
            }
            HasTriggerImages = TriggerObjects.Values.Any(x => x.isActive);

            return null;
        }

        // Create ar objects from layers
        private ArObjectState CreateArObjectState(List<ArObject> existingArObjects, List<ArLayer> layers)
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
                        informationMessage = layer.actions.Select(x => x.activityMessage).FirstOrDefault(x => !IsEmpty(x));
                    }
                }

                if (layer.hotspots == null)
                {
                    continue;
                }
                var layerPois = layer.hotspots.Where(x => x.isVisible && !IsEmpty(x.GameObjectName) && (x.ArLayer = layer) == layer);
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
                                               && (IsEmpty(x.BaseUrl) || arObject.BaseUrl.Equals(x.BaseUrl))
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
                    if (IsEmpty(objectName))
                    {
                        continue;
                    }

                    string baseUrl = poi.BaseUrl;
                    if (!IsEmpty(baseUrl))
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

        #region GetData
        // A coroutine retrieving the objects
        protected override IEnumerator GetData()
        {
            var os = "Android";
            var bundle = "200222";
#if UNITY_IOS
            os = "iOS";
            bundle = "20" + bundle;
#endif
            long count = 0;
            string layerName = ArpoiseDirectoryLayer;
            string uri = ArpoiseDirectoryUrl;

            bool setError = true;

            while (OriginalLatitude == 0.0 && OriginalLongitude == 0.0)
            {
                // wait for the position to be determined
                yield return new WaitForSeconds(.01f);
            }

            while (InfoPanelIsActive())
            {
                yield return new WaitForSeconds(.01f);
            }

            while (IsEmpty(ErrorMessage))
            {
                MenuEnabled = null;
                count++;

                float filteredLatitude = FilteredLatitude;
                float filteredLongitude = FilteredLongitude;
                float usedLatitude = UsedLatitude;
                float usedLongitude = UsedLongitude;
                var layers = new List<ArLayer>();
                var nextPageKey = string.Empty;

                #region Download all pages of the layer
                for (; ; )
                {
                    var url = uri + "?lang=en&version=1&radius=1500&accuracy=100"
                        + "&lat=" + usedLatitude.ToString("F6")
                        + "&lon=" + usedLongitude.ToString("F6")
                        + (filteredLatitude != usedLatitude ? "&latOfDevice=" + filteredLatitude.ToString("F6") : string.Empty)
                        + (filteredLongitude != usedLongitude ? "&lonOfDevice=" + filteredLongitude.ToString("F6") : string.Empty)
                        + "&layerName=" + layerName
                        + (!IsEmpty(nextPageKey) ? "&pageKey=" + nextPageKey : string.Empty)
                        + "&userId=" + SystemInfo.deviceUniqueIdentifier
                        + "&client=" + _clientApplicationName
                        + "&bundle=" + bundle
                        + "&os=" + os
                        + "&count=" + count
                    ;

                    url = FixUrl(url);
                    var request = UnityWebRequest.Get(url);
                    request.certificateHandler = new ArpoiseCertificateHandler();
                    request.timeout = 30;
                    yield return request.SendWebRequest();

                    var maxWait = request.timeout * 100;
                    while (!(request.isNetworkError || request.isHttpError) && !request.isDone && maxWait > 0)
                    {
                        yield return new WaitForSeconds(.01f);
                        maxWait--;
                    }

                    if (maxWait < 1)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Layer contents didn't download in 30 seconds.";
                            yield break;
                        }
                        break;
                    }

                    if (request.isNetworkError || request.isHttpError)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Layer contents download error: " + request.error;
                            yield break;
                        }
                        break;
                    }

                    var text = request.downloadHandler.text;
                    if (IsEmpty(text))
                    {
                        if (setError)
                        {
                            ErrorMessage = "Layer contents download received empty text.";
                            yield break;
                        }
                        break;
                    }
                    try
                    {
                        var layer = ArLayer.Create(text);
                        if (!IsEmpty(layer.redirectionUrl))
                        {
                            uri = layer.redirectionUrl.Trim();
                        }
                        if (!IsEmpty(layer.redirectionLayer))
                        {
                            layerName = layer.redirectionLayer.Trim();
                        }
                        if (!IsEmpty(layer.redirectionUrl) || !IsEmpty(layer.redirectionLayer))
                        {
                            layers.Clear();
                            nextPageKey = string.Empty;
                            continue;
                        }
                        layers.Add(layer);
                        if (layer.morePages == false || IsEmpty(layer.nextPageKey))
                        {
                            break;
                        }
                        nextPageKey = layer.nextPageKey;
                    }
                    catch (Exception e)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Layer parse exception: " + e.Message;
                            yield break;
                        }
                        break;
                    }
                }
                #endregion

                #region Handle the showMenuButton of the layers
                MenuButtonSetActive = new MenuButtonSetActiveActivity { ArBehaviour = this, Layers = layers.ToList() };
                #endregion

                #region Download the asset bundle for icons
                var assetBundleUrls = new HashSet<string>();
                var iconAssetBundleUrl = "www.arpoise.com/AB/arpoiseicons.ace";
                assetBundleUrls.Add(iconAssetBundleUrl);
                foreach (var url in assetBundleUrls)
                {
                    if (_assetBundles.ContainsKey(url))
                    {
                        continue;
                    }
                    var assetBundleUri = FixUrl(GetAssetBundleUrl(url));
                    var request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUri, 0);
                    request.certificateHandler = new ArpoiseCertificateHandler();
                    request.timeout = 60;
                    yield return request.SendWebRequest();

                    var maxWait = request.timeout * 100;
                    while (!(request.isNetworkError || request.isHttpError) && !request.isDone && maxWait > 0)
                    {
                        yield return new WaitForSeconds(.01f);
                        maxWait--;
                    }

                    if (maxWait < 1)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Bundle '" + assetBundleUri + "' download timeout.";
                            yield break;
                        }
                        continue;
                    }

                    if (request.isNetworkError || request.isHttpError)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Bundle '" + assetBundleUri + "' error: " + request.error;
                            yield break;
                        }
                        continue;
                    }

                    var assetBundle = DownloadHandlerAssetBundle.GetContent(request);
                    if (assetBundle == null)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Bundle '" + assetBundleUri + "' download is null.";
                            yield break;
                        }
                        continue;
                    }
                    _assetBundles[url] = assetBundle;
                }
                #endregion

                #region Handle lists of possible layers to show
                {
                    var itemList = new List<ArItem>();
                    foreach (var layer in layers.Where(x => x.hotspots != null))
                    {
                        if ("Arpoise-Directory".Equals(layer.layer) || "AR-vos-Directory".Equals(layer.layer))
                        {
                            foreach (var poi in layer.hotspots)
                            {
                                GameObject spriteObject = null;
                                var spriteName = poi.line4;
                                if (!IsEmpty(spriteName))
                                {
                                    AssetBundle iconAssetBundle = null;
                                    if (_assetBundles.TryGetValue(iconAssetBundleUrl, out iconAssetBundle))
                                    {
                                        spriteObject = iconAssetBundle.LoadAsset<GameObject>(spriteName);
                                    }
                                }
                                var sprite = spriteObject?.GetComponent<SpriteRenderer>().sprite;

                                itemList.Add(new ArItem
                                {
                                    layerName = poi.title,
                                    itemName = poi.line1,
                                    line2 = poi.line2,
                                    line3 = poi.line3,
                                    url = poi.BaseUrl,
                                    distance = (int)poi.distance,
                                    icon = sprite
                                });
                            }
                        }
                    }

                    if (itemList.Any())
                    {
                        LayerItemList = itemList;

                        // There are different layers to show
                        MenuButtonClick = new MenuButtonClickActivity { ArBehaviour = this };

                        // Wait for the user to select a layer
                        for (; ; )
                        {
                            var refreshRequest = RefreshRequest;
                            RefreshRequest = null;
                            if (refreshRequest != null)
                            {
                                count = 0;
                                layerName = refreshRequest.layerName;
                                uri = refreshRequest.url;
                                FixedDeviceLatitude = refreshRequest.latitude;
                                FixedDeviceLongitude = refreshRequest.longitude;
                                break;
                            }
                            yield return new WaitForSeconds(.1f);
                        }
                        continue;
                    }
                }
                #endregion

                #region Download all inner layers
                var innerLayers = new Dictionary<string, bool>();
                foreach (var layer in layers.Where(x => x.hotspots != null))
                {
                    foreach (var hotspot in layer.hotspots.Where(x => !IsEmpty(x.InnerLayerName)))
                    {
                        innerLayers[hotspot.InnerLayerName] = layer.isDefaultLayer;
                    }
                }

                foreach (var innerLayer in innerLayers.Keys)
                {
                    if (_innerLayers.ContainsKey(innerLayer))
                    {
                        continue;
                    }

                    if (layerName.Equals(innerLayer))
                    {
                        _innerLayers[layerName] = layers;
                        continue;
                    }

                    var isDefaultLayer = innerLayers[innerLayer];
                    var latitude = isDefaultLayer ? 0f : usedLatitude;
                    var longitude = isDefaultLayer ? 0f : usedLongitude;
                    nextPageKey = string.Empty;
                    for (; ; )
                    {
                        var url = uri + "?lang=en&version=1&radius=1500&accuracy=100&innerLayer=true"
                        + "&lat=" + latitude.ToString("F6")
                        + "&lon=" + longitude.ToString("F6")
                        + ((filteredLatitude != latitude) ? "&latOfDevice=" + filteredLatitude.ToString("F6") : string.Empty)
                        + ((filteredLongitude != longitude) ? "&lonOfDevice=" + filteredLongitude.ToString("F6") : string.Empty)
                        + "&layerName=" + innerLayer
                        + (!IsEmpty(nextPageKey) ? "&pageKey=" + nextPageKey : string.Empty)
                        + "&userId=" + SystemInfo.deviceUniqueIdentifier
                        + "&client=" + _clientApplicationName
                        + "&bundle=" + bundle
                        + "&os=" + os
                        ;

                        url = FixUrl(url);
                        var request = UnityWebRequest.Get(url);
                        request.certificateHandler = new ArpoiseCertificateHandler();
                        request.timeout = 30;
                        yield return request.SendWebRequest();

                        var maxWait = request.timeout * 100;
                        while (!(request.isNetworkError || request.isHttpError) && !request.isDone && maxWait > 0)
                        {
                            yield return new WaitForSeconds(.01f);
                            maxWait--;
                        }

                        if (maxWait < 1)
                        {
                            if (setError)
                            {
                                ErrorMessage = "Layer " + innerLayer + " contents didn't download in 30 seconds.";
                                yield break;
                            }
                            break;
                        }

                        if (request.isNetworkError || request.isHttpError)
                        {
                            if (setError)
                            {
                                ErrorMessage = "Layer " + innerLayer + " contents download error: " + request.error;
                                yield break;
                            }
                            break;
                        }

                        var text = request.downloadHandler.text;
                        if (IsEmpty(text))
                        {
                            if (setError)
                            {
                                ErrorMessage = "Layer " + innerLayer + " contents download received empty text.";
                                yield break;
                            }
                            break;
                        }

                        try
                        {
                            var layer = ArLayer.Create(text);

                            List<ArLayer> layersList = null;
                            if (_innerLayers.TryGetValue(innerLayer, out layersList))
                            {
                                layersList.Add(layer);
                            }
                            else
                            {
                                _innerLayers[innerLayer] = new List<ArLayer> { layer };
                            }

                            if (layer.morePages == false || IsEmpty(layer.nextPageKey))
                            {
                                break;
                            }
                            nextPageKey = layer.nextPageKey;
                        }
                        catch (Exception e)
                        {
                            if (setError)
                            {
                                ErrorMessage = "Layer " + innerLayer + " parse exception: " + e.Message;
                                yield break;
                            }
                            break;
                        }
                    }
                }
                #endregion

                #region Download all asset bundles
                foreach (var layer in layers.Where(x => x.hotspots != null))
                {
                    assetBundleUrls.UnionWith(layer.hotspots.Where(x => !IsEmpty(x.BaseUrl)).Select(x => x.BaseUrl));
                }

                foreach (var layerList in _innerLayers.Values)
                {
                    foreach (var layer in layerList.Where(x => x.hotspots != null))
                    {
                        assetBundleUrls.UnionWith(layer.hotspots.Where(x => !IsEmpty(x.BaseUrl)).Select(x => x.BaseUrl));
                    }
                }

                var webRequests = new List<Tuple<string, string, UnityWebRequest>>();

                foreach (var url in assetBundleUrls)
                {
                    if (_assetBundles.ContainsKey(url))
                    {
                        continue;
                    }
                    var assetBundleUri = FixUrl(GetAssetBundleUrl(url));
                    var request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUri, 0);
                    request.certificateHandler = new ArpoiseCertificateHandler();
                    request.timeout = 60;
                    webRequests.Add(new Tuple<string, string, UnityWebRequest>(url, assetBundleUri, request));
                    yield return request.SendWebRequest();
                }

                foreach (var tuple in webRequests)
                {
                    var url = tuple.Item1;
                    var assetBundleUri = tuple.Item2;
                    var request = tuple.Item3;

                    var maxWait = request.timeout * 100;
                    while (!(request.isNetworkError || request.isHttpError) && !request.isDone && maxWait > 0)
                    {
                        yield return new WaitForSeconds(.01f);
                        maxWait--;
                    }

                    if (maxWait < 1)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Bundle '" + assetBundleUri + "' download timeout.";
                            yield break;
                        }
                        continue;
                    }

                    if (request.isNetworkError || request.isHttpError)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Bundle '" + assetBundleUri + "' error: " + request.error;
                            yield break;
                        }
                        continue;
                    }

                    var assetBundle = DownloadHandlerAssetBundle.GetContent(request);
                    if (assetBundle == null)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Bundle '" + assetBundleUri + "' download is null.";
                            yield break;
                        }
                        continue;
                    }
                    _assetBundles[url] = assetBundle;
                }
                #endregion

                #region Download the trigger images
                var triggerImageUrls = new HashSet<string>();

                foreach (var layer in layers.Where(x => x.hotspots != null))
                {
                    triggerImageUrls.UnionWith(layer.hotspots.Where(x => !IsEmpty(x.TriggerImageURL)).Select(x => x.TriggerImageURL));
                }

                foreach (var layerList in _innerLayers.Values)
                {
                    foreach (var layer in layerList.Where(x => x.hotspots != null))
                    {
                        triggerImageUrls.UnionWith(layer.hotspots.Where(x => !IsEmpty(x.TriggerImageURL)).Select(x => x.TriggerImageURL));
                    }
                }

                webRequests = new List<Tuple<string, string, UnityWebRequest>>();

                foreach (var url in triggerImageUrls)
                {
                    if (_triggerImages.ContainsKey(url))
                    {
                        continue;
                    }
                    var triggerImageUri = FixUrl(url);
                    var request = UnityWebRequestTexture.GetTexture(triggerImageUri);
                    request.certificateHandler = new ArpoiseCertificateHandler();
                    request.timeout = 30;
                    webRequests.Add(new Tuple<string, string, UnityWebRequest>(url, triggerImageUri, request));
                    yield return request.SendWebRequest();
                }

                foreach (var tuple in webRequests)
                {
                    var url = tuple.Item1;
                    var triggerImageUri = tuple.Item2;
                    var request = tuple.Item3;

                    var maxWait = request.timeout * 100;
                    while (!(request.isNetworkError || request.isHttpError) && !request.isDone && maxWait > 0)
                    {
                        yield return new WaitForSeconds(.01f);
                        maxWait--;
                    }

                    if (maxWait < 1)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Image " + triggerImageUri + " contents didn't download in 30 seconds.";
                            yield break;
                        }
                        continue;
                    }

                    if (request.isNetworkError || request.isHttpError)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Image " + triggerImageUri + " contents download error: " + request.error;
                            yield break;
                        }
                        continue;
                    }

                    var texture = DownloadHandlerTexture.GetContent(request);
                    if (texture == null)
                    {
                        if (setError)
                        {
                            ErrorMessage = "Image " + triggerImageUri + " contents download received empty texture.";
                            yield break;
                        }
                        continue;
                    }
                    _triggerImages[url] = texture;
                }
                #endregion

                #region Activate the Header
                var layerTitle = layers.Select(x => x.layerTitle).FirstOrDefault(x => !IsEmpty(x));
                HeaderSetActive = new HeaderSetActiveActivity { LayerTitle = layerTitle, ArBehaviour = this };
                #endregion

                List<ArObject> existingArObjects = null;
                var arObjectState = ArObjectState;
                if (arObjectState != null)
                {
                    existingArObjects = arObjectState.ArObjects.ToList();
                }
                arObjectState = CreateArObjectState(existingArObjects, layers);
                setError = false;

                if (ArObjectState == null)
                {
                    ErrorMessage = CreateArObjects(arObjectState, null, SceneAnchor.transform, arObjectState.ArPois);
                    arObjectState.ArPois.Clear();

                    if (!IsEmpty(ErrorMessage))
                    {
                        yield break;
                    }
                    if (!arObjectState.ArObjects.Any() && !ArvosApplicationName.Equals(_clientApplicationName))
                    {
                        var message = layers.Select(x => x.noPoisMessage).FirstOrDefault(x => !IsEmpty(x));
                        if (IsEmpty(message))
                        {
                            message = "Sorry, there are no augments at your location!";
                        }
                        ErrorMessage = message;
                        yield break;
                    }
                    arObjectState.SetArObjectsToPlace();

                    StartTicks = DateTime.Now.Ticks;
                    ArObjectState = arObjectState;
                }
                else
                {
                    if (arObjectState.ArPois.Any())
                    {
                        ArObjectState.ArPois.AddRange(arObjectState.ArPois);
                    }
                    if (arObjectState.ArObjectsToDelete.Any())
                    {
                        ArObjectState.ArObjectsToDelete.AddRange(arObjectState.ArObjectsToDelete);
                    }
                    ArObjectState.IsDirty = true;
                }
                IsNewLayer = true;

                var refreshInterval = RefreshInterval;
                var doNotRefresh = refreshInterval < 1;

                long nowTicks = DateTime.Now.Ticks;
                long waitUntil = nowTicks + (long)refreshInterval * 10000000L;

                while (doNotRefresh || nowTicks < waitUntil)
                {
                    nowTicks = DateTime.Now.Ticks;

                    var refreshRequest = RefreshRequest;
                    RefreshRequest = null;
                    if (refreshRequest != null)
                    {
                        count = 0;
                        layerName = refreshRequest.layerName;
                        uri = refreshRequest.url;
                        FixedDeviceLatitude = refreshRequest.latitude;
                        FixedDeviceLongitude = refreshRequest.longitude;
                        ErrorMessage = string.Empty;
                        foreach (var triggerObject in TriggerObjects.Values)
                        {
                            triggerObject.isActive = false;
                        }
                        HasTriggerImages = false;
                        break;
                    }
                    yield return new WaitForSeconds(.1f);
                }
            }
            yield break;
        }
        #endregion

        #region Misc
        public virtual void SetMenuButtonActive(List<ArLayer> layers)
        {
        }

        public virtual void HandleInfoPanelClosed()
        {
        }

        public virtual void HandleMenuButtonClick()
        {
        }

        public virtual void SetHeaderActive(string layerTitle)
        {
        }

#if UNITY_IOS
        public readonly int DeviceAngle = 360;
#endif
#if UNITY_ANDROID
        public int DeviceAngle
        {
            get
            {
                switch (InitialDeviceOrientation)
                {
                    case DeviceOrientation.LandscapeRight:
                        return 180;

                    case DeviceOrientation.PortraitUpsideDown:
                        return 270;

                    case DeviceOrientation.Portrait:
                        return 90;

                    default:
                        return 360;
                }
            }
        }
#endif
        private string GetAssetBundleUrl(string url)
        {
#if UNITY_IOS
            if (url.EndsWith(".ace"))
            {
                url = url.Replace(".ace", "i.ace");
            }
            else
            {
                url += "i";
            }
#endif
            return url;
        }

        private string FixUrl(string url)
        {
            while (url.Contains('\\'))
            {
                url = url.Replace("\\", string.Empty);
            }
#if HAS_AR_CORE
            if (url.StartsWith("http://"))
            {
                url = url.Substring(7);
            }
            if (!url.StartsWith("https://"))
            {
                url = "https://" + url;
            }
#endif
            return url;
        }
        #endregion
    }
}
