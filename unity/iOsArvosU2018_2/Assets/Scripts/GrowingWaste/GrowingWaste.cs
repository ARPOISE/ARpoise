/*
GrowingWaste.cs - Behaviour for a game object of 'Growing Waste' for Arpoise.

Copyright (C) 2020, Tamiko Thiel and Peter Graf - All Rights Reserved

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
using System.Linq;
using System;
using static com.arpoise.arpoiseapp.GrowingWaste.GrowingWasteBase;

namespace com.arpoise.arpoiseapp.GrowingWaste
{
    public class GrowingWaste : GrowingWasteBase, IBuildCallback
    {
        [SerializeField]
        protected int Year = -1;
        [SerializeField]
        protected int Index = -1;
        [SerializeField]
        protected int Age = -1;
        [SerializeField]
        protected int MaxAge = -1;
        [SerializeField]
        protected string Fitness;
        [SerializeField]
        protected string Weights;
        [SerializeField]
        protected GameObject TrunkObject;
        [SerializeField]
        protected Material TrunkMaterial;
        [SerializeField]
        protected string Derivation = "F[-&^F][^++&F]||F[--&^F][+&F]";

        private GameObject GetTrunkObject()
        {
            if (gameObject != null)
            {
                var trunkObject = Instantiate(gameObject);
                trunkObject.SetActive(false);
                gameObject.GetComponent<MeshFilter>().mesh = new Mesh();
                return trunkObject;
            }
            return null;
        }
        protected void Start()
        {
            StartBackend();
            if (TrunkObject == null)
            {
                if (gameObject != null)
                {
                    TrunkObject = GetTrunkObject();
                }
            }
            if (TrunkObject != null)
            {
                Index = GetIndex(TrunkObject);
            }
        }

        private Creature _creature;

        private long _lastUpdateMilliseconds;

        private float Size
        {
            get
            {
                var factor = 1f;
                if (_creature != null)
                {
                    if (_creature.Bounds.max.y <= 1f)
                    {
                        factor = 1.7f;
                    }
                    else if (_creature.Bounds.max.y <= 2f)
                    {
                        factor = 1.3f;
                    }
                    else if (_creature.Bounds.max.y <= 3f)
                    {
                        factor = 1.1f;
                    }
                }

                if (Age < 25)
                {
                    return factor * (0.01f + Age / 25f);
                }
                else if (Age > MaxAge - 20)
                {
                    return factor * (Math.Max((MaxAge - Age) / 20f, 0.00f));
                }
                return factor;
            }
        }

        //[SerializeField]
        protected float ColorA = -1;

        private float? _colorA;

        private float Fade
        {
            get
            {
                if (Age > MaxAge - 25)
                {
                    return (Math.Max((MaxAge - Age) / 25f, 0.00f));
                }
                return 1f;
            }
        }

        protected void Update()
        {
            KeepBackendAlive = true;

            if (TrunkObject == null)
            {
                if (gameObject != null)
                {
                    TrunkObject = GetTrunkObject();
                }
            }
            if (Index < 0)
            {
                Index = GetIndex(TrunkObject);
            }

            var creatures = Creatures;
            if (creatures == null || creatures.Count <= 1)
            {
                return;
            }

            var milliSeconds = DateTime.Now.Ticks / 10000;
            if (milliSeconds - _lastUpdateMilliseconds < 200)
            {
                if (_creature != null)
                {
                    if (_creature.Trunk != null)
                    {
                        var fade = Fade;
                        if (fade < 1)
                        {
                            for (int i = 0; i < _creature.Trunk.transform.childCount; i++)
                            {
                                var chunk = _creature.Trunk.transform.GetChild(i);
                                var material = chunk.GetComponent<MeshRenderer>().material;
                                var color = material.color;
                                if (_colorA == null)
                                {
                                    _colorA = color.a;
                                }
                                ColorA = Mathf.Lerp(color.a, _colorA.Value * fade, .05f);
                                material.color = new Color(color.r, color.g, color.b, ColorA);
                            }
                        }
                        var size = Size;
                        var scale = _creature.Trunk.transform.localScale;
                        if (scale.x != size)
                        {
                            _creature.Trunk.transform.localScale = Vector3.Lerp(scale, new Vector3(size, size, size), .05f);
                        }
                    }
                }
                return;
            }
            Year++;
            _lastUpdateMilliseconds = milliSeconds;
            Weights = Creature.Weights;

            if (_creature != null)
            {
                if (Age++ > MaxAge)
                {
                    _creature.Trunk = null;
                    _creature = null;
                }
            }

            if (_creature == null)
            {
                if (TrunkMaterial == null)
                {
                    TrunkMaterial = GetMaterial(Index);
                }
                RequestBuild(Index, creatures.First().Derivation, TrunkObject, TrunkMaterial);
            }

            if (_creature != null)
            {
                var fade = Fade;
                if (fade < 1)
                {
                    for (int i = 0; i < _creature.Trunk.transform.childCount; i++)
                    {
                        var chunk = _creature.Trunk.transform.GetChild(i);
                        var material = chunk.GetComponent<MeshRenderer>().material;
                        var color = material.color;
                        if (_colorA == null)
                        {
                            _colorA = color.a;
                        }
                        ColorA = Mathf.Lerp(color.a, _colorA.Value * fade, .05f);
                        material.color = new Color(color.r, color.g, color.b, ColorA);
                    }
                }
                var size = Size;
                var scale = _creature.Trunk.transform.localScale;
                if (scale.x != size)
                {
                    _creature.Trunk.transform.localScale = Vector3.Lerp(scale, new Vector3(size, size, size), .05f);
                }
            }
        }

        private Creature _newCreature;
        private bool _buildrequested;
        private void RequestBuild(int index, string derivation, GameObject trunkObject, Material trunkMaterial)
        {
            if (_buildrequested || ArBehaviourUserInterface.FramesPerSecond < 20)
            {
                return;
            }
            _buildrequested = true;

            _newCreature = new Creature(index, derivation, trunkObject, trunkMaterial);
            Derivation = _newCreature.Derive();
            string moduleString = _newCreature.Derive(Creature.Axiom, Creature.Derivations, Derivation);
            Build(
                _newCreature.TrunkObject,
                Creature.SegmentHeight,
                _newCreature.TrunkMaterial,
                Creature.Angle,
                moduleString,
                this
                );
        }

        public void BuildFinished(GameObject trunk, Bounds bounds, Vector3 center)
        {
            _newCreature.Trunk = trunk;
            _newCreature.Bounds = bounds;
            _newCreature.Center = center;

            _newCreature.Trunk.transform.parent = transform;
            _newCreature.Trunk.transform.localPosition = transform.position;
            _newCreature.Trunk.transform.localScale = Vector3.zero;

            ColorA = -1;
            _colorA = null;
            Age = 0;
            MaxAge = 5 * Index + 80 + _newCreature.NextRandom(40);
            if (Year < 50)
            {
                MaxAge = (int)(MaxAge * 0.4f);
            }
            else if (Year < 100)
            {
                MaxAge = (int)(MaxAge * 0.6f);
            }
            else if (Year < 150)
            {
                MaxAge = (int)(MaxAge * 0.8f);
            }
            Fitness = _newCreature.FitnessString;
            _creature = _newCreature;
            _buildrequested = false;
        }
    }
}
