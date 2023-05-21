using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MultiNavMeshVisualizer : MonoBehaviour
{
    public bool ShowVisualization = true;

    [SerializeField]
    private NavMeshSurface[] Surfaces;
    [SerializeField]
    private Vector3 GeneratedMeshOffset = new(0, 0.05f, 0);

    private GameObject MeshVisualization;

    private void Awake()
    {
        MeshVisualization = new("NavMesh Visualization");

        foreach (NavMeshSurface surface in Surfaces)
        {
            NavMesh.RemoveAllNavMeshData();
            surface.BuildNavMesh();

            // Note: As of at least May 21 2023, Triangulations calculated with the NavMeshSurface do not properly
            // generate the "areas" property of the NavMeshTriangulation. Meaning there will only ever be areas
            // of index 0 provided if you are not using the built-in baking system (which is much less flexible).
            // The class will still generate the mesh correctly, but you will not get different colors per AREA
            // like you will with the built-in system.
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

            GameObject visualization = new($"Visualization for Agent {surface.agentTypeID}");

            Dictionary<int, List<int>> areaIndices = new();

            for (int i = 0; i < triangulation.areas.Length; i++)
            {
                if (!areaIndices.ContainsKey(triangulation.areas[i]))
                {
                    areaIndices.Add(triangulation.areas[i], new());
                }

                areaIndices[triangulation.areas[i]].Add(triangulation.indices[3 * i]);
                areaIndices[triangulation.areas[i]].Add(triangulation.indices[3 * i + 1]);
                areaIndices[triangulation.areas[i]].Add(triangulation.indices[3 * i + 2]);
            }

            MeshRenderer renderer = visualization.AddComponent<MeshRenderer>();
            MeshFilter filter = visualization.AddComponent<MeshFilter>();
            Mesh navMesh = new Mesh();

            navMesh.subMeshCount = areaIndices.Count;
            Material[] materials = new Material[areaIndices.Count];
            navMesh.SetVertices(triangulation.vertices);

            int index = 0;
            foreach(KeyValuePair<int, List<int>> keyValuePair in areaIndices)
            {
                navMesh.SetIndices(keyValuePair.Value, MeshTopology.Triangles, index);

                Material material = new(Shader.Find("Universal Render Pipeline/Lit"));
                Color randomColor = new(
                    Random.Range(0, 1f),
                    Random.Range(0, 1f),
                    Random.Range(0, 1f),
                    0.4f
                );
                material.color = randomColor;

                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_Surface", 1);

                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                material.SetShaderPassEnabled("DepthOnly", false);
                material.SetShaderPassEnabled("SHADOWCASTER", false);

                material.SetOverrideTag("RenderType", "Transparent");

                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                materials[index] = material;

                index++;
            }

            renderer.sharedMaterials = materials;
            filter.mesh = navMesh;

            visualization.transform.SetParent(MeshVisualization.transform, false);
        }

        foreach (NavMeshSurface surface in Surfaces)
        {
            surface.BuildNavMesh();
        }
    }

    private void Update()
    {
        MeshVisualization.SetActive(ShowVisualization);
        MeshVisualization.transform.position = GeneratedMeshOffset;
    }
}
