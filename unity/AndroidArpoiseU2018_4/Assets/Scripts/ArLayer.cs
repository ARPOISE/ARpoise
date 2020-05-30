/*
ArLayer.cs - Data description for an Arpoise layer.

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
using System.Globalization;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    [Serializable]
    public class PoiVector3
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
    }

    [Serializable]
    public class PoiTransform
    {
        public bool rel = false;
        public float angle = 0;
        public float scale = 0;
    }

    [Serializable]
    public class PoiAction
    {
        public float autoTriggerRange = 0;
        public bool autoTriggerOnly = false;
        public string uri = string.Empty;
        public string label = string.Empty;
        public string contentType = string.Empty;
        public int activityType = 0;
        public string method = "GET";
        public string[] poiParams = null;
        public bool closeBiw = false;
        public bool showActivity = true;
        public string activityMessage = string.Empty;
    }

    [Serializable]
    public class PoiAnimation
    {
        public string name = string.Empty;
        public string type = string.Empty;
        public float length = 0;
        public float delay = 0;
        public string interpolation = string.Empty;
        public float interpolationParam = 0;
        public bool persist = false;
        public bool repeat = false;
        public float from = 0;
        public float to = 0;
        public PoiVector3 axis = null;
        public string followedBy = string.Empty;
    }

    [Serializable]
    public class PoiAnimations
    {
        public PoiAnimation[] onCreate = null;
        public PoiAnimation[] onFollow = null;
        public PoiAnimation[] onFocus = null;
        public PoiAnimation[] inFocus = null;
        public PoiAnimation[] onClick = null;
    }

    [Serializable]
    public class PoiObject
    {
        public string baseURL = string.Empty;
        public string full = string.Empty;
        public string poiLayerName = string.Empty;
        public string relativeLocation = string.Empty;
        public string icon = string.Empty;
        public float size = 0;
        public string triggerImageURL = string.Empty;
        public float triggerImageWidth = 0;

        public float[] RelativeLocation
        {
            get
            {
                var relativePosition = relativeLocation;
                if (string.IsNullOrWhiteSpace(relativePosition))
                {
                    relativePosition = "0,0,0";
                }
                var parts = relativePosition.Split(',');

                double value;
                var xOffset = (float)(parts.Length > 0 && double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
                var yOffset = (float)(parts.Length > 1 && double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
                var zOffset = (float)(parts.Length > 2 && double.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
                return new float[] { xOffset, yOffset, zOffset };
            }
            set
            {
                if (value != null)
                {
                    relativeLocation = $"{(value.Length > 0 ? value[0].ToString(CultureInfo.InvariantCulture) : "0")},{(value.Length > 1 ? value[1].ToString(CultureInfo.InvariantCulture) : "0")},{(value.Length > 2 ? value[2].ToString(CultureInfo.InvariantCulture) : "0")}";
                }
                else
                {
                    relativeLocation = string.Empty;
                }
            }
        }
    }

    [Serializable]
    public class Poi
    {
        public long id = 0;
        public int dimension = 0;
        public bool showSmallBiw = true;
        public bool isVisible = true;
        public PoiTransform transform = null;
        public PoiObject poiObject = null;
        public PoiAction[] actions = Array.Empty<PoiAction>();
        public PoiAnimations animations = null;
        public string attribution = string.Empty;
        public float distance = 0;
        public float relativeAlt = 0;
        public string imageURL = string.Empty;
        public int lat = 0;
        public int lon = 0;

        public string line1 = string.Empty;
        public string line2 = string.Empty;
        public string line3 = string.Empty;
        public string line4 = string.Empty;
        public string title = string.Empty;
        public int type = 0;

        public float Latitude { get { return lat / 1000000f; } }
        public float Longitude { get { return lon / 1000000f; } }

        [NonSerialized]
        public ArLayer ArLayer;

        public string BaseUrl
        {
            get
            {
                string baseUrl = poiObject?.baseURL;
                if (baseUrl != null)
                {
                    baseUrl = baseUrl.Trim();
                }
                return baseUrl;
            }
        }

        public string TriggerImageURL
        {
            get
            {
                string triggerImageURL = poiObject?.triggerImageURL;
                if (triggerImageURL != null)
                {
                    triggerImageURL = triggerImageURL.Trim();
                }
                return triggerImageURL;
            }
        }

        public string GameObjectName
        {
            get
            {
                string name = poiObject?.full;
                if (name != null)
                {
                    name = name.Trim();
                }
                return name;
            }
        }

        public string InnerLayerName
        {
            get
            {
                string name = poiObject?.poiLayerName;
                if (name != null)
                {
                    name = name.Trim();
                }
                return name;
            }
        }

        public Poi Clone()
        {
            var s = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<Poi>(s);
        }
    }

    // This class defines the Json message returned by ugpoise on the client side, allowing to parse the message
    [Serializable]
    public class ArLayer
    {
        public Poi[] hotspots = Array.Empty<Poi>();
        public float radius = 0;
        public float refreshInterval = 0;
        public float refreshDistance = 0;
        public string redirectionUrl = string.Empty;
        public string redirectionLayer = string.Empty;
        public string noPoisMessage = string.Empty;
        public string layerTitle = string.Empty;
        public string showMessage = string.Empty;
        public bool morePages = false;
        public string nextPageKey = string.Empty;
        public string layer = string.Empty;
        public int errorCode = 0;
        public string errorString = string.Empty;
        public int bleachingValue = 0;
        public int areaSize = 0;
        public int areaWidth = 0;
        public int visibilityRange = 1500;
        public bool applyKalmanFilter = true;
        public bool isDefaultLayer = false;
        public bool showMenuButton = true;

        public PoiAction[] actions = Array.Empty<PoiAction>();

        public static ArLayer Create(string json)
        {
            // 'params' and 'object' are reserved words in C#, we have to replace them before we parse the json
            json = json.Replace("\"params\"", "\"poiParams\"").Replace("\"object\"", "\"poiObject\"");

            return JsonUtility.FromJson<ArLayer>(json);
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
