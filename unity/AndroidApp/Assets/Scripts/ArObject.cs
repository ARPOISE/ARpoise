/*
ArObject.cs - ArObject for Arpoise.

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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArObject
    {
        public bool IsDirty = true;
        public readonly long Id = 0;
        public readonly string GameObjectName;
        public readonly string BaseUrl;
        public readonly GameObject WrapperObject;
        public readonly List<GameObject> GameObjects;
        public readonly List<ArObject> ArObjects;
        public readonly string Text;
        public float Latitude;
        public float Longitude;
        public readonly float RelativeAltitude;
        public Vector3 TargetPosition;
        public readonly bool IsRelative;
        public float Scale;

        private List<KeyValuePair<Renderer, Color>> _rendererColorPairs = null;

        public ArObject(
            long id, string text, string gameObjectName, string baseUrl, GameObject wrapperObject, GameObject gameObject,
            float latitude, float longitude, float relativeAltitude, bool isRelative
            )
        {
            Id = id;
            Text = text;
            GameObjectName = gameObjectName;
            BaseUrl = baseUrl;
            WrapperObject = wrapperObject;
            GameObjects = gameObject != null ? new List<GameObject>(new[] { gameObject }) : new List<GameObject>();
            ArObjects = new List<ArObject>();
            Latitude = latitude;
            Longitude = longitude;
            RelativeAltitude = relativeAltitude;
            IsRelative = isRelative;
            Scale = 1;
        }

        public void SetBleachingValue(int bleachingValue)
        {
            if (_rendererColorPairs == null)
            {
                _rendererColorPairs = new List<KeyValuePair<Renderer, Color>>();
                foreach (var gameObject in GameObjects)
                {
                    Renderer objectRenderer = gameObject.GetComponent<MeshRenderer>();
                    if (objectRenderer != null)
                    {
                        _rendererColorPairs.Add(new KeyValuePair<Renderer, Color>(objectRenderer, objectRenderer.material.color));
                    }
                    else
                    {
                        foreach (GameObject child in gameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject))
                        {
                            if (child != null)
                            {
                                objectRenderer = child.GetComponent<MeshRenderer>();
                                if (objectRenderer != null)
                                {
                                    _rendererColorPairs.Add(new KeyValuePair<Renderer, Color>(objectRenderer, objectRenderer.material.color));
                                }
                            }
                        }
                    }
                }
            }

            if (_rendererColorPairs != null && _rendererColorPairs.Any())
            {
                foreach (var pair in _rendererColorPairs)
                {
                    var color = pair.Value;
                    pair.Key.material.color = new Color(
                        color.r + bleachingValue * ((1f - color.r) / 100f),
                        color.g + bleachingValue * ((1f - color.g) / 100f),
                        color.b + bleachingValue * ((1f - color.b) / 100f),
                        color.a
                        );
                }
            }
        }
    }
}
