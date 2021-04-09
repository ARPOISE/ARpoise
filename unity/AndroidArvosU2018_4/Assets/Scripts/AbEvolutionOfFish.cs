/*
AbEvolutionOfFish.cs - Gameplay of Alys Beach version of Evolution of Fish for ARpoise.

Copyright (C) 2019, Tamiko Thiel and Peter Graf - All Rights Reserved

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
using com.arpoise.arpoiseapp;
using UnityEngine;

public class AbEvolutionOfFish : ArFlock
{
    public int Index = 0;
    public GameObject ArCamera = null;
    public GameObject InfoText = null;

    public GameObject FishPrefab0;
    public GameObject FishPrefab1;
    public GameObject FishPrefab2;
    public GameObject FishPrefab3;
    public GameObject FishPrefab4;
    public GameObject FishPrefab5;
    public GameObject FishPrefab6;

    #region Parameters
    private int? _numberOfFish;
    public int NumberOfFish
    {
        get
        {
            if (_numberOfFish.HasValue)
            {
                return _numberOfFish.Value;
            }
            return 100;
        }
    }

    private float? _goalPositionX;
    public float GoalPositionX
    {
        get
        {
            if (_goalPositionX.HasValue)
            {
                return _goalPositionX.Value;
            }
            return 2.0f;
        }
    }

    private float? _cycleDurationForward;
    public float CycleDurationForward
    {
        get
        {
            if (_cycleDurationForward.HasValue)
            {
                return _cycleDurationForward.Value;
            }
            return 13.0f;
        }
    }

    private float? _cycleDurationRight;
    public float CycleDurationRight
    {
        get
        {
            if (_cycleDurationRight.HasValue)
            {
                return _cycleDurationRight.Value;
            }
            return 24.1f;
        }
    }

    private float? _toRight;
    public float ToRight
    {
        get
        {
            if (_toRight.HasValue)
            {
                return _toRight.Value;
            }
            return 6.0f;
        }
    }

    public float FromRight
    {
        get
        {
            return 0f;
        }
    }

    private float? _toForward;
    public float ToForward
    {
        get
        {
            if (_toForward.HasValue)
            {
                return _toForward.Value;
            }
            return 20.0f;
        }
    }

    private float? _fromForward;
    public float FromForward
    {
        get
        {
            if (_fromForward.HasValue)
            {
                return _fromForward.Value;
            }
            return 15.0f;
        }
    }

    private float? _movementThreshold;
    public float MovementThreshold
    {
        get
        {
            if (_movementThreshold.HasValue)
            {
                return _movementThreshold.Value;
            }
            return 2.5f;
        }
    }

    private float? _movementFactor;
    public float MovementFactor
    {
        get
        {
            if (_movementFactor.HasValue)
            {
                return _movementFactor.Value;
            }
            return 30f;
        }
    }

    private int? _swimLimitX;
    public int SwimLimitX
    {
        get
        {
            if (_swimLimitX.HasValue)
            {
                return _swimLimitX.Value;
            }
            return 32;
        }
    }

    private int? _swimLimitY;
    public int SwimLimitY
    {
        get
        {
            if (_swimLimitY.HasValue)
            {
                return _swimLimitY.Value;
            }
            return 12;
        }
    }

    private int? _swimLimitZ;
    public int SwimLimitZ
    {
        get
        {
            if (_swimLimitZ.HasValue)
            {
                return _swimLimitZ.Value;
            }
            return 32;
        }
    }

    public override void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(NumberOfFish)))
        {
            _numberOfFish = SetParameter(setValue, value, (int?)null);
        }
        else if (label.Equals(nameof(ToForward)))
        {
            _toForward = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(FromForward)))
        {
            _fromForward = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(ToRight)))
        {
            _toRight = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(CycleDurationForward)))
        {
            _cycleDurationForward = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(CycleDurationRight)))
        {
            _cycleDurationRight = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(GoalPositionX)))
        {
            _goalPositionX = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(MovementThreshold)))
        {
            _movementThreshold = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(MovementFactor)))
        {
            _movementFactor = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(SwimLimitX)))
        {
            _swimLimitX = SetParameter(setValue, value, (int?)null);
        }
        else if (label.Equals(nameof(SwimLimitY)))
        {
            _swimLimitY = SetParameter(setValue, value, (int?)null);
        }
        else if (label.Equals(nameof(SwimLimitZ)))
        {
            _swimLimitZ = SetParameter(setValue, value, (int?)null);
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }
    #endregion

    protected new void Start()
    {
        base.Start();
    }

    private bool _first = true;
    private void First()
    {
        _first = false;
        if (Index == 0)
        {
            NeighbourDistance = 6f;
            MinNeighbourDistance = 3f;
        }

        _lengthTicksForward = (long)(10000000.0 * CycleDurationForward);
        _lengthTicksRight = (long)(10000000.0 * CycleDurationRight);
        SwimLimits = new Vector3(SwimLimitX, SwimLimitY, SwimLimitZ);

        var allFish = new GameObject[NumberOfFish];
        for (int i = 0; i < allFish.Length; i++)
        {
            var pos = transform.position + new Vector3(UnityEngine.Random.Range(-SwimLimits.x, SwimLimits.x),
                                                       UnityEngine.Random.Range(-SwimLimits.y, SwimLimits.y),
                                                       UnityEngine.Random.Range(-SwimLimits.z, SwimLimits.z));
            GameObject fish;
            if (Index == 0)
            {
                fish = Instantiate(FishPrefab6, pos, Quaternion.identity);
            }
            else
            {
                switch (i % 5)
                {
                    case 1:
                        fish = Instantiate(FishPrefab1, pos, Quaternion.identity);
                        break;
                    case 2:
                        fish = Instantiate(FishPrefab2, pos, Quaternion.identity);
                        break;
                    case 3:
                        fish = Instantiate(FishPrefab3, pos, Quaternion.identity);
                        break;
                    case 4:
                        fish = Instantiate(FishPrefab4, pos, Quaternion.identity);
                        break;
                    default:
                        fish = Instantiate(FishPrefab0, pos, Quaternion.identity);
                        break;
                }
            }
            // put the fish below the EOF-GO, so it gets deleted if the GO gets deleted
            fish.transform.parent = transform;
            allFish[i] = fish;
            allFish[i].GetComponent<ArFish>().Flock = this;
        }
        AllFish = allFish;
        ShowFishOrGarbage(0);
    }

    private Transform[] _transforms;
    private Transform[] Transforms
    {
        get
        {
            if (_transforms == null)
            {
                var list = new List<Transform>();
                foreach (var item in AllFish)
                {
                    list.AddRange(item.GetComponentsInChildren<Transform>().Where(x => x.gameObject != null));
                }
                _transforms = list.ToArray();
            }
            return _transforms;
        }
    }

    private GameObject[] _fish;
    private GameObject[] Fish
    {
        get
        {
            if (_fish == null)
            {
                _fish = Transforms.Select(x => x.gameObject).Where(x => nameof(Fish).Equals(x.gameObject.name)).ToArray();
            }
            return _fish;
        }
    }

    private GameObject[] _garbage;
    private GameObject[] Garbage
    {
        get
        {
            if (_garbage == null)
            {
                _garbage = Transforms.Select(x => x.gameObject).Where(x => nameof(Garbage).Equals(x.gameObject.name)).ToArray();
            }
            return _garbage;
        }
    }

    private void ShowFishOrGarbage(int percentage)
    {
        if (percentage < 0)
        {
            percentage = 0;
        }
        else if (percentage > 100)
        {
            percentage = 100;
        }

        var percentageValue = (Fish.Length / 100f) * percentage;
        for (int i = 0; i < Fish.Length; i++)
        {
            var showFish = i >= percentageValue;
            var fish = Fish[i];
            if (fish.activeSelf != showFish)
            {
                fish.SetActive(showFish);
            }
            var garbage = Garbage[i];
            if (garbage.activeSelf == showFish)
            {
                garbage.SetActive(!showFish);
            }
        }
    }

    private long _startTicksForward = 0;
    private long _lengthTicksForward = (long)(10000000.0 * 10f);

    private long _startTicksRight = 0;
    private long _lengthTicksRight = (long)(10000000.0 * 14.2f);

    private Vector3 _lastForward;
    private float _difference = 0;
    private int _count = 0;

    protected void Update()
    {
        if (ArCamera == null)
        {
            ArCamera = Camera.main.gameObject;
        }
        if (ArCamera == null)
        {
            return;
        }
        if (_first)
        {
            First();
        }

        var forward = ArCamera.transform.forward;
        var difference = Vector3.Distance(_lastForward, forward);
        _lastForward = forward;
        float percentage = _difference = _difference * .999f + difference / 1000;

        if (_count++ % 10 == 9)
        {
            _count = 0;

            percentage *= 1000f;
            percentage = (percentage - MovementThreshold) * MovementFactor;
            if (percentage < 0)
            {
                percentage = 0;
            }
            else if (percentage > 100)
            {
                percentage = 100;
            }
            if (Index == 0)
            {
                ArBehaviour.DisplayPercentage = percentage;
            }
            ShowFishOrGarbage((int)percentage);
        }

        long nowTicks = DateTime.Now.Ticks;

        float animationValueForward = 0;
        if (_startTicksForward == 0)
        {
            _startTicksForward = nowTicks;
        }
        else
        {
            var endTicks = _startTicksForward + _lengthTicksForward;
            if (endTicks < nowTicks)
            {
                if (endTicks + _lengthTicksForward < nowTicks)
                {
                    _startTicksForward = nowTicks;
                }
                else
                {
                    _startTicksForward += _lengthTicksForward;
                }
            }
            animationValueForward = (nowTicks - _startTicksForward) / ((float)_lengthTicksForward);
        }

        float animationValueRight = 0;
        if (_startTicksRight == 0)
        {
            _startTicksRight = nowTicks;
        }
        else
        {
            var endTicks = _startTicksRight + _lengthTicksRight;
            if (endTicks < nowTicks)
            {
                if (endTicks + _lengthTicksRight < nowTicks)
                {
                    _startTicksRight = nowTicks;
                }
                else
                {
                    _startTicksRight += _lengthTicksRight;
                }
            }
            animationValueRight = (nowTicks - _startTicksRight) / ((float)_lengthTicksRight);
        }

        animationValueForward = .5f + (-1f + (float)Math.Cos(2 * Math.PI * animationValueForward)) / 2;
        animationValueRight = .5f + (-1f + (float)Math.Cos(2 * Math.PI * animationValueRight)) / 2;

        if (Index == 0)
        {
            ArBehaviour.DisplayAnimationValueForward = animationValueForward;
            ArBehaviour.DisplayAnimationValueRight = animationValueRight;
        }

        var animationFactorForward = FromForward + (ToForward - FromForward) * animationValueForward;
        var animationFactorRight = FromRight + (ToRight - FromRight) * animationValueRight;

        GoalPosition = animationFactorForward * ArCamera.transform.forward + animationFactorRight * ArCamera.transform.right;
        GoalPosition += new Vector3(0, GoalPositionX, 0);

        if (Index == 0)
        {
            ArBehaviour.DisplayGoalPosition = GoalPosition;
        }
    }
}
