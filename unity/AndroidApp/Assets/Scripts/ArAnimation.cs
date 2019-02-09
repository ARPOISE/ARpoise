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
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArAnimation
    {
        public static string Rotate = "rotate";
        public static string Scale = "scale";
        public static string Transform = "transform";
        public static string Linear = "linear";
        public static string Cyclic = "cyclic";
        public static string Sine = "sine";
        public static string Halfsine = "halfsine";

        public readonly long PoiId;
        public readonly GameObject GameObject;

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

        private bool _isActive = true;
        private long _startTicks = 0;
        private long _elapsedTicks = 0;

        public ArAnimation(long poiId, GameObject gameObject, PoiAnimation poiAnimation)
        {
            PoiId = poiId;
            GameObject = gameObject;
            if (poiAnimation != null)
            {
                _lengthTicks = (long)(10000000.0 * poiAnimation.length);
                _delayTicks = (long)(10000000.0 * poiAnimation.delay);
                if (poiAnimation.type != null)
                {
                    _type = poiAnimation.type.ToLower().Contains(Rotate) ? Rotate
                        : poiAnimation.type.ToLower().Contains(Scale) ? Scale
                        : Transform;
                }
                if (poiAnimation.interpolation != null)
                {
                    _interpolation = poiAnimation.interpolation.ToLower().Contains(Cyclic) ? Cyclic : Linear;
                    _interpolationType = poiAnimation.interpolation.ToLower().Contains(Halfsine) ? Halfsine
                        : poiAnimation.interpolation.ToLower().Contains(Sine) ? Sine
                        : string.Empty;
                }
                _persisting = poiAnimation.persist;
                _repeating = poiAnimation.repeat;
                _from = poiAnimation.from;
                _to = poiAnimation.to;
                _axis = poiAnimation.axis == null ? Vector3.zero
                    : new Vector3(poiAnimation.axis.x, poiAnimation.axis.y, poiAnimation.axis.z);
            }
        }

        public void Stop(long worldStartTicks, long nowTicks, bool animate = true)
        {
            if (animate)
            {
                Animate(worldStartTicks, nowTicks);
            }
            _isActive = false;

            if (_persisting)
            {
                _elapsedTicks = nowTicks - _startTicks;
            }
            else
            {
                _elapsedTicks = 0;
                if (Rotate.Equals(_type))
                {
                    GameObject.transform.eulerAngles = Vector3.zero;
                }
                else if (Scale.Equals(_type))
                {
                    GameObject.transform.localScale = Vector3.one;
                }
                else if (Transform.Equals(_type))
                {
                    GameObject.transform.localPosition = Vector3.zero;
                }
            }
            return;
        }

        public void Activate(long worldStartTicks, long nowTicks)
        {
            _isActive = true;
            if (_elapsedTicks > 0)
            {
                _startTicks = nowTicks - _elapsedTicks;
            }
            else
            {
                _startTicks = 0;
            }
            Animate(worldStartTicks, nowTicks);
        }

        public void Animate(long worldStartTicks, long nowTicks)
        {
            if (worldStartTicks <= 0 || !_isActive || _lengthTicks < 1 || _delayTicks < 0)
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

            if (Rotate.Equals(_type))
            {
                GameObject.transform.eulerAngles = new Vector3(
                    _axis.x * animationFactor,
                    _axis.y * animationFactor,
                    _axis.z * animationFactor
                    );
            }
            else if (Scale.Equals(_type))
            {
                GameObject.transform.localScale = new Vector3(
                    _axis.x == 0 ? 1 : _axis.x * animationFactor,
                    _axis.y == 0 ? 1 : _axis.y * animationFactor,
                    _axis.z == 0 ? 1 : _axis.z * animationFactor
                    );
            }
            else if (Transform.Equals(_type))
            {
                GameObject.transform.localPosition = new Vector3(
                    _axis.x * animationFactor,
                    _axis.y * animationFactor,
                    _axis.z * animationFactor
                    );
            }
        }
    }
}
