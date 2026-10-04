using UnityEngine;

/// <summary>
/// Generates a cube-sphere mesh by projecting a subdivided cube onto a sphere surface.
/// Each of the 6 cube faces becomes a grid of (resolution+1)² vertices.
/// </summary>
public static class SphereCreator
{
    private struct Face
    {
        public Vector3 localUp;
        public Vector3 axisA;
        public Vector3 axisB;
    }

    public static void CreateSphereMesh(int resolution, float radius,
        out Vector3[] vertices, out int[] triangles, out Vector2[] uvs)
    {
        int vertsPerFace = (resolution + 1) * (resolution + 1);
        int trisPerFace = resolution * resolution * 2;

        vertices = new Vector3[vertsPerFace * 6];
        uvs = new Vector2[vertsPerFace * 6];
        triangles = new int[trisPerFace * 3 * 6];

        // Each face is defined so Cross(axisA, axisB) == localUp,
        // giving consistent outward-facing normals across all 6 faces.
        Face[] faces = new Face[]
        {
            new Face { localUp = Vector3.up,      axisA = Vector3.forward, axisB = Vector3.right },
            new Face { localUp = Vector3.down,    axisA = Vector3.back,    axisB = Vector3.right },
            new Face { localUp = Vector3.left,    axisA = Vector3.forward, axisB = Vector3.up    },
            new Face { localUp = Vector3.right,   axisA = Vector3.back,    axisB = Vector3.up    },
            new Face { localUp = Vector3.forward, axisA = Vector3.right,   axisB = Vector3.up    },
            new Face { localUp = Vector3.back,    axisA = Vector3.left,    axisB = Vector3.up    },
        };

        int vertexIndex = 0;
        int triangleIndex = 0;

        foreach (Face face in faces)
            CreateFace(face, resolution, radius, vertices, triangles, uvs, ref vertexIndex, ref triangleIndex);
    }

    private static void CreateFace(Face face, int resolution, float radius,
        Vector3[] vertices, int[] triangles, Vector2[] uvs,
        ref int vertexIndex, ref int triangleIndex)
    {
        int faceVertexStart = vertexIndex;

        for (int y = 0; y <= resolution; y++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                Vector2 percent = new Vector2(x, y) / resolution;

                Vector3 pointOnCube = face.localUp
                    + (percent.x - 0.5f) * 2f * face.axisA
                    + (percent.y - 0.5f) * 2f * face.axisB;

                vertices[vertexIndex] = pointOnCube.normalized * radius;
                uvs[vertexIndex] = percent;
                vertexIndex++;
            }
        }

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = faceVertexStart + y * (resolution + 1) + x;

                // First triangle (BL, BR, TL)
                triangles[triangleIndex] = i;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i + resolution + 1;

                // Second triangle (BR, TR, TL)
                triangles[triangleIndex + 3] = i + 1;
                triangles[triangleIndex + 4] = i + resolution + 2;
                triangles[triangleIndex + 5] = i + resolution + 1;

                triangleIndex += 6;
            }
        }
    }
}