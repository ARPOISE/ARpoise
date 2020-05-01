/*
LSystem.cs - the L-System used for the creatures of 'Growing Waste' for Arpoise.

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

using UnityEngine;
using System;
using System.Collections.Generic;

namespace com.arpoise.arpoiseapp.GrowingWaste
{
    public static class LSystem
    {
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
            GameObject gameObject,
            Material trunkMaterial,
            Turtle turtle,
            ref Mesh currentMesh,
            ref int chunkCount,
            GameObject trunk
            )
        {
            Mesh segment;
            if (gameObject != null)
            {
                var filter = gameObject.GetComponent<MeshFilter>();
                var mesh = filter.sharedMesh;
                if (mesh == null)
                {
                    mesh = filter.mesh;
                }
                segment = mesh;
            }
            else
            {
                throw new Exception("gameObject must be given");
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

        public static void Interpret(
            GameObject gameObject,
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

            var current = new Turtle(Quaternion.identity, Vector3.zero, new Vector3(0, segmentHeight, 0));
            var stack = new Stack<Turtle>();

            foreach (var module in moduleString)
            {
                switch (module)
                {
                    case 'F':
                        CreateSegment(
                            gameObject,
                            trunkMaterial,
                            current,
                            ref currentMesh,
                            ref chunkCount,
                            trunk
                            );
                        n++;
                        center += current.Center();
                        var position = current.Forward();
                        min.x = Mathf.Min(min.x, position.x);
                        min.y = Mathf.Min(min.y, position.y);
                        min.z = Mathf.Min(min.z, position.z);
                        max.x = Mathf.Max(max.x, position.x);
                        max.y = Mathf.Max(max.y, position.y);
                        max.z = Mathf.Max(max.z, position.z);
                        break;
                    case '+':
                        current.RotateZ(angle);
                        break;
                    case '-':
                        current.RotateZ(-angle);
                        break;
                    case '&':
                        current.RotateX(angle);
                        break;
                    case '^':
                        current.RotateX(-angle);
                        break;
                    case '\\':
                        current.RotateY(angle);
                        break;
                    case '/':
                        current.RotateY(-angle);
                        break;
                    case '|':
                        current.RotateZ(180);
                        break;
                    case '[':
                        stack.Push(current);
                        current = new Turtle(current);
                        break;
                    case ']':
                        current = stack.Pop();
                        break;

                }
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

            var current = new Turtle(Quaternion.identity, Vector3.zero, new Vector3(0, segmentHeight, 0));
            var stack = new Stack<Turtle>();

            foreach (var module in moduleString)
            {
                switch (module)
                {
                    case 'F':
                        n++;
                        center += current.Center();
                        var position = current.Forward();
                        min.x = Mathf.Min(min.x, position.x);
                        min.y = Mathf.Min(min.y, position.y);
                        min.z = Mathf.Min(min.z, position.z);
                        max.x = Mathf.Max(max.x, position.x);
                        max.y = Mathf.Max(max.y, position.y);
                        max.z = Mathf.Max(max.z, position.z);
                        break;
                    case '+':
                        current.RotateZ(angle);
                        break;
                    case '-':
                        current.RotateZ(-angle);
                        break;
                    case '&':
                        current.RotateX(angle);
                        break;
                    case '^':
                        current.RotateX(-angle);
                        break;
                    case '\\':
                        current.RotateY(angle);
                        break;
                    case '/':
                        current.RotateY(-angle);
                        break;
                    case '|':
                        current.RotateZ(180);
                        break;
                    case '[':
                        stack.Push(current);
                        current = new Turtle(current);
                        break;
                    case ']':
                        current = stack.Pop();
                        break;
                }
            }
            bounds.SetMinMax(min, max);
            center /= n;
        }
    }
}
