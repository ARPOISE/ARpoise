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

    public class ArBehaviourData : ArBehaviourArObject
    {
#if HAS_AR_CORE
        private readonly string _clientApplicationName = ArvosApplicationName;
#else
#if HAS_AR_KIT
        private readonly string _clientApplicationName = ArvosApplicationName;
#else
        private readonly string _clientApplicationName = ArpoiseApplicationName;
#endif
#endif
        public const string ArvosApplicationName = "Arvos";
        public const string ArpoiseApplicationName = "Arpoise";

        #region Globals

        public GameObject SceneAnchor = null;
        public bool IsSlam { get; private set; }

        #endregion

        #region Protecteds

        protected List<ArItem> LayerItemList = null;
        protected bool IsNewLayer = false;

        protected bool? MenuEnabled = null;
        protected volatile RefreshRequest RefreshRequest = null;

        protected MenuButtonSetActiveActivity MenuButtonSetActive;
        protected HeaderSetActiveActivity HeaderSetActive;
        protected MenuButtonClickActivity MenuButtonClick;

        protected static readonly string ArpoiseDirectoryLayer = "Arpoise-Directory";
        protected static readonly string ArpoiseDirectoryUrl = "http://www.arpoise.com/cgi-bin/ArpoiseDirectory.cgi";

        #endregion

        #region GetData
        // A coroutine retrieving the objects
        protected override IEnumerator GetData()
        {
            var os = "Android";
            var bundle = "200529";
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

            while (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                MenuEnabled = null;
                count++;

                float filteredLatitude = FilteredLatitude;
                float filteredLongitude = FilteredLongitude;
                float usedLatitude = UsedLatitude;
                float usedLongitude = UsedLongitude;
                var layers = new List<ArLayer>();
                var nextPageKey = string.Empty;

                IsSlam = false;
                SlamObjects.Clear();

                #region Download all pages of the layer
                LayerWebUrl = null;
                for (; ; )
                {
                    var url = uri + "?lang=en&version=1&radius=1500&accuracy=100"
                        + "&lat=" + usedLatitude.ToString("F6")
                        + "&lon=" + usedLongitude.ToString("F6")
                        + (filteredLatitude != usedLatitude ? "&latOfDevice=" + filteredLatitude.ToString("F6") : string.Empty)
                        + (filteredLongitude != usedLongitude ? "&lonOfDevice=" + filteredLongitude.ToString("F6") : string.Empty)
                        + "&layerName=" + layerName
                        + (!string.IsNullOrWhiteSpace(nextPageKey) ? "&pageKey=" + nextPageKey : string.Empty)
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
                    if (string.IsNullOrWhiteSpace(text))
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
                        if (!string.IsNullOrWhiteSpace(layer.redirectionUrl))
                        {
                            uri = layer.redirectionUrl.Trim();
                        }
                        if (!string.IsNullOrWhiteSpace(layer.redirectionLayer))
                        {
                            layerName = layer.redirectionLayer.Trim();
                        }
                        if (!string.IsNullOrWhiteSpace(layer.redirectionUrl) || !string.IsNullOrWhiteSpace(layer.redirectionLayer))
                        {
                            layers.Clear();
                            nextPageKey = string.Empty;
                            continue;
                        }
                        layers.Add(layer);
                        if (layer.morePages == false || string.IsNullOrWhiteSpace(layer.nextPageKey))
                        {
                            LayerWebUrl = uri + "?layerName=" + layerName;
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
                    if (AssetBundles.ContainsKey(url))
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
                    AssetBundles[url] = assetBundle;
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
                                if (!string.IsNullOrWhiteSpace(spriteName))
                                {
                                    AssetBundle iconAssetBundle = null;
                                    if (AssetBundles.TryGetValue(iconAssetBundleUrl, out iconAssetBundle))
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
                    foreach (var hotspot in layer.hotspots.Where(x => !string.IsNullOrWhiteSpace(x.InnerLayerName)))
                    {
                        innerLayers[hotspot.InnerLayerName] = layer.isDefaultLayer;
                    }
                }

                foreach (var innerLayer in innerLayers.Keys)
                {
                    if (InnerLayers.ContainsKey(innerLayer))
                    {
                        continue;
                    }

                    if (layerName.Equals(innerLayer))
                    {
                        InnerLayers[layerName] = layers;
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
                        + (!string.IsNullOrWhiteSpace(nextPageKey) ? "&pageKey=" + nextPageKey : string.Empty)
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
                        if (string.IsNullOrWhiteSpace(text))
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
                            if (InnerLayers.TryGetValue(innerLayer, out layersList))
                            {
                                layersList.Add(layer);
                            }
                            else
                            {
                                InnerLayers[innerLayer] = new List<ArLayer> { layer };
                            }

                            if (layer.morePages == false || string.IsNullOrWhiteSpace(layer.nextPageKey))
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
                    assetBundleUrls.UnionWith(layer.hotspots.Where(x => !string.IsNullOrWhiteSpace(x.BaseUrl)).Select(x => x.BaseUrl));
                }

                foreach (var layerList in InnerLayers.Values)
                {
                    foreach (var layer in layerList.Where(x => x.hotspots != null))
                    {
                        assetBundleUrls.UnionWith(layer.hotspots.Where(x => !string.IsNullOrWhiteSpace(x.BaseUrl)).Select(x => x.BaseUrl));
                    }
                }

                var webRequests = new List<Tuple<string, string, UnityWebRequest>>();

                foreach (var url in assetBundleUrls)
                {
                    if (AssetBundles.ContainsKey(url))
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
                    AssetBundles[url] = assetBundle;
                }
                #endregion

                #region Download the trigger images
                var triggerImageUrls = new HashSet<string>();

                foreach (var layer in layers.Where(x => x.hotspots != null))
                {
                    triggerImageUrls.UnionWith(layer.hotspots.Where(x => !string.IsNullOrWhiteSpace(x.TriggerImageURL)).Select(x => x.TriggerImageURL));
                }

                foreach (var layerList in InnerLayers.Values)
                {
                    foreach (var layer in layerList.Where(x => x.hotspots != null))
                    {
                        triggerImageUrls.UnionWith(layer.hotspots.Where(x => !string.IsNullOrWhiteSpace(x.TriggerImageURL)).Select(x => x.TriggerImageURL));
                    }
                }

                webRequests = new List<Tuple<string, string, UnityWebRequest>>();

                if (triggerImageUrls.Any(x => IsSlamUrl(x)))
                {
                    IsSlam = true;
                    triggerImageUrls.Clear();
                }
                foreach (var url in triggerImageUrls)
                {
                    if (TriggerImages.ContainsKey(url))
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
                    TriggerImages[url] = texture;
                }
                #endregion

                #region Activate the Header
                var layerTitle = layers.Select(x => x.layerTitle).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
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

                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        yield break;
                    }
                    if (!arObjectState.ArObjects.Any() && !ArvosApplicationName.Equals(_clientApplicationName))
                    {
                        var message = layers.Select(x => x.noPoisMessage).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                        if (string.IsNullOrWhiteSpace(message))
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
                        IsSlam = false;
                        SlamObjects.Clear();
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
            if (url.StartsWith("http://"))
            {
                url = url.Substring(7);
            }
            if (!url.StartsWith("https://"))
            {
                url = "https://" + url;
            }
            return url;
        }
        #endregion
    }
}
