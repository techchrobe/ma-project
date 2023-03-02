using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

    public int xSize, ySize;
    public bool newMesh = false;

    private Mesh mesh;
    private Vector3[] vertices;

    private void Update() {
        if (newMesh)
            Generate();
        else
        {
            SplitMesh();
            GenerateExisting();
        }
    }

    private void GenerateExisting() {
        mesh = GetComponent<MeshFilter>().mesh;
        if(mesh != null)
        {
            Color[] coords = new[]
            {
                new Color(1, 0, 0),
                new Color(0, 1, 0),
                new Color(0, 0, 1),
            };

            vertices = mesh.vertices;
            Color32[] vertexColors = new Color32[vertices.Length];

            //vertexColors[0] = coords[1];
            //vertexColors[1] = coords[0];
            //vertexColors[2] = coords[0];
            //vertexColors[3] = coords[2];

            for (int i = 0; i < vertices.Length; i += 3)
            {
                vertexColors[i] = coords[0];
                if (i + 1 < vertices.Length)
                    vertexColors[i + 1] = coords[1];
                if (i + 2 < vertices.Length)
                    vertexColors[i + 2] = coords[2];
            }

            mesh.colors32 = vertexColors;
            mesh.RecalculateNormals();
        }
    }

    void SplitMesh()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh != null)
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

    [ContextMenu("generate")]
    private void Generate() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        Color[] coords = new[]
        {
            new Color(1, 0, 0),
            new Color(0, 1, 0),
            new Color(0, 0, 1),
        };

        vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        Color32[] vertexColors = new Color32[vertices.Length];
        for(int i = 0, y = 0; y <= ySize; y++) {
            for(int x = 0; x <= xSize; x++, i++) {
                vertices[i] = new Vector3(x * 5, y * 5);
                vertexColors[i] = coords[(int)Mathf.Repeat(x - y, 3)];
                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                tangents[i] = tangent;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors32 = vertexColors;
        mesh.tangents = tangents;

        int[] triangles = new int[xSize * ySize * 6];
        for(int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
            for(int x = 0; x < xSize; x++, ti += 6, vi++) {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
