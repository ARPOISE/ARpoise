/*
ArCreature.cs - MonoBehaviour for a Lindenmayer System creature in ARpoise.

This code is derived from the project "L-Systems in Unity" by Pedro Boechat,
see https://github.com/pboechat/LSystemsInUnity

ARpoise - Augmented Reality point of interest service environment 

This file is part of ARpoise. ARpoise is free software.

MIT License

Copyright (c) 2020 Pedro Boechat, Tamiko Thiel and Peter Graf

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

*/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArCreature : MonoBehaviour
    {
        public const string Axiom = "F";
        public const char AxiomLetter = 'F';
        private float _factor = 0.8f;
        private int _maxDistance;

        private struct Turtle
        {
            public Quaternion Direction;
            public Vector3 Position;
            public Transform ParentTransform;
            public int MaxDistance;
            private int _distance;
            private readonly float _factor;
            private readonly Vector3 _step;

            public Turtle(Turtle other)
            {
                Direction = other.Direction;
                Position = other.Position;
                ParentTransform = other.ParentTransform;
                _step = other._step;
                _factor = other._factor;
                _distance = other._distance;
                MaxDistance = 0;
            }

            public Turtle(Quaternion direction, Vector3 position, Vector3 step, float factor, Transform parentTransform)
            {
                Direction = direction;
                Position = position;
                ParentTransform = parentTransform;
                _step = step;
                _factor = factor;
                _distance = MaxDistance = 0;
            }

            public float DistanceFactor
            {
                get
                {
                    var factor = 1f;
                    for (int i = 0; i < _distance; i++)
                    {
                        factor *= _factor;
                    }
                    return factor;
                }
            }

            public void Forward()
            {
                Position += Direction * (_step * DistanceFactor);
                MaxDistance = Math.Max(++_distance, MaxDistance);
            }

            public void RotateX(float angle) { Direction *= Quaternion.Euler(angle, 0, 0); }
            public void RotateY(float angle) { Direction *= Quaternion.Euler(0, angle, 0); }
            public void RotateZ(float angle) { Direction *= Quaternion.Euler(0, 0, angle); }
        }

        private static string Derive(int nDerivations, string lindenmayerString)
        {
            var derived = Axiom;
            var sb = new StringBuilder();
            int n = Math.Max(1, nDerivations);
            while (n-- > 0)
            {
                foreach (var letter in derived)
                {
                    if (AxiomLetter == letter)
                    {
                        sb.Append(lindenmayerString);
                    }
                    else
                    {
                        sb.Append(letter);
                    }
                }
                derived = sb.ToString();
                sb.Clear();
            }
            return derived;
        }

        private static Turtle Interpret(char letter, float angle, Stack<Turtle> stack, Turtle turtle)
        {
            switch (letter)
            {
                case AxiomLetter: turtle.Forward(); break;
                case '[': stack.Push(turtle); turtle = new Turtle(turtle); break;
                case ']': turtle = stack.Pop(); break;
                case '+': turtle.RotateZ(angle); break;
                case '-': turtle.RotateZ(-angle); break;
                case '&': turtle.RotateX(angle); break;
                case '^': turtle.RotateX(-angle); break;
                case '/': turtle.RotateY(-angle); break;
                case '\\': turtle.RotateY(angle); break;
            }
            return turtle;
        }

        private static GameObject Add(GameObject objectToAdd, Turtle turtle)
        {
            var distanceFactor = turtle.DistanceFactor;
            var newObject = Instantiate(objectToAdd);
            newObject.transform.localScale = new Vector3(distanceFactor, distanceFactor, distanceFactor);
            newObject.transform.localRotation = turtle.Direction;
            newObject.transform.localPosition = turtle.Position;
            newObject.transform.parent = turtle.ParentTransform;
            return newObject;
        }

        public static GameObject Create(int nDerivations, string lindenmayerString, GameObject wrapper, GameObject trunk, GameObject leaf, float angle, float factor, Transform parentTransform)
        {
            var creature = Instantiate(wrapper);
            creature.transform.parent = parentTransform;
            var arCreature = creature.AddComponent(typeof(ArCreature)) as ArCreature;
            arCreature._factor = factor;

            var turtle = new Turtle(Quaternion.identity, Vector3.zero, Vector3.up, factor, creature.transform);
            var stack = new Stack<Turtle>();
            var derived = Derive(nDerivations, lindenmayerString);
            foreach (var letter in derived)
            {
                if (AxiomLetter == letter)
                {
                    var newObject = Add(trunk, turtle);
                    turtle.ParentTransform = newObject.transform;
                }
                else if (']' == letter && leaf != null)
                {
                    Add(leaf, turtle);
                    turtle.Forward();
                    if (arCreature != null && turtle.MaxDistance > arCreature._maxDistance)
                    {
                        arCreature._maxDistance = turtle.MaxDistance;
                    }
                }
                turtle = Interpret(letter, angle, stack, turtle);
                if (arCreature != null && turtle.MaxDistance > arCreature._maxDistance)
                {
                    arCreature._maxDistance = turtle.MaxDistance;
                }
            }
            return creature;
        }

        private void Grow(Transform transform, int level, float size, float factor)
        {
            Vector3 factorVector;
            if (level > 0)
            {
                factorVector = new Vector3(factor, factor, factor);
                for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
                {
                    var childTransform = transform.GetChild(childIndex).transform;
                    if (childTransform.localScale.x != factor)
                    {
                        childTransform.localScale = factorVector;
                    }
                    Grow(childTransform, level - 1, size, _factor);
                }
                return;
            }

            var sizedFactor = size * factor;
            factorVector = new Vector3(sizedFactor, sizedFactor, sizedFactor);

            for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                var childTransform = transform.GetChild(childIndex).transform;
                if (childTransform.localScale.x != sizedFactor)
                {
                    childTransform.localScale = factorVector;
                    for (int grandChildIndex = 0; grandChildIndex < childTransform.childCount; grandChildIndex++)
                    {
                        var grandChildTransform = childTransform.GetChild(grandChildIndex).transform;
                        if (grandChildTransform.localScale.x != 0)
                        {
                            grandChildTransform.localScale = Vector3.zero;
                        }
                    }
                }
            }
        }

        public void Grow(float growth)
        {
            int levelToGrow = -1;
            if (_maxDistance > 1 && growth > 0)
            {
                var x = 1f / _maxDistance;
                levelToGrow = (int)(growth / x);
                growth -= levelToGrow * x;
                growth *= _maxDistance;
            }
            Grow(gameObject.transform, levelToGrow, growth, 1f);
        }
    }
}
