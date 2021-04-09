/*
ArFish.cs - Flocking behaviour of a single fish for ARpoise.

    The code is derived from the video
    https://www.youtube.com/watch?v=a7GkPNMGz8Y
    by Holistic3d, aka Professor Penny de Byl.

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

using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArFish : MonoBehaviour
    {
        private ArFlock _flock;
        public ArFlock Flock { set { _flock = value; } }

        private float _speed = 0.001f;

        protected void Start()
        {
            var flock = _flock;
            if (flock == null)
            {
                return;
            }
            _speed = Random.Range(flock.MinimumSpeed, flock.MaximumSpeed);
        }

        protected void Update()
        {
            var flock = _flock;
            if (flock == null)
            {
                return;
            }

            var allFish = flock.AllFish;
            if (allFish == null)
            {
                return;
            }

            //determine the bounding box of the manager cube
            var b = new Bounds(flock.transform.position, flock.SwimLimits * 2);

            //if fish is outside the bounds of the cube then start turning around
            if (!b.Contains(transform.position))
            {
                //turn towards the centre of the manager cube
                var direction = flock.transform.position - transform.position;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.LookRotation(direction),
                                                      _flock.RotationSpeed * Time.deltaTime);
                _speed = Random.Range(flock.MinimumSpeed, flock.MaximumSpeed) * _flock.SpeedFactor;
            }
            else
            {
                if (Random.Range(0, 100) < _flock.ApplyRulesPercentage)
                {
                    ApplyRules(flock, allFish);
                }
            }
            transform.Translate(0, 0, Time.deltaTime * _speed * _flock.SpeedFactor);
        }

        private void ApplyRules(ArFlock flock, GameObject[] allFish)
        {
            var centerDirection = Vector3.zero;
            var avoidDirection = Vector3.zero;
            float groupSpeed = 0.01f;

            float distance;

            int groupSize = 0;
            foreach (var fish in allFish)
            {
                if (fish != gameObject)
                {
                    distance = Vector3.Distance(fish.transform.position, transform.position);
                    if (distance <= _flock.NeighbourDistance)
                    {
                        centerDirection += fish.transform.position;
                        groupSize++;

                        if (distance < _flock.MinNeighbourDistance)
                        {
                            avoidDirection += transform.position - fish.transform.position;
                        }

                        var otherFish = fish.GetComponent<ArFish>();
                        groupSpeed += otherFish._speed;
                    }
                }
            }

            if (groupSize > 0)
            {
                centerDirection = centerDirection / groupSize + (flock.GoalPosition - transform.position);
                _speed = groupSpeed / groupSize * _flock.SpeedFactor;

                var direction = (centerDirection + avoidDirection) - transform.position;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                                          Quaternion.LookRotation(direction),
                                                          _flock.RotationSpeed * Time.deltaTime);
                }
            }
        }
    }
}
