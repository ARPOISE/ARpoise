/*
ArFlock.cs - Flocking behaviour for an entire flock for Arpoise.

    The code is derived from the video
    https://www.youtube.com/watch?v=a7GkPNMGz8Y
    by Holistic3d, aka Professor Penny de Byl.

Copyright (C) 2019, Tamiko Thiel and Peter Graf - All Rights Reserved

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
using com.arpoise.arpoiseapp;
using UnityEngine;

public class ArFlock : MonoBehaviour
{
    #region Parameters
    private float? _neighbourDistance;
    public float NeighbourDistance
    {
        get
        {
            if (_neighbourDistance.HasValue)
            {
                return _neighbourDistance.Value;
            }
            return 2.5f;
        }
        set
        {
            _neighbourDistance = value;
        }
    }

    private float? _minNeighbourDistance;
    public float MinNeighbourDistance
    {
        get
        {
            if (_minNeighbourDistance.HasValue)
            {
                return _minNeighbourDistance.Value;
            }
            return 1.8f;
        }
        set
        {
            _minNeighbourDistance = value;
        }
    }

    private float? _speedFactor;
    public float SpeedFactor
    {
        get
        {
            if (_speedFactor.HasValue)
            {
                return _speedFactor.Value;
            }
            return 1f;
        }
        set
        {
            _speedFactor = value;
        }
    }

    private float? _rotationSpeed;
    public float RotationSpeed
    {
        get
        {
            if (_rotationSpeed.HasValue)
            {
                return _rotationSpeed.Value;
            }
            return 4f;
        }
        set
        {
            _rotationSpeed = value;
        }
    }

    private float? _minimumSpeed;
    public float MinimumSpeed
    {
        get
        {
            if (_minimumSpeed.HasValue)
            {
                return _minimumSpeed.Value;
            }
            return .7f;
        }
        set
        {
            _minimumSpeed = value;
        }
    }

    private float? _maximumSpeed;
    public float MaximumSpeed
    {
        get
        {
            if (_maximumSpeed.HasValue)
            {
                return _maximumSpeed.Value;
            }
            return 2f;
        }
        set
        {
            _maximumSpeed = value;
        }
    }

    private float? _applyRulesPercentage;
    public float ApplyRulesPercentage
    {
        get
        {
            if (_applyRulesPercentage.HasValue)
            {
                return _applyRulesPercentage.Value;
            }
            return 20f;
        }
        set
        {
            _applyRulesPercentage = value;
        }
    }

    public virtual void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals("NeighbourDistance"))
        {
            _neighbourDistance = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals("MinNeighbourDistance"))
        {
            _minNeighbourDistance = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals("SpeedFactor"))
        {
            _speedFactor = SetParameter(setValue, value, (float?)null).Value;
        }
        else if (label.Equals("RotationSpeed"))
        {
            _rotationSpeed = SetParameter(setValue, value, (float?)null).Value;
        }
        else if (label.Equals("ApplyRulesPercentage"))
        {
            _applyRulesPercentage = SetParameter(setValue, value, (float?)null).Value;
        }
        else if (label.Equals("MinimumSpeed"))
        {
            _minimumSpeed = SetParameter(setValue, value, (float?)null).Value;
        }
        else if (label.Equals("MaximumSpeed"))
        {
            _maximumSpeed = SetParameter(setValue, value, (float?)null).Value;
        }
    }

    protected int? SetParameter(bool setValue, string value, int? defaultValue)
    {
        if (setValue && !string.IsNullOrWhiteSpace(value))
        {
            int intValue;
            if (int.TryParse(value, out intValue))
            {
                return intValue;
            }
        }
        return defaultValue;
    }

    protected float? SetParameter(bool setValue, string value, float? defaultValue)
    {
        if (setValue && !string.IsNullOrWhiteSpace(value))
        {
            float floatValue;
            if (float.TryParse(value, out floatValue))
            {
                return floatValue;
            }
        }
        return defaultValue;
    }
    #endregion

    public Vector3 GoalPosition;

    //set the size of the bounding box to keep the fish within.
    //its actual side length will be twice the values given here
    public Vector3 SwimLimits = new Vector3(42, 42, 42);

    private GameObject[] _allFish = null;
    public GameObject[] AllFish { get { return _allFish; } set { _allFish = value; } }

    protected void Start()
    {
        GoalPosition = transform.position;
        RenderSettings.fogColor = Camera.main.backgroundColor;
        RenderSettings.fogDensity = 0.03F;
        RenderSettings.fog = true;
    }
}
