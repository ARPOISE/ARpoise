/*
ArFlock.cs - Flocking behaviour of for an entire flock for Arpoise.

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
using UnityEngine;

public class ArFlock : MonoBehaviour
{
    public float NeighbourDistance = 2.5f;
    public float MinNeighbourDistance = 1.8f;

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

    protected void Update()
    {
    }
}
