using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

    public int xSize, ySize;

    private Mesh mesh;
    private Vector3[] vertices;

    private void Awake() {
        Generate();
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
