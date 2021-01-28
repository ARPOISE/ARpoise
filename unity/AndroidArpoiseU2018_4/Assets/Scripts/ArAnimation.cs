/*
ArAnimation.cs - Handling porpoise level animations for Arpoise.

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

namespace com.arpoise.arpoiseapp
{
    public class ArAnimation
    {
        public const string Rotate = "rotate";
        public const string Scale = "scale";
        public const string Transform = "transform";
        public const string Destroy = "destroy";
        public const string Duplicate = "duplicate";
        public const string Fade = "fade";
        //public const string Bleach = "bleach";

        public const string Linear = "linear";
        public const string Cyclic = "cyclic";
        public const string Sine = "sine";
        public const string Halfsine = "halfsine";

        public readonly long PoiId;
        public readonly GameObject Wrapper;
        public readonly GameObject GameObject;
        public readonly string Name;
        public readonly string FollowedBy;

        private readonly long _lengthTicks;
        private readonly long _delayTicks;
        private readonly string _type = Transform;
        private readonly string _interpolation = Linear;
        private readonly string _interpolationType = string.Empty;
        private readonly bool _persisting;
        private readonly bool _repeating;
        private readonly float _from;
        private readonly float _to;
        private readonly Vector3 _axis;

        private long _startTicks = 0;

        public bool IsToBeDestroyed { get; private set; }
        public bool IsToBeDuplicated { get; set; }

        public ArAnimation(long poiId, GameObject wrapper, GameObject gameObject, PoiAnimation poiAnimation, bool isActive)
        {
            PoiId = poiId;
            Wrapper = wrapper;
            GameObject = gameObject;
            IsActive = isActive;
            if (poiAnimation != null)
            {
                Name = poiAnimation.name;
                _lengthTicks = (long)(10000000.0 * poiAnimation.length);
                _delayTicks = (long)(10000000.0 * poiAnimation.delay);
                if (poiAnimation.type != null)
                {
                    var type = poiAnimation.type.ToLower();
                    _type = type.Contains(Rotate) ? Rotate
                        : type.Contains(Scale) ? Scale
                        : type.Contains(Destroy) ? Destroy
                        : type.Contains(Duplicate) ? Duplicate
                        : type.Contains(Fade) ? Fade
                        //: type.Contains(Bleach) ? Bleach
                        : Transform;
                }
                if (poiAnimation.interpolation != null)
                {
                    var interpolation = poiAnimation.interpolation.ToLower();
                    _interpolation = interpolation.Contains(Cyclic) ? Cyclic : Linear;
                    _interpolationType = interpolation.Contains(Halfsine) ? Halfsine
                        : interpolation.Contains(Sine) ? Sine
                        : string.Empty;
                }
                _persisting = poiAnimation.persist;
                _repeating = poiAnimation.repeat;
                _from = poiAnimation.from;
                _to = poiAnimation.to;
                _axis = poiAnimation.axis == null ? Vector3.zero
                    : new Vector3(poiAnimation.axis.x, poiAnimation.axis.y, poiAnimation.axis.z);
                FollowedBy = poiAnimation.followedBy;
            }
        }

        public bool IsActive { get; private set; }
        public bool JustActivated { get; private set; }
        public bool JustStopped { get; private set; }

        public void Activate(long worldStartTicks, long nowTicks)
        {
            IsActive = true;
            _startTicks = 0;
            Animate(worldStartTicks, nowTicks);
        }

        private float? _initialA = null;
        private float? _initialR = null;
        private float? _initialG = null;
        private float? _initialB = null;

        public bool HandleOpenUrl(string s)
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                var openUrl = "openUrl:";
                if (openUrl.Equals(s.Substring(0, openUrl.Length), StringComparison.InvariantCultureIgnoreCase))
                {
                    var url = s.Substring(openUrl.Length);
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        Application.OpenURL(url);
                        return true;
                    }
                }
            }
            return false;
        }

        public void Animate(long worldStartTicks, long nowTicks)
        {
            JustActivated = false;
            JustStopped = false;

            if (worldStartTicks <= 0 || !IsActive || _lengthTicks < 1 || _delayTicks < 0)
            {
                return;
            }

            if (_delayTicks > 0 && worldStartTicks + _delayTicks > nowTicks)
            {
                return;
            }

            float animationValue = 0;

            if (_startTicks == 0)
            {
                _startTicks = nowTicks;
                JustActivated = true;
            }
            else
            {
                var endTicks = _startTicks + _lengthTicks;
                if (endTicks < nowTicks)
                {
                    if (!_repeating)
                    {
                        Stop(worldStartTicks, nowTicks, false);
                        return;
                    }

                    if (endTicks + _lengthTicks < nowTicks)
                    {
                        _startTicks = nowTicks;
                    }
                    else
                    {
                        _startTicks += _lengthTicks;
                    }
                    JustActivated = true;
                }
                animationValue = (nowTicks - _startTicks) / ((float)_lengthTicks);
            }

            var from = _from;
            var to = _to;

            if (Cyclic.Equals(_interpolation))
            {
                if (animationValue >= .5)
                {
                    animationValue -= .5f;
                    var temp = from;
                    from = to;
                    to = temp;
                }
                animationValue *= 2;
            }

            if (Halfsine.Equals(_interpolationType))
            {
                animationValue = (float)Math.Sin(Math.PI * animationValue);
            }
            else if (Sine.Equals(_interpolationType))
            {
                animationValue = (-1f + (float)Math.Cos(2 * Math.PI * animationValue)) / 2;
            }

            if (animationValue < 0)
            {
                animationValue = -animationValue;
            }
            var animationFactor = from + (to - from) * animationValue;

            if (Rotate.Equals(_type) && Wrapper != null && Wrapper.transform != null)
            {
                Wrapper.transform.localEulerAngles = new Vector3(
                    _axis.x * animationFactor,
                    _axis.y * animationFactor,
                    _axis.z * animationFactor
                    );
            }
            else if (Scale.Equals(_type) && Wrapper != null && Wrapper.transform != null)
            {
                Wrapper.transform.localScale = new Vector3(
                    _axis.x == 0 ? 1 : _axis.x * animationFactor,
                    _axis.y == 0 ? 1 : _axis.y * animationFactor,
                    _axis.z == 0 ? 1 : _axis.z * animationFactor
                    );
            }
            else if (Transform.Equals(_type) && Wrapper != null && Wrapper.transform != null)
            {
                Wrapper.transform.localPosition = new Vector3(
                    _axis.x * animationFactor,
                    _axis.y * animationFactor,
                    _axis.z * animationFactor
                    );
            }
            else if (Fade.Equals(_type))
            {
                SetFadeValue(animationFactor);
            }
            //else if (Bleach.Equals(_type))
            //{
            //    SetBleachingValue(animationFactor);
            //}
            else if (Destroy.Equals(_type))
            {
                IsToBeDestroyed = true;
            }
            else if (JustActivated && Duplicate.Equals(_type))
            {
                IsToBeDuplicated = true;
            }
            if (JustActivated)
            {
                HandleOpenUrl(Name);
            }
        }

        public void Stop(long worldStartTicks, long nowTicks, bool animate = true)
        {
            if (animate)
            {
                Animate(worldStartTicks, nowTicks);
            }
            JustStopped = true;
            IsActive = false;

            if (!_persisting && Wrapper != null && Wrapper.transform != null)
            {
                if (Rotate.Equals(_type))
                {
                    Wrapper.transform.localEulerAngles = Vector3.zero;
                }
                else if (Scale.Equals(_type))
                {
                    Wrapper.transform.localScale = Vector3.one;
                }
                else if (Transform.Equals(_type))
                {
                    Wrapper.transform.localPosition = Vector3.zero;
                }
                else if (Fade.Equals(_type))
                {
                    if (_initialA.HasValue)
                    {
                        SetFadeValue(_initialA.Value);
                    }
                }
                //else if (Bleach.Equals(_type))
                //{
                //    if (_initialR.HasValue)
                //    {
                //        SetBleachingValue(0);
                //    }
                //}
            }
        }

        private void SetFadeValue(float value)
        {
            var gameObject = GameObject;
            if (gameObject == null)
            {
                return;
            }

            //particle system also have renderers that could be accessed
            //var ps = gameObject.GetComponent<ParticleSystem>();
            //var x = ps.shape.meshRenderer.material.color;
            //var y = ps.shape.spriteRenderer.material.color;
            //var z = ps.shape.skinnedMeshRenderer.material.color;

            var rendererColorPairs = new List<KeyValuePair<Renderer, Color>>();
            Renderer objectRenderer = gameObject.GetComponent<MeshRenderer>();
            if (objectRenderer != null && objectRenderer.material != null && objectRenderer.material.color != null)
            {
                rendererColorPairs.Add(new KeyValuePair<Renderer, Color>(objectRenderer, objectRenderer.material.color));
            }
            else
            {
                foreach (var child in gameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject))
                {
                    if (child != null)
                    {
                        objectRenderer = child.GetComponent<MeshRenderer>();
                        if (objectRenderer != null && objectRenderer.material != null && objectRenderer.material.color != null)
                        {
                            rendererColorPairs.Add(new KeyValuePair<Renderer, Color>(objectRenderer, objectRenderer.material.color));
                        }
                    }
                }
            }
            if (rendererColorPairs.Any())
            {
                foreach (var pair in rendererColorPairs)
                {
                    var color = pair.Value;
                    if (_initialA == null)
                    {
                        _initialA = color.a;
                    }
                    pair.Key.material.color = new Color(color.r, color.g, color.b, value);
                }
            }
        }

        // This does not really work, it was work in progress from October 2020, a version of Nothing of him
        //
        //private void SetBleachingValue(float value)
        //{
        //    var gameObject = GameObject;
        //    if (gameObject == null)
        //    {
        //        return;
        //    }

        //    Renderer objectRenderer;
        //    var rendererColorPairs = new List<KeyValuePair<Renderer, Color>>();
        //    objectRenderer = gameObject.GetComponent<MeshRenderer>();
        //    if (objectRenderer != null && objectRenderer.material != null && objectRenderer.material.color != null)
        //    {
        //        rendererColorPairs.Add(new KeyValuePair<Renderer, Color>(objectRenderer, objectRenderer.material.color));
        //    }
        //    else
        //    {
        //        foreach (var child in gameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject))
        //        {
        //            if (child != null)
        //            {
        //                objectRenderer = child.GetComponent<MeshRenderer>();
        //                if (objectRenderer != null && objectRenderer.material != null && objectRenderer.material.color != null)
        //                {
        //                    rendererColorPairs.Add(new KeyValuePair<Renderer, Color>(objectRenderer, objectRenderer.material.color));
        //                }
        //            }
        //        }
        //    }
        //    if (rendererColorPairs.Any())
        //    {
        //        foreach (var pair in rendererColorPairs)
        //        {
        //            var color = pair.Value;
        //            if (_initialR == null)
        //            {
        //                _initialR = color.r;
        //            }
        //            if (_initialG == null)
        //            {
        //                _initialG = color.g;
        //            }
        //            if (_initialB == null)
        //            {
        //                _initialB = color.b;
        //            }
        //            pair.Key.material.color = new Color(
        //                _initialR.Value + value * ((1f - _initialR.Value) / 100f),
        //                _initialG.Value + value * ((1f - _initialG.Value) / 100f),
        //                _initialB.Value + value * ((1f - _initialB.Value) / 100f),
        //                color.a
        //                );
        //        }
        //    }
        //}
    }
}