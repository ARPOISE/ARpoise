/*
ArAnimation.cs - Handling porpoise level animations for ARpoise.

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
        public readonly string Name = string.Empty;
        public readonly string[] FollowedBy = Array.Empty<string>();

        private readonly ArCreature _creature;
        private readonly Transform _transform;
        private readonly long _lengthTicks;
        private readonly long _delayTicks;
        private readonly ArAnimationType _animationType;
        private readonly ArInterpolation _interpolation;
        private readonly bool _persisting;
        private readonly bool _repeating;
        private readonly float _from;
        private readonly float _to;
        private readonly Vector3 _axis;

        private static readonly string _rotate = nameof(ArAnimationType.Rotate).ToLower();
        private static readonly string _scale = nameof(ArAnimationType.Scale).ToLower();
        private static readonly string _destroy = nameof(ArAnimationType.Destroy).ToLower();
        private static readonly string _duplicate = nameof(ArAnimationType.Duplicate).ToLower();
        private static readonly string _fade = nameof(ArAnimationType.Fade).ToLower();
        private static readonly string _grow = nameof(ArAnimationType.Grow).ToLower();
        private static readonly string _cyclic = nameof(ArInterpolation.Cyclic).ToLower();
        private static readonly string _halfsine = nameof(ArInterpolation.Halfsine).ToLower();
        private static readonly string _sine = nameof(ArInterpolation.Sine).ToLower();

        private float? _initialA = null;
        private long _startTicks = 0;
        private List<Material> _materialsToFade = null;

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
                Name = poiAnimation.name?.Trim();
                FollowedBy = !string.IsNullOrWhiteSpace(poiAnimation.followedBy)
                    ? poiAnimation.followedBy.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray()
                    : FollowedBy;
                _lengthTicks = (long)(10000000.0 * poiAnimation.length);
                _delayTicks = (long)(10000000.0 * poiAnimation.delay);
                if (poiAnimation.type != null)
                {
                    var type = poiAnimation.type.ToLower();
                    _animationType = type.Contains(_rotate) ? ArAnimationType.Rotate
                        : type.Contains(_scale) ? ArAnimationType.Scale
                        : type.Contains(_destroy) ? ArAnimationType.Destroy
                        : type.Contains(_duplicate) ? ArAnimationType.Duplicate
                        : type.Contains(_fade) ? ArAnimationType.Fade
                        : type.Contains(_grow) ? ArAnimationType.Grow
                        : ArAnimationType.Transform;
                }
                if (poiAnimation.interpolation != null)
                {
                    var interpolation = poiAnimation.interpolation.ToLower();
                    _interpolation = interpolation.Contains(_cyclic) ? ArInterpolation.Cyclic
                        : interpolation.Contains(_halfsine) ? ArInterpolation.Halfsine
                        : interpolation.Contains(_sine) ? ArInterpolation.Sine
                        : ArInterpolation.Linear;
                }
                _persisting = poiAnimation.persist;
                _repeating = poiAnimation.repeat;
                _from = poiAnimation.from;
                _to = poiAnimation.to;
                _axis = poiAnimation.axis == null ? Vector3.zero
                    : new Vector3(poiAnimation.axis.x, poiAnimation.axis.y, poiAnimation.axis.z);
                if (_animationType == ArAnimationType.Grow && GameObject != null)
                {
                    _creature = GameObject.GetComponent(typeof(ArCreature)) as ArCreature;
                }
            }
        }

        public bool IsActive { get; private set; }
        public bool JustActivated { get; private set; }
        public bool JustStopped { get; private set; }

        public void Activate(long startTicks, long nowTicks)
        {
            IsActive = true;
            _startTicks = 0;
            Animate(startTicks, nowTicks);
        }

        public void Animate(long startTicks, long nowTicks)
        {
            JustStopped = JustActivated = false;

            if (startTicks <= 0 || !IsActive || _lengthTicks < 1 || _delayTicks < 0)
            {
                return;
            }
            if (_delayTicks > 0 && startTicks + _delayTicks > nowTicks)
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
                        Stop(startTicks, endTicks, true);
                        return;
                    }
                    _startTicks = endTicks + lengthTicks >= nowTicks ? endTicks : nowTicks;
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

            switch (_animationType)
            {
                case ArAnimationType.Rotate:
                    Rotate(animationFactor);
                    break;

                case ArAnimationType.Scale:
                    Scale(animationFactor);
                    break;

                case ArAnimationType.Transform:
                    Transform(animationFactor);
                    break;

                case ArAnimationType.Grow:
                    Grow(animationFactor);
                    break;

                case ArAnimationType.Fade:
                    Fade(animationFactor);
                    break;

                case ArAnimationType.Destroy:
                    IsToBeDestroyed = true;
                    break;

                case ArAnimationType.Duplicate:
                    IsToBeDuplicated = JustActivated;
                    break;
            }

            if (JustActivated)
            {
                HandleOpenUrl(Name);
                HandleSetActive(Name, true);
                HandleAudioSource();
            }
        }

        public void Stop(long startTicks, long nowTicks, bool animate = true)
        {
            if (animate)
            {
                Animate(startTicks, nowTicks);
            }
            JustStopped = true;
            IsActive = false;
            if (!_persisting)
            {
                HandleSetActive(Name, false);
                switch (_animationType)
                {
                    case ArAnimationType.Rotate:
                        if (_transform != null)
                        {
                            _transform.localEulerAngles = Vector3.zero;
                        }
                        break;

                    case ArAnimationType.Scale:
                        if (_transform != null)
                        {
                            _transform.localScale = Vector3.one;
                        }
                        break;

                    case ArAnimationType.Transform:
                        if (_transform != null)
                        {
                            _transform.localPosition = Vector3.zero;
                        }
                        break;

                    case ArAnimationType.Fade:
                        if (_initialA.HasValue)
                        {
                            Fade(_initialA.Value);
                        }
                        break;
                }
            }
        }

        private static readonly string _openUrl = "openUrl:";
        public bool HandleOpenUrl(string s)
        {
            if (!string.IsNullOrWhiteSpace(s)
                && s.Length > _openUrl.Length
                && _openUrl.Equals(s.Substring(0, _openUrl.Length), StringComparison.InvariantCultureIgnoreCase))
            {
                var url = s.Substring(_openUrl.Length);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    Application.OpenURL(url);
                    return true;
                }
            }
            return false;
        }

        private static readonly string _setInActive = "SetInActive";
        public bool HandleSetActive(string s, bool setActive)
        {
            var gameObject = GameObject;
            if (gameObject != null)
            {
                if (nameof(gameObject.SetActive).Equals(s, StringComparison.InvariantCultureIgnoreCase))
                {
                    gameObject.SetActive(setActive);
                }
                else if (_setInActive.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                {
                    gameObject.SetActive(!setActive);
                }
            }
            return false;
        }

        private void HandleAudioSource()
        {
            if (GameObject != null)
            {
                var audioSource = GameObject.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Play();
                }
            }
        }

        private void Rotate(float value)
        {
            if (_transform != null)
            {
                _transform.localEulerAngles = new Vector3(_axis.x * value, _axis.y * value, _axis.z * value);
            }
        }

        private void Scale(float value)
        {
            if (_transform != null)
            {
                _transform.localScale = new Vector3(
                    _axis.x == 0 ? 1 : _axis.x * value, _axis.y == 0 ? 1 : _axis.y * value, _axis.z == 0 ? 1 : _axis.z * value);
            }
        }

        private void Transform(float value)
        {
            if (_transform != null)
            {
                _transform.localPosition = new Vector3(_axis.x * value, _axis.y * value, _axis.z * value);
            }
        }

        private void Grow(float value)
        {
            if (_creature != null)
            {
                _creature.Grow(value);
            }
        }

        private void Fade(float value)
        {
            if (_materialsToFade == null)
            {
                GetMaterialsToFade(GameObject, _materialsToFade = new List<Material>());
            }
            foreach (var material in _materialsToFade)
            {
                var color = material.color;
                if (!_initialA.HasValue)
                {
                    _initialA = color.a;
                }
                material.color = new Color(color.r, color.g, color.b, value);
            }
        }

        private void GetMaterialsToFade(GameObject gameObject, List<Material> materials)
        {
            if (gameObject != null)
            {
                //particle system also have renderers that could be accessed
                //var ps = gameObject.GetComponent<ParticleSystem>();
                //var x = ps.shape.meshRenderer.material.color;
                //var y = ps.shape.spriteRenderer.material.color;
                //var z = ps.shape.skinnedMeshRenderer.material.color;

                var renderer = gameObject.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.material != null)
                {
                    materials.Add(renderer.material);
                }
                foreach (var child in gameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject))
                {
                    if (child != null)
                    {
                        renderer = child.GetComponent<MeshRenderer>();
                        if (renderer != null && renderer.material != null)
                        {
                            materials.Add(renderer.material);
                        }
                    }
                    // Making this fully recursive is too slow for example in AyCorona
                    // GetMaterialsToFade(transform.gameObject, materials);
                }
            }
        }

        // This does not really work, it was work in progress from October 2020, a version of Nothing of Him
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