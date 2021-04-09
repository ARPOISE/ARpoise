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
    public enum ArInterpolation
    {
        Linear = 0,
        Cyclic = 1,
        Sine = 2,
        Halfsine = 3
    }

    public enum ArAnimationType
    {
        Transform = 0,
        Rotate = 1,
        Scale = 2,
        Destroy = 3,
        Duplicate = 4,
        Fade = 5,
        Grow = 6
    }

    public class ArAnimation
    {
        public readonly long PoiId;
        public readonly GameObject Wrapper;
        public readonly GameObject GameObject;
        public readonly string Name;
        public readonly string FollowedBy;

        private readonly ArCreature _creature;
        private readonly Transform _transform;
        private readonly long _lengthTicks;
        private readonly long _delayTicks;
        private readonly ArAnimationType _type;
        private readonly ArInterpolation _interpolation;
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
            GameObject = gameObject;
            IsActive = isActive;
            if ((Wrapper = wrapper) != null)
            {
                _transform = wrapper.transform;
            }
            if (poiAnimation != null)
            {
                Name = poiAnimation.name;
                _lengthTicks = (long)(10000000.0 * poiAnimation.length);
                _delayTicks = (long)(10000000.0 * poiAnimation.delay);
                if (poiAnimation.type != null)
                {
                    var type = poiAnimation.type.ToLower();
                    _type = type.Contains(nameof(ArAnimationType.Rotate).ToLower()) ? ArAnimationType.Rotate
                        : type.Contains(nameof(ArAnimationType.Scale).ToLower()) ? ArAnimationType.Scale
                        : type.Contains(nameof(ArAnimationType.Grow).ToLower()) ? ArAnimationType.Grow
                        : type.Contains(nameof(ArAnimationType.Destroy).ToLower()) ? ArAnimationType.Destroy
                        : type.Contains(nameof(ArAnimationType.Duplicate).ToLower()) ? ArAnimationType.Duplicate
                        : type.Contains(nameof(ArAnimationType.Fade).ToLower()) ? ArAnimationType.Fade
                        : ArAnimationType.Transform;
                }
                if (poiAnimation.interpolation != null)
                {
                    var interpolation = poiAnimation.interpolation.ToLower();
                    _interpolation = interpolation.Contains(nameof(ArInterpolation.Cyclic).ToLower()) ? ArInterpolation.Cyclic
                        : interpolation.Contains(nameof(ArInterpolation.Halfsine).ToLower()) ? ArInterpolation.Halfsine
                        : interpolation.Contains(nameof(ArInterpolation.Sine).ToLower()) ? ArInterpolation.Sine
                        : ArInterpolation.Linear;
                }
                _persisting = poiAnimation.persist;
                _repeating = poiAnimation.repeat;
                _from = poiAnimation.from;
                _to = poiAnimation.to;
                _axis = poiAnimation.axis == null ? Vector3.zero
                    : new Vector3(poiAnimation.axis.x, poiAnimation.axis.y, poiAnimation.axis.z);
                if (_type == ArAnimationType.Grow && GameObject != null)
                {
                    _creature = GameObject.GetComponent(typeof(ArCreature)) as ArCreature;
                }
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
        // private float? _initialR = null;
        // private float? _initialG = null;
        // private float? _initialB = null;

        private static readonly string _openUrl = "openUrl:";
        public bool HandleOpenUrl(string s)
        {
            if (!string.IsNullOrWhiteSpace(s) && s.Length > _openUrl.Length)
            {
                if (_openUrl.Equals(s.Substring(0, _openUrl.Length), StringComparison.InvariantCultureIgnoreCase))
                {
                    var url = s.Substring(_openUrl.Length);
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
                var lengthTicks = _lengthTicks;
                var endTicks = _startTicks + lengthTicks;
                if (endTicks < nowTicks)
                {
                    if (!_repeating)
                    {
                        Stop(worldStartTicks, nowTicks, false);
                        return;
                    }

                    _startTicks = endTicks + lengthTicks < nowTicks ? nowTicks : _startTicks + lengthTicks;
                    JustActivated = true;
                }
                animationValue = (nowTicks - _startTicks) / ((float)lengthTicks);
            }

            var from = _from;
            var to = _to;

            switch (_interpolation)
            {
                case ArInterpolation.Cyclic:
                    if (animationValue >= .5f)
                    {
                        animationValue -= .5f;
                        var temp = from;
                        from = to;
                        to = temp;
                    }
                    animationValue *= 2;
                    break;

                case ArInterpolation.Halfsine:
                    animationValue = (float)Math.Sin(Math.PI * animationValue);
                    break;

                case ArInterpolation.Sine:
                    animationValue = (-1f + (float)Math.Cos(2 * Math.PI * animationValue)) / 2;
                    break;
            }

            if (animationValue < 0)
            {
                animationValue = -animationValue;
            }
            var animationFactor = from + (to - from) * animationValue;

            Transform transform;
            switch (_type)
            {
                case ArAnimationType.Rotate:
                    transform = _transform;
                    if (transform != null)
                    {
                        var axis = _axis;
                        transform.localEulerAngles = new Vector3(
                            axis.x * animationFactor,
                            axis.y * animationFactor,
                            axis.z * animationFactor
                            );
                    }
                    break;

                case ArAnimationType.Scale:
                    transform = _transform;
                    if (transform != null)
                    {
                        var axis = _axis;
                        transform.localScale = new Vector3(
                            axis.x == 0 ? 1 : axis.x * animationFactor,
                            axis.y == 0 ? 1 : axis.y * animationFactor,
                            axis.z == 0 ? 1 : axis.z * animationFactor
                            );
                    }
                    break;

                case ArAnimationType.Transform:
                    transform = _transform;
                    if (transform != null)
                    {
                        var axis = _axis;
                        transform.localPosition = new Vector3(
                            axis.x * animationFactor,
                            axis.y * animationFactor,
                            axis.z * animationFactor
                            );
                    }
                    break;

                case ArAnimationType.Grow:
                    var creature = _creature;
                    if (creature != null)
                    {
                        creature.Grow(animationFactor);
                    }
                    break;

                case ArAnimationType.Fade:
                    SetFadeValue(animationFactor);
                    break;

                case ArAnimationType.Destroy:
                    IsToBeDestroyed = true;
                    break;

                case ArAnimationType.Duplicate:
                    if (JustActivated)
                    {
                        IsToBeDuplicated = true;
                    }
                    break;
            }
            if (JustActivated)
            {
                HandleOpenUrl(Name);

                if (GameObject != null)
                {
                    var audioSource = GameObject.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.Play();
                    }
                }
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
            if (!_persisting)
            {
                Transform transform;
                switch (_type)
                {
                    case ArAnimationType.Rotate:
                        transform = _transform;
                        if (transform != null)
                        {
                            transform.localEulerAngles = Vector3.zero;
                        }
                        break;

                    case ArAnimationType.Scale:
                        transform = _transform;
                        if (transform != null)
                        {
                            transform.localScale = Vector3.one;
                        }
                        break;

                    case ArAnimationType.Transform:
                        transform = _transform;
                        if (transform != null)
                        {
                            transform.localPosition = Vector3.zero;
                        }
                        break;

                    case ArAnimationType.Fade:
                        if (_initialA.HasValue)
                        {
                            SetFadeValue(_initialA.Value);
                        }
                        break;
                }
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
            if (objectRenderer != null && objectRenderer.material != null)
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
                        if (objectRenderer != null && objectRenderer.material != null)
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
        //    if (objectRenderer != null && objectRenderer.material != null)
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
        //                if (objectRenderer != null && objectRenderer.material != null)
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