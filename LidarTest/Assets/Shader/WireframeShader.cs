// Used tutorials:
// https://github.com/Unity-Technologies/arfoundation-demos/tree/master/Assets/Shaders/Wireframe

using UnityEngine;

public class WireframeShader : MonoBehaviour
{
    private Mesh mesh;

    Color[] coords = new[]
    {
        new Color(1, 0, 0),
        new Color(0, 1, 0),
        new Color(0, 0, 1),
    };

    private void Update()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh != null)
        {
            SplitMesh(mesh);
            GenerateExisting(mesh);
        }
    }

    private void GenerateExisting(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] vertexColors = new Color32[vertices.Length];

        for (int i = 0; i < vertices.Length; i += 3)
        {
            vertexColors[i] = coords[0];
            if (i + 1 < vertices.Length)
                vertexColors[i + 1] = coords[1];
            if (i + 2 < vertices.Length)
                vertexColors[i + 2] = coords[2];
        }

        mesh.colors32 = vertexColors;
    }

    void SplitMesh(Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uvs = mesh.uv;

        Vector3[] newVerts;
        Vector3[] newNormals;
        Vector2[] newUvs;

        int n = triangles.Length;
        newVerts = new Vector3[n];
        newNormals = new Vector3[n];
        newUvs = new Vector2[n];

        for (int i = 0; i < n; i++)
        {
            newVerts[i] = verts[triangles[i]];
            newNormals[i] = normals[triangles[i]];
            if (uvs.Length > 0)
            {
                newUvs[i] = uvs[triangles[i]];
            }
            triangles[i] = i;
        }

        mesh.vertices = newVerts;
        mesh.normals = newNormals;
        mesh.uv = newUvs;
        mesh.triangles = triangles;
    }
}
