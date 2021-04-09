/*
EvolutionOfFish.cs - Gameplay of initial version of Evolution of Fish for ARpoise.

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
using com.arpoise.arpoiseapp;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionOfFish : ArFlock
{
    public GameObject ArCamera = null;
    public GameObject InfoText = null;

    public GameObject FishPrefab0;
    public GameObject FishPrefab1;
    public GameObject FishPrefab2;
    public GameObject FishPrefab3;
    public GameObject FishPrefab4;

    protected new void Start()
    {
        base.Start();
    }

    private bool _first = true;
    private void First()
    {
        _first = false;

        SwimLimits = new Vector3(12, 12, 12);

        var allFish = new GameObject[100];
        for (int i = 0; i < allFish.Length; i++)
        {
            var pos = transform.position + new Vector3(Random.Range(-SwimLimits.x, SwimLimits.x),
                                                       Random.Range(-SwimLimits.y, SwimLimits.y),
                                                       Random.Range(-SwimLimits.z, SwimLimits.z));
            GameObject fish;
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
            // put the fish below the EOF-GO, so it gets deleted if the GO gets deleted
            fish.transform.parent = transform;
            allFish[i] = fish;
            allFish[i].GetComponent<ArFish>().Flock = this;
        }
        AllFish = allFish;
        ShowFishOrGarbage(true);
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
                _fish = Transforms
                    .Select(x => x.gameObject)
                    .Where(x => "Fish".Equals(x.gameObject.name)).ToArray();
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
                _garbage = Transforms
                    .Select(x => x.gameObject)
                    .Where(x => "Garbage".Equals(x.gameObject.name)).ToArray();
            }
            return _garbage;
        }
    }

    private bool _showFish = false;
    private void ShowFishOrGarbage(bool showFish)
    {
        if (_showFish == showFish)
        {
            return;
        }
        _showFish = showFish;

        foreach (var fish in Fish)
        {
            fish.SetActive(showFish);
        }
        foreach (var garbage in Garbage)
        {
            garbage.SetActive(!showFish);
        }
    }


    private readonly float _sittingOnStandDistance = .2f;
    private float _garbageDistance = .2f;
    private Vector3 _lastForward;
    private float _difference = 0.01f;
    private Vector3 _nearScale = new Vector3(0, 2, 0);
    protected void Update()
    {
        if (ArCamera == null)
        {
            return;
        }
        if (_first)
        {
            First();
        }

        _garbageDistance += .001f;

        var forward = ArCamera.transform.forward;
        var difference = Vector3.Distance(_lastForward, forward);
        _lastForward = forward;

        _difference = _difference * .99f + difference / 100;
        var distance = _difference * 1000f;

        var showFish = true;
        Vector3 newPosition;

        if (distance < _sittingOnStandDistance)
        {
            _garbageDistance = _sittingOnStandDistance;
            MinNeighbourDistance = 1.8f;
            newPosition = 10 * _lastForward;
        }
        else if (distance < _garbageDistance)
        {
            showFish = false;
            MinNeighbourDistance = .5f;
            newPosition = Vector3.Scale(_lastForward, _nearScale);
        }
        else if (distance > 10)
        {
            MinNeighbourDistance = 1.8f;
            newPosition = 10 * _lastForward;
        }
        else
        {
            MinNeighbourDistance = 1.8f;
            newPosition = distance * _lastForward;
        }
        ShowFishOrGarbage(showFish);

        GoalPosition = newPosition;

        if (InfoText != null)
        {
            InfoText.GetComponent<Text>().text =
                    string.Empty
                    + " D " + distance.ToString("F3")
                    + " G " + _garbageDistance.ToString("F3")
                    ;
        }
    }
}
