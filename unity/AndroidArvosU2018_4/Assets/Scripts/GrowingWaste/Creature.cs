/*
Creature.cs - A creature grown with a genetical algorithm of 'Growing Waste' for Arpoise.

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

using System;
using System.Text;
using UnityEngine;

namespace com.arpoise.arpoiseapp.GrowingWaste
{
    public class Creature
    {
        public const float Angle = 22.5f;
        public const float SegmentHeight = 1f;
        public const string Axiom = "F";
        public const char AxiomLetter = 'F';
        public const int Derivations = 2;

        public GameObject TrunkObject { get; private set; }
        public Material TrunkMaterial { get; private set; }
        public int Index { get; private set; }

        public Creature(int index, string derivation, GameObject trunkObject, Material trunkMaterial)
        {
            Index = index;
            Derivation = derivation;
            TrunkObject = trunkObject;
            TrunkMaterial = trunkMaterial;
        }

        public Creature(int index, GameObject trunkObject, Material trunkMaterial)
        {
            Index = index;
            TrunkObject = trunkObject;
            TrunkMaterial = trunkMaterial;

            Randomize();
        }

        public Creature(int index, Creature mother, Creature father, GameObject trunkObject, Material trunkMaterial)
        {
            Index = index;
            TrunkObject = trunkObject;
            TrunkMaterial = trunkMaterial;

            Crossover(mother, father);
        }

        public Creature(int index,Creature mother, int n, GameObject trunkObject, Material trunkMaterial)
        {
            Index = index;
            TrunkObject = trunkObject;
            TrunkMaterial = trunkMaterial;

            Mutate(mother, n);
        }

        public void FastDerive()
        {
            string moduleString = Derive(Axiom, Derivations, Derive());

            Vector3 center;
            Bounds bounds;
            GrowingWasteBase.Interpret(
                SegmentHeight,
                Angle,
                moduleString,
                out bounds,
                out center
                );

            Trunk = null;
            Bounds = bounds;
            Center = center;
        }

        public string Derive(string axiom, int derivations, string successor)
        {
            var result = axiom;
            int n = Math.Max(1, derivations);
            while (n-- > 0)
            {
                var sb = new StringBuilder();
                foreach (var letter in result)
                {
                    if (AxiomLetter.Equals(letter))
                    {
                        sb.Append(successor);
                    }
                    else
                    {
                        sb.Append(letter);
                    }
                }
                result = sb.ToString();
            }
            return result;
        }

        private readonly int[] _genom = new int[32];
        private readonly string[] _alphabet = new string[] { "F", "+", "-", "&", "^", "/", "\\", "[" };

        private static System.Random _random = new System.Random();

        public bool IsOld { get { return Age++ > 99; } }

        public int Age { get; private set; }

        public int NextRandom(int maxValue)
        {
            return _random.Next(maxValue);
        }

        private static readonly RandomWeight _weight1 = new RandomWeight();
        private static readonly RandomWeight _weight2 = new RandomWeight();
        private static readonly RandomWeight _weight3 = new RandomWeight();
        private static readonly RandomWeight _weight4 = new RandomWeight();
        private static readonly RandomWeight _weight5 = new RandomWeight();

        public void Update()
        {
            _weight1.Update();
            _weight2.Update();
            _weight3.Update();
            _weight4.Update();
            _weight5.Update();
        }

        public static string Weights
        {
            get
            {
                return _weight1.Weight.ToString("F3") + " "
                    + _weight2.Weight.ToString("F3") + " "
                    + _weight3.Weight.ToString("F3") + " "
                    + _weight4.Weight.ToString("F3") + " "
                    + _weight5.Weight.ToString("F3");
            }
        }

        public string FitnessString
        {
            get
            {
                var fitness = Fitness;
                if (fitness == float.MinValue)
                {
                    return "---";
                }
                return fitness.ToString("F2");
            }
        }

        private float? _fitness;
        public float Fitness
        {
            get
            {
                if (Age % 10 == 0)
                {
                    _fitness = null;
                }
                if (_fitness == null && _bounds != null)
                {
                    if (_bounds.max.y >= 1)
                    {
                        _fitness = _weight1.Weight * _bounds.center.y;

                        _fitness += _weight2.Weight * Mathf.Abs(Center.x);
                        _fitness += _weight2.Weight * Mathf.Abs(Center.z);
                        _fitness += (_weight3.Weight / 6) * _bounds.extents.x * _bounds.extents.z;
                        _fitness += Math.Max(-1.0f, _weight4.Weight) * Mathf.Abs(_bounds.extents.x - _bounds.extents.z);
                        _fitness += Math.Max(-1.0f, _weight5.Weight / 10) * _numberOfSplits * _numberOfSplits * _numberOfSplits;

                        _fitness += -2.0f * Math.Max(0, _bounds.extents.x * _bounds.extents.z - 40);
                        _fitness += -2.0f * Math.Max(0, _bounds.extents.x + _bounds.extents.z + _bounds.extents.y - 12);
                        _fitness += -2.0f * Math.Max(0, _bounds.extents.x + _bounds.extents.z - 8);
                        _fitness += -2.0f * Math.Max(0, _bounds.extents.x - 4);
                        _fitness += -2.0f * Math.Max(0, _bounds.extents.z - 4);
                        _fitness += -2.0f * Math.Max(0, _bounds.extents.y - 4);
                        _fitness += -2.0f * Math.Max(0, -_bounds.min.y);
                        if (NumberOfSegments > 6)
                        {
                            _fitness += -2.0f * (NumberOfSegments - 5) * (NumberOfSegments - 5) * (NumberOfSegments - 5);
                        }
                    }
                }
                return _fitness == null ? float.MinValue : _fitness.Value;
            }
        }

        private float AgeFactor
        {
            get
            {
                if (Age < 25)
                {
                    return 0.01f + Age / 25f;
                }
                else if (Age > 75)
                {
                    return Math.Max((100 - Age) / 25f, 0.01f);
                }
                return 1f;
            }
        }

        public float GetSize(int rank, float nCreatures)
        {
            var ageRank = 15;
            if (rank < ageRank)
            {
                return AgeFactor * 1f;
            }
            rank -= ageRank;
            nCreatures -= ageRank;

            return Math.Min(AgeFactor * (0.1f + (nCreatures - rank) / nCreatures), 1f);
        }

        private Vector3 _center;
        public Vector3 Center
        {
            get { return _center; }
            set
            {
                _fitness = null;
                _center = value;
            }
        }

        private Bounds _bounds;
        public Bounds Bounds
        {
            get { return _bounds; }
            set
            {
                _fitness = null;
                _bounds = value;
            }
        }

        private GameObject _trunk;
        public GameObject Trunk
        {
            get
            {
                return _trunk;
            }
            set
            {
                if (_trunk != null)
                {
                    _trunk.transform.parent = null;
                    _trunk.transform.localScale = Vector3.zero;
                    UnityEngine.Object.Destroy(_trunk);
                }
                _fitness = null;
                _trunk = value;
            }
        }

        public string Derivation { get; private set; }

        public string Derive()
        {
            if (Derivation != null)
            {
                return Derivation;
            }

            var result = new StringBuilder();
            //result.Append(Axiom);

            int bracketCount = 0;
            foreach (var gene in _genom)
            {
                var letter = _alphabet[gene];
                if (letter.StartsWith("["))
                {
                    if (bracketCount > 0)
                    {
                        letter = "F]";
                        bracketCount--;
                    }
                    else
                    {
                        bracketCount++;
                    }
                }
                result.Append(letter);
            }
            while (bracketCount-- > 0)
            {
                result.Append("F]");
            }
            _numberOfSegments = -1;
            _numberOfSplits = -1;
            var derivation = result.ToString();
            if (!derivation.EndsWith(Axiom))
            {
                derivation += Axiom;
            }
            return Derivation = derivation;
        }

        private int _numberOfSplits = -1;
        private int _numberOfSegments = -1;
        public int NumberOfSegments
        {
            get
            {
                if (_numberOfSegments == -1)
                {
                    _numberOfSegments = 0;
                    _numberOfSplits = 0;

                    var derivation = Derivation;
                    if (string.IsNullOrEmpty(derivation))
                    {
                        Derive();
                        derivation = Derivation;
                    }

                    for (int i = 0; i < derivation.Length; i++)
                    {
                        if (Axiom.Equals(derivation.Substring(i, 1)))
                        {
                            _numberOfSegments++;
                        }
                        else if ("[".Equals(derivation.Substring(i, 1)))
                        {
                            _numberOfSplits++;
                        }
                    }
                }
                return _numberOfSegments;
            }
        }

        public int NumberOfSplits
        {
            get
            {
                if (NumberOfSegments == -1)
                {
                    return _numberOfSplits;
                }
                return _numberOfSplits;
            }
        }

        private void Randomize()
        {
            for (int i = 0; i < _genom.Length; i++)
            {
                _genom[i] = _random.Next(_alphabet.Length);
            }
        }

        private void Crossover(Creature mother, Creature father)
        {
            int crossIndex = _random.Next(_genom.Length);
            for (int i = 0; i < crossIndex; i++)
            {
                _genom[i] = mother._genom[i];
            }
            for (int i = crossIndex; i < _genom.Length; i++)
            {
                _genom[i] = father._genom[i];
            }
            if (_random.Next(10) == 0)
            {
                _genom[_random.Next(_genom.Length)] = _random.Next(_alphabet.Length);
            }
        }

        private void Mutate(Creature mother, int n)
        {
            for (int i = 0; i < _genom.Length; i++)
            {
                _genom[i] = mother._genom[i];
            }
            for (int i = 0; i < n; i++)
            {
                _genom[_random.Next(_genom.Length)] = _random.Next(_alphabet.Length);
            }
        }
    }

    public class RandomWeight
    {
        private static System.Random _random = new System.Random();

        private float _step;
        private float _value = float.MinValue;

        public void Update()
        {
            if (_value != float.MinValue)
            {
                _value += _step;
            }
            else
            {
                _value = _random.Next(2001) / 1000f - 1f;
                _step = 0.0005f + (_random.Next(1001) / 1000000f);
                if (_random.Next(2) > 0)
                {
                    _step = -_step;
                }
            }
            if (_value < -1f)
            {
                _value = -1f;
                _step = 0.0005f + (_random.Next(1001) / 1000000f);
            }
            else if (_value > 1f)
            {
                _value = 1f;
                _step = -Math.Abs(_step);
            }
        }

        public float Weight
        {
            get
            {
                if (_value == float.MinValue)
                {
                    Update();
                }
                return _value;
            }
        }
    }
}