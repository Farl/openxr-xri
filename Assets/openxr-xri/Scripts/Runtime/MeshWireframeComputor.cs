using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// Reference: https://github.com/MinaPecheux/unity-tutorials
// main/Assets/00-Shaders/CrossPlatformWireframe/Scripts/MeshWireframeComputor.cs
[RequireComponent(typeof(MeshFilter))]
public class MeshWireframeComputor : MonoBehaviour
{
    public enum RedundancySolution
    {
        None,
        PickAgain,
        Color4,
        Random,
    }

    private static Color[] _COLORS = new Color[] {
        new Color(1, 0, 0, 0),
        new Color(0, 1, 0, 0),
        new Color(0, 0, 1, 0),
        new Color(0, 0, 0, 1),
    };

    [SerializeField] private RedundancySolution _redundancySolution = RedundancySolution.None;

    // Calculate the wireframe of a mesh and write it into vertex colors
    [ContextMenu("Update Mesh")]
    public void UpdateMesh()
    {
        if (!gameObject.activeSelf || !GetComponent<MeshRenderer>().enabled)
            return;

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
            return;

        // Compute and store vertex colors for the
        // wireframe shader
        Color[] colors = _SortedColoring(mesh);

        if (colors != null)
        {
            mesh.SetColors(colors);
        }
    }

    // https://tech.metail.com/colouring-graphs-for-a-wireframe-shader/
    private Color[] _SortedColoring(Mesh mesh)
    {
        int n = mesh.vertexCount;
        int[] labels = new int[n];

        // Each int[] represents a triangle, with the indices of its vertices
        List<int[]> triangles = _GetSortedTriangles(mesh.triangles);

        // Sort the triangles by their vertices
        triangles.Sort((int[] t1, int[] t2) =>
        {
            int i = 0;
            while (i < t1.Length && i < t2.Length)
            {
                if (t1[i] < t2[i]) return -1;
                if (t1[i] > t2[i]) return 1;
                i += 1;
            }
            if (t1.Length < t2.Length) return -1;
            if (t1.Length > t2.Length) return 1;
            return 0;
        });

        foreach (int[] triangle in triangles)
        {
            HashSet<int> availableLabels = new HashSet<int>() { 1, 2, 3 };
            foreach (int vertexIndex in triangle)
            {
                if (labels[vertexIndex] > 0)
                {
                    if (availableLabels.Contains(labels[vertexIndex]))
                    {
                        availableLabels.Remove(labels[vertexIndex]);
                    }
                    else
                    {
                        // Color already used by another vertex
                        switch(_redundancySolution)
                        {
                            default:
                            case RedundancySolution.None:
                                // Do nothing
                                break;
                            case RedundancySolution.Color4:
                                labels[vertexIndex] = 4;
                                break;
                            case RedundancySolution.Random:
                                // Pick a random color
                                labels[vertexIndex] = Random.Range(1, 4);
                                break;
                            case RedundancySolution.PickAgain:
                                // Pick again
                                labels[vertexIndex] = 0;
                                break;

                        }
                    }
                }
            }
            foreach (int vertexIndex in triangle)
            {
                if (labels[vertexIndex] == 0)
                {
                    if (availableLabels.Count == 0)
                    {
                        Debug.LogError("Could not find color");
                        return null;
                    }
                    labels[vertexIndex] = availableLabels.First();
                    availableLabels.Remove(labels[vertexIndex]);
                }
            }
        }

        Color[] colors = new Color[n];
        for (int i = 0; i < n; i++)
        {
            colors[i] = labels[i] > 0 ? _COLORS[labels[i] - 1] : _COLORS[3];
        }
        return colors;
    }

    // Get the sorted triangles of a mesh by sorting the vertices of each triangle
    private List<int[]> _GetSortedTriangles(int[] triangles)
    {
        List<int[]> result = new List<int[]>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            List<int> t = new List<int> { triangles[i], triangles[i + 1], triangles[i + 2] };
            t.Sort();
            result.Add(t.ToArray());
        }
        return result;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateMesh();
    }
#endif
}
