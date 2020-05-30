/*
GrowingWasteBase.cs - Base class for a game object of 'Growing Waste' for Arpoise.

This code is derived from the project "L-Systems in Unity" by Pedro Boechat,
see https://github.com/pboechat/LSystemsInUnity

ARPOISE - Augmented Reality Point Of Interest Service 

This file is part of Arpoise. Arpoise is free software.

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
Arpoise, see www.Arpoise.com/

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.arpoise.arpoiseapp.GrowingWaste
{
    public class GrowingWasteBase : MonoBehaviour
    {
        private volatile static bool _backendRunning;

        private static int _index;
        private static readonly List<GameObject> _trunkObjects = new List<GameObject>();
        private static readonly List<Material> _materials = new List<Material>();

        private string BaseName(string name)
        {
            var baseName = new StringBuilder();
            foreach (var c in name)
            {
                if (!char.IsNumber(c))
                {
                    baseName.Append(c);
                }
            }
            return baseName.ToString();
        }

        protected int GetIndex(GameObject trunkObject)
        {
            if (trunkObject != null)
            {
                var name = trunkObject.name;
                if (name == null)
                {
                    name = string.Empty;
                }
                name = BaseName(name);
                if (_trunkObjects.FirstOrDefault(x => name.Equals(BaseName(x.name))) == null)
                {
                    _trunkObjects.Add(trunkObject);
                    _materials.Add(trunkObject.GetComponent<MeshRenderer>().sharedMaterial);
                }
            }
            return _index++;
        }

        protected Material GetMaterial(int index)
        {
            return _materials[index % _materials.Count];
        }

        #region Genetic Algorithm

        protected void StartBackend()
        {
            if (!_backendRunning)
            {
                _backendRunning = true;
                StartCoroutine("RunBackend");
            }
        }

        protected bool KeepBackendAlive
        {
            set
            {
                if (value)
                {
                    KeepAlive = 1000;
                }
                else
                {
                    KeepAlive = -1;
                }
            }
        }

        private volatile int _keepAlive;
        private int KeepAlive
        {
            get { return _keepAlive; }
            set { _keepAlive = value; }
        }

        private static List<Creature> _myCreatures = new List<Creature>();
        private static List<Creature> _creatures = new List<Creature>();
        protected List<Creature> Creatures
        {
            get { return _creatures; }
            private set { _creatures = value; }
        }

        private long _backendLastUpdateMilliseconds;

        // A Coroutine for running the genetic algorithm in the background
        //
        protected IEnumerator RunBackend()
        {
            _myCreatures = new List<Creature>();

            for (int i = 0; i < 25; i++)
            {
                var creature = new Creature(_myCreatures.Count, null, null);
                _myCreatures.Add(creature);

                creature.FastDerive();
            }

            for (KeepAlive = 1000; KeepAlive > 0; KeepAlive -= 1)
            {
                var milliSeconds = DateTime.Now.Ticks / 10000;
                if (milliSeconds - _backendLastUpdateMilliseconds < 90)
                {
                    yield return new WaitForSeconds(.01f);
                }
                _backendLastUpdateMilliseconds = milliSeconds;

                #region Update
                {
                    _myCreatures.First().Update();
                    var creatures = _myCreatures.OrderByDescending(x => x.Fitness).ToList();

                    var victims = new List<Creature>(creatures.Where(x => x.IsOld));
                    for (int i = 0; victims.Count <= 5 && i < 5; i++)
                    {
                        victims.Add(creatures[(_myCreatures.Count - 1) - i]);
                    }
                    while (victims.Count > 5)
                    {
                        victims.RemoveAt(victims.Count - 1);
                    }

                    var mother1 = creatures[0];
                    var mother2 = creatures[1];
                    var father1 = creatures[1 + mother1.NextRandom(_myCreatures.Count - 1)];
                    var father2 = creatures[1 + mother1.NextRandom(_myCreatures.Count - 1)];
                    var father3 = creatures[1 + mother1.NextRandom(_myCreatures.Count - 1)];
                    if (father3.Index == mother2.Index)
                    {
                        father3 = mother1;
                    }

                    int index = 0;
                    {
                        var victim = victims[index];
                        var creature = new Creature(victim.Index, mother1, father1, victim.TrunkObject, victim.TrunkMaterial);
                        creature.FastDerive();
                        if (creatures.FirstOrDefault(x => x.Derivation == creature.Derivation) == null)
                        {
                            _myCreatures[creature.Index] = creature;
                        }
                    }
                    index++;
                    {
                        var victim = victims[index];
                        var creature = new Creature(victim.Index, mother1, father2, victim.TrunkObject, victim.TrunkMaterial);
                        creature.FastDerive();
                        if (creatures.FirstOrDefault(x => x.Derivation == creature.Derivation) == null)
                        {
                            _myCreatures[creature.Index] = creature;
                        }
                    }
                    index++;
                    {
                        var victim = victims[index];
                        var creature = new Creature(victim.Index, mother2, father3, victim.TrunkObject, victim.TrunkMaterial);
                        creature.FastDerive();
                        if (creatures.FirstOrDefault(x => x.Derivation == creature.Derivation) == null)
                        {
                            _myCreatures[creature.Index] = creature;
                        }
                    }
                    index++;
                    {
                        var victim = victims[index];
                        var creature = new Creature(victim.Index, mother1, 1, victim.TrunkObject, victim.TrunkMaterial);
                        creature.FastDerive();
                        if (creatures.FirstOrDefault(x => x.Derivation == creature.Derivation) == null)
                        {
                            _myCreatures[creature.Index] = creature;
                        }
                    }
                    index++;
                    {
                        var victim = victims[index];
                        var creature = new Creature(victim.Index, victim.TrunkObject, victim.TrunkMaterial);
                        creature.FastDerive();
                        if (creatures.FirstOrDefault(x => x.Derivation == creature.Derivation) == null)
                        {
                            _myCreatures[creature.Index] = creature;
                        }
                    }
                    Creatures = _myCreatures.OrderByDescending(x => x.Fitness).ToList();
                }
                #endregion
                yield return new WaitForSeconds(.01f);
            }
            _backendRunning = false;
            yield break;
        }
        #endregion

        #region L-System

        public interface IBuildCallback
        {
            void BuildFinished(GameObject trunk, Bounds bounds, Vector3 center);
        }

        private struct Turtle
        {
            public Quaternion Direction;
            public Vector3 Position;
            public Vector3 Step;
            public Vector3 HalfStep;
            public int Distance;

            public Turtle(Turtle other)
            {
                Direction = other.Direction;
                Position = other.Position;
                Step = other.Step;
                HalfStep = Step / 2;
                Distance = other.Distance;
            }

            public Turtle(Quaternion direction, Vector3 position, Vector3 step)
            {
                Direction = direction;
                Position = position;
                Step = step;
                HalfStep = step / 2;
                Distance = 0;
            }

            public float Factor
            {
                get
                {
                    var factor = 1f;
                    for (int i = 0; i < Distance; i++)
                    {
                        factor *= .8f; // plant parts further from the root are scaled down
                    }
                    return factor;
                }
            }

            public Vector3 Forward()
            {
                var factor = Factor;
                Distance++;
                return Position += Direction * (Step * factor);
            }

            public Vector3 Center()
            {
                return Position + Direction * (HalfStep * Factor);
            }

            public void RotateX(float angle)
            {
                Direction *= Quaternion.Euler(angle, 0, 0);
            }

            public void RotateY(float angle)
            {
                Direction *= Quaternion.Euler(0, angle, 0);
            }

            public void RotateZ(float angle)
            {
                Direction *= Quaternion.Euler(0, 0, angle);
            }
        }

        private static void CreateSegment(
            GameObject trunkObject,
            Material trunkMaterial,
            Turtle turtle,
            ref Mesh currentMesh,
            ref int chunkCount,
            GameObject trunk
            )
        {
            Mesh segment;
            if (trunkObject != null)
            {
                var filter = trunkObject.GetComponent<MeshFilter>();
                var mesh = filter.sharedMesh;
                if (mesh == null)
                {
                    mesh = filter.mesh;
                }
                segment = mesh;
            }
            else
            {
                throw new Exception("trunkObject must be given");
            }

            Vector3[] newVertices = segment.vertices;
            Vector3[] newNormals = segment.normals;
            Vector2[] newUVs = segment.uv;
            int[] newIndices = segment.triangles;

            if (currentMesh.vertices.Length + newVertices.Length > 65000)
            {
                CreateNewChunk(currentMesh, ref chunkCount, trunkMaterial, trunk);
                currentMesh = new Mesh();
            }

            int numVertices = currentMesh.vertices.Length + newVertices.Length;
            int numTriangles = currentMesh.triangles.Length + newIndices.Length;

            var vertices = new Vector3[numVertices];
            var normals = new Vector3[numVertices];
            int[] indices = new int[numTriangles];
            var uvs = new Vector2[numVertices];

            Array.Copy(currentMesh.vertices, 0, vertices, 0, currentMesh.vertices.Length);
            Array.Copy(currentMesh.normals, 0, normals, 0, currentMesh.normals.Length);
            Array.Copy(currentMesh.triangles, 0, indices, 0, currentMesh.triangles.Length);
            Array.Copy(currentMesh.uv, 0, uvs, 0, currentMesh.uv.Length);

            float factor = turtle.Factor;
            int offset = currentMesh.vertices.Length;
            for (int i = 0; i < newVertices.Length; i++)
            {
                var vertice = newVertices[i];
                if (factor < 1)
                {
                    vertice *= factor;
                }
                vertices[offset + i] = turtle.Position + (turtle.Direction * vertice);
            }

            int trianglesOffset = currentMesh.vertices.Length;
            offset = currentMesh.triangles.Length;
            for (int i = 0; i < newIndices.Length; i++)
            {
                indices[offset + i] = (trianglesOffset + newIndices[i]);
            }

            Array.Copy(newNormals, 0, normals, currentMesh.normals.Length, newNormals.Length);
            Array.Copy(newUVs, 0, uvs, currentMesh.uv.Length, newUVs.Length);

            currentMesh.vertices = vertices;
            currentMesh.normals = normals;
            currentMesh.triangles = indices;
            currentMesh.uv = uvs;
        }

        private static void CreateNewChunk(Mesh mesh, ref int count, Material trunkMaterial, GameObject trunk)
        {
            var chunk = new GameObject("Chunk " + (++count));
            chunk.transform.parent = trunk.transform;
            chunk.transform.localPosition = Vector3.zero;
            chunk.AddComponent<MeshRenderer>().material = trunkMaterial;
            chunk.AddComponent<MeshFilter>().mesh = mesh;
        }

        private static void Interpret(char letter, float angle, ref Stack<Turtle> stack, ref Turtle turtle, ref Vector3 min, ref Vector3 max, ref Vector3 center)
        {
            switch (letter)
            {
                case Creature.AxiomLetter:
                    center += turtle.Center();
                    var position = turtle.Forward();
                    min.x = Mathf.Min(min.x, position.x);
                    min.y = Mathf.Min(min.y, position.y);
                    min.z = Mathf.Min(min.z, position.z);
                    max.x = Mathf.Max(max.x, position.x);
                    max.y = Mathf.Max(max.y, position.y);
                    max.z = Mathf.Max(max.z, position.z);
                    break;
                case '+':
                    turtle.RotateZ(angle);
                    break;
                case '-':
                    turtle.RotateZ(-angle);
                    break;
                case '&':
                    turtle.RotateX(angle);
                    break;
                case '^':
                    turtle.RotateX(-angle);
                    break;
                case '\\':
                    turtle.RotateY(angle);
                    break;
                case '/':
                    turtle.RotateY(-angle);
                    break;
                case '|':
                    turtle.RotateZ(180);
                    break;
                case '[':
                    stack.Push(turtle);
                    turtle = new Turtle(turtle);
                    break;
                case ']':
                    turtle = stack.Pop();
                    break;
            }
        }

        public static void Interpret(
            GameObject trunkObject,
            float segmentHeight,
            Material trunkMaterial,
            float angle,
            string moduleString,
            out GameObject trunk,
            out Bounds bounds,
            out Vector3 center
        )
        {
            int n = 1;
            bounds = new Bounds();
            center = Vector3.zero;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            trunk = new GameObject("Trunk");
            int chunkCount = 0;
            var currentMesh = new Mesh();

            var turtle = new Turtle(Quaternion.identity, Vector3.zero, new Vector3(0, segmentHeight, 0));
            var stack = new Stack<Turtle>();

            foreach (var letter in moduleString)
            {
                if (Creature.AxiomLetter.Equals(letter))
                {
                    CreateSegment(
                            trunkObject,
                            trunkMaterial,
                            turtle,
                            ref currentMesh,
                            ref chunkCount,
                            trunk
                            );
                    n++;
                }
                Interpret(letter, angle, ref stack, ref turtle, ref min, ref max, ref center);
            }

            CreateNewChunk(currentMesh, ref chunkCount, trunkMaterial, trunk);
            bounds.SetMinMax(min, max);
            center /= n;
        }

        public static void Interpret(
            float segmentHeight,
            float angle,
            string moduleString,
            out Bounds bounds,
            out Vector3 center
            )
        {
            int n = 1;
            bounds = new Bounds();
            center = Vector3.zero;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var turtle = new Turtle(Quaternion.identity, Vector3.zero, new Vector3(0, segmentHeight, 0));
            var stack = new Stack<Turtle>();

            foreach (var letter in moduleString)
            {
                if (Creature.AxiomLetter.Equals(letter))
                {
                    n++;
                }
                Interpret(letter, angle, ref stack, ref turtle, ref min, ref max, ref center);
            }
            bounds.SetMinMax(min, max);
            center /= n;
        }

        public void Build(
            GameObject trunkObject,
            float segmentHeight,
            Material trunkMaterial,
            float angle,
            string moduleString,
            IBuildCallback callback
            )
        {
            _trunkObject = trunkObject;
            _segmentHeight = segmentHeight;
            _trunkMaterial = trunkMaterial;
            _angle = angle;
            _moduleString = moduleString;
            _callback = callback;
            StartCoroutine("Build");
        }

        private GameObject _trunkObject;
        private float _segmentHeight;
        private Material _trunkMaterial;
        private float _angle;
        private string _moduleString;
        private IBuildCallback _callback;

        // A Coroutine for running the genetic algorithm in the background
        //
        public IEnumerator Build()
        {
            int n = 1;
            var bounds = new Bounds();
            var center = Vector3.zero;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var trunk = new GameObject("Trunk");
            int chunkCount = 0;
            var currentMesh = new Mesh();

            var turtle = new Turtle(Quaternion.identity, Vector3.zero, new Vector3(0, _segmentHeight, 0));
            var stack = new Stack<Turtle>();

            foreach (var letter in _moduleString)
            {
                if (Creature.AxiomLetter.Equals(letter))
                {
                    CreateSegment(
                            _trunkObject,
                            _trunkMaterial,
                            turtle,
                            ref currentMesh,
                            ref chunkCount,
                            trunk
                            );
                    yield return null;
                    n++;
                }
                Interpret(letter, _angle, ref stack, ref turtle, ref min, ref max, ref center);
            }

            CreateNewChunk(currentMesh, ref chunkCount, _trunkMaterial, trunk);
            bounds.SetMinMax(min, max);
            center /= n;

            if (_callback != null)
            {
                _callback.BuildFinished(trunk, bounds, center);
            }
            yield break;
        }
        #endregion
    }
}
