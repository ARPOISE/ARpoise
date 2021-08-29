/*
ArLayer.cs - Data description for an ARpoise layer.

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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        public int visibilityRange = 0;
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

        [NonSerialized]
        private int? _maximumCount = null;
        public int MaximumCount
        {
            get
            {
                if (!_maximumCount.HasValue)
                {
                    _maximumCount = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(MaximumCount).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        int value = 0;
                        if (int.TryParse(action.activityMessage, out value))
                        {
                            _maximumCount = value;
                        }
                    }
                }
                return _maximumCount.Value;
            }
        }

        [NonSerialized]
        private double? _trackingTimeout = null;
        public double TrackingTimeout
        {
            get
            {
                if (!_trackingTimeout.HasValue)
                {
                    _trackingTimeout = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(TrackingTimeout).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        double value = 0;
                        if (double.TryParse(action.activityMessage, out value))
                        {
                            _trackingTimeout = value;
                        }
                    }
                }
                return _trackingTimeout.Value;
            }
        }

        [NonSerialized]
        private string _lindenmayerString = null;
        public string LindenmayerString
        {
            get
            {
                if (_lindenmayerString == null)
                {
                    _lindenmayerString = string.Empty;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LindenmayerString).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        _lindenmayerString = action.activityMessage;
                        if (_lindenmayerString != null)
                        {
                            _lindenmayerString = Regex.Replace(_lindenmayerString, @"\s+", string.Empty);
                        }
                    }
                }
                return _lindenmayerString;
            }
        }

        [NonSerialized]
        private string _leafPrefab = null;
        public string LeafPrefab
        {
            get
            {
                if (_leafPrefab == null)
                {
                    _leafPrefab = string.Empty;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LeafPrefab).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        _leafPrefab = action.activityMessage;
                        if (_leafPrefab != null)
                        {
                            _leafPrefab = Regex.Replace(_leafPrefab, @"\s+", string.Empty);
                        }
                    }
                }
                return _leafPrefab;
            }
        }

        [NonSerialized]
        private float? _lindenmayerAngle = null;
        public float LindenmayerAngle
        {
            get
            {
                if (!_lindenmayerAngle.HasValue)
                {
                    _lindenmayerAngle = 22.5f;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LindenmayerAngle).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value = 0;
                        if (float.TryParse(action.activityMessage, out value))
                        {
                            _lindenmayerAngle = value;
                        }
                    }
                }
                return _lindenmayerAngle.Value;
            }
        }

        [NonSerialized]
        private float? _lindenmayerFactor = null;
        public float LindenmayerFactor
        {
            get
            {
                if (!_lindenmayerFactor.HasValue)
                {
                    _lindenmayerFactor = 0.8f;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LindenmayerFactor).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value = 0;
                        if (float.TryParse(action.activityMessage, out value))
                        {
                            _lindenmayerFactor = value;
                        }
                    }
                }
                return _lindenmayerFactor.Value;
            }
        }

        [NonSerialized]
        private int? _derivations = null;
        public int LindenmayerDerivations
        {
            get
            {
                if (!_derivations.HasValue)
                {
                    _derivations = 1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LindenmayerDerivations).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        int value = 0;
                        if (int.TryParse(action.activityMessage, out value))
                        {
                            _derivations = value;
                        }
                    }
                }
                return _derivations.Value;
            }
        }
    }

    // This class defines the Json message returned by porpoise to the client side, allowing to parse the message
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

        [NonSerialized]
        private float _latitude = float.MinValue;
        public float Latitude
        {
            get
            {
                if (_latitude == float.MinValue)
                {
                    if (hotspots.Any())
                    {
                        _latitude = hotspots.Select(x => x.Latitude).Average();
                    }
                    else
                    {
                        _latitude = 0;
                    }
                }
                return _latitude;
            }
        }

        [NonSerialized]
        private float _longitude = float.MinValue;
        public float Longitude
        {
            get
            {
                if (_longitude == float.MinValue)
                {
                    if (hotspots.Any())
                    {
                        _longitude = hotspots.Select(x => x.Longitude).Average();
                    }
                    else
                    {
                        _longitude = 0;
                    }
                }
                return _longitude;
            }
        }

        [NonSerialized]
        private HashSet<string> _actionLabels = new HashSet<string>(new string[] { nameof(PositionUpdateInterval) });
        [NonSerialized]
        private bool? _showInfo;
        public bool ShowInfo
        {
            get
            {
                if (!_showInfo.HasValue)
                {
                    _showInfo = (actions?.FirstOrDefault(x => !_actionLabels.Contains(x.label?.Trim()) && x.showActivity)) != null;
                }
                return _showInfo.Value;
            }
        }
        [NonSerialized]
        private string _informationMessage;
        public string InformationMessage
        {
            get
            {
                if (_informationMessage == null)
                {
                    _informationMessage = actions.Where(x => !_actionLabels.Contains(x.label?.Trim()) && x.showActivity).Select(x => x.activityMessage).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                    if (_informationMessage == null)
                    {
                        _informationMessage = string.Empty;
                    }
                }
                return _informationMessage;
            }
        }
        [NonSerialized]
        private float? _positionUpdateInterval;
        public float PositionUpdateInterval
        {
            get
            {
                if (_positionUpdateInterval == null)
                {
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(PositionUpdateInterval).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value = 0;
                        if (float.TryParse(action.activityMessage, out value))
                        {
                            _positionUpdateInterval = value;
                        }
                    }
                    if (_positionUpdateInterval == null)
                    {
                        _positionUpdateInterval = 0;
                    }
                }
                return _positionUpdateInterval.Value;
            }
        }
    }
}
