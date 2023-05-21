using UnityEngine;
using UnityEngine.AI;

public class NavMeshVisualizer : MonoBehaviour
{
    public bool ShowVisualization = true;

    [SerializeField]
    private Material VisualizationMaterial;
    [SerializeField]
    private Vector3 GeneratedMeshOffset = new(0, 0.05f, 0);

    private GameObject MeshVisualization;

    private void Start()
    {
        MeshVisualization = new("NavMesh Visualization");
        MeshRenderer renderer = MeshVisualization.AddComponent<MeshRenderer>();
        MeshFilter filter = MeshVisualization.AddComponent<MeshFilter>();
        Mesh navMesh = new Mesh();

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        navMesh.SetVertices(triangulation.vertices);
        navMesh.SetIndices(triangulation.indices, MeshTopology.Triangles, 0);

        renderer.sharedMaterial = VisualizationMaterial;
        filter.mesh = navMesh;
    }

    private void Update()
    {
        MeshVisualization.SetActive(ShowVisualization);
        MeshVisualization.transform.position = GeneratedMeshOffset;
    }
}
