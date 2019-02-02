/*
ArLayer.cs - Data description for Arpoise.

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
using UnityEngine;

namespace com.arpoise.androidapp
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
    }

    [Serializable]
    public class PoiAnimations
    {
        public PoiAnimation[] onCreate = null;
        public PoiAnimation[] onUpdate = null;
        public PoiAnimation[] onDelete = null;
        public PoiAnimation[] onFocus = null;
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
        public PoiAction[] actions = new PoiAction[0];
        public PoiAnimations animations = null;
        public string attribution = string.Empty;
        public float distance = 0;
        public float relativeAlt = 0;
        public string imageURL = string.Empty;
        public int lat = 0;
        public int lon = 0;

        public string line2 = string.Empty;
        public string line3 = string.Empty;
        public string line4 = string.Empty;
        public string title = string.Empty;
        public int type = 0;

        public float Latitude { get { return lat / 1000000f; } }
        public float Longitude { get { return lon / 1000000f; } }

        public ArLayer ArLayer;

        public string BaseUrl
        {
            get
            {
                string baseUrl = poiObject != null ? poiObject.baseURL : null;
                if (baseUrl != null)
                {
                    baseUrl = baseUrl.Trim();
                }
                return baseUrl;
            }
        }

        public string GameObjectName
        {
            get
            {
                string name = poiObject != null ? poiObject.full : null;
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
                string name = poiObject != null ? poiObject.poiLayerName : null;
                if (name != null)
                {
                    name = name.Trim();
                }
                return name;
            }
        }
    }

    // This class defines the Json message returned by ugpoise on the client side, allowing to parse the message
    [Serializable]
    public class ArLayer
    {
        public Poi[] hotspots = new Poi[0];
        public float radius = 0;
        public float refreshInterval = 0;
        public float refreshDistance = 0;
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

        public PoiAction[] actions = new PoiAction[0];

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
