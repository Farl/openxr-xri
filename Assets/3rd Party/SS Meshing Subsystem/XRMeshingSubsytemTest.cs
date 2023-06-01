using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using TMPro;

namespace SS
{
    // Create mesh from xr sensor data and add collider on it
    public class XRMeshingSubsytemTest : MonoBehaviour
    {
        public GameObject emptyMeshPrefab;
        public TMP_Text textMesh;
        public Transform target;

        [SerializeField] private bool shouldComputeNormals = true;

        private XRMeshSubsystem s_MeshSubsystem;
        private List<MeshInfo> s_MeshInfos = new List<MeshInfo>();

        private Dictionary<MeshId, GameObject> m_MeshIdToGo = new Dictionary<MeshId, GameObject>();

        private Dictionary<MeshId, MeshInfo> m_MeshesBeingGenerated = new Dictionary<MeshId, MeshInfo>();
        private Dictionary<MeshId, MeshInfo> m_MeshesNeedingGeneration = new Dictionary<MeshId, MeshInfo>();

        [Range(1, 10)]
        private int meshQueueSize = 1;

        // meshIdToGameObjectMap
        private Dictionary<MeshId, GameObject> meshIdToGameObjectMap = new Dictionary<MeshId, GameObject>();

        void Start()
        {
            var feature = OpenXRSettings.Instance.GetFeature<XRMeshingSubsystemTestFeature>();
            if (null == feature || feature.enabled == false)
            {
                enabled = false;
                Debug.LogError("XRMeshingSubsytemTestFeature is not enabled");
                return;
            }

            var meshSubsystems = new List<XRMeshSubsystem>();
            SubsystemManager.GetInstances(meshSubsystems);
            if (meshSubsystems.Count == 1)
            {
                s_MeshSubsystem = meshSubsystems[0];
                Debug.Log("MeshSubsystem is running");
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("Failed to initialize MeshSubsystem.\nTry reloading the Unity Editor");
#else
                Debug.LogError("Failed to initialize MeshSubsystem.");
#endif
                enabled = false;
            }
        }

        void AddToQueueIfNecessary(MeshInfo meshInfo)
        {
            if (m_MeshesNeedingGeneration.ContainsKey(meshInfo.MeshId))
                return;

            m_MeshesNeedingGeneration.Add(meshInfo.MeshId, meshInfo);
        }

        void RaiseMeshRemoved(MeshId meshId)
        {
            if (m_MeshesBeingGenerated.ContainsKey(meshId))
                m_MeshesBeingGenerated.Remove(meshId);
        }

        private MeshId GetNextMeshToGenerate()
        {
            // Get highest priority mesh first from m_MeshesNeedingGeneration
            MeshId meshId = default(MeshId);
            float highestPriority = float.MinValue;
            foreach (var pair in m_MeshesNeedingGeneration)
            {
                if (pair.Value.PriorityHint > highestPriority)
                {
                    meshId = pair.Key;
                    highestPriority = pair.Value.PriorityHint;
                }
            }
            return meshId;
        }

        GameObject GetOrCreateGameObjectForMesh(MeshId meshId)
        {
            GameObject meshGameObject;
            if (!meshIdToGameObjectMap.TryGetValue(meshId, out meshGameObject))
            {
                meshGameObject = Instantiate(emptyMeshPrefab, target, false);
                meshIdToGameObjectMap.Add(meshId, meshGameObject);
            }

            return meshGameObject;
        }

        void UpdateExample()
        {
            meshQueueSize = Mathf.Clamp(meshQueueSize, 1, meshQueueSize);

            if (s_MeshSubsystem.TryGetMeshInfos(s_MeshInfos))
            {
                foreach (var meshInfo in s_MeshInfos)
                {
                    switch (meshInfo.ChangeState)
                    {
                        case MeshChangeState.Added:
                        case MeshChangeState.Updated:
                            AddToQueueIfNecessary(meshInfo);
                            break;

                        case MeshChangeState.Removed:
                            RaiseMeshRemoved(meshInfo.MeshId);

                            // Remove from processing queue
                            m_MeshesNeedingGeneration.Remove(meshInfo.MeshId);

                            // Destroy the GameObject
                            GameObject meshGameObject;
                            if (meshIdToGameObjectMap.TryGetValue(meshInfo.MeshId, out meshGameObject))
                            {
                                Destroy(meshGameObject);
                                meshIdToGameObjectMap.Remove(meshInfo.MeshId);
                            }

                            break;

                        default:
                            break;
                    }
                }
            }

            // ...

            while (m_MeshesBeingGenerated.Count < meshQueueSize && m_MeshesNeedingGeneration.Count > 0)
            {
                // Get the next mesh to generate. Could be based on the mesh's
                // priorityHint, whether it is new vs updated, etc.
                var meshId = GetNextMeshToGenerate();

                // Gather the necessary Unity objects for the generation request
                var meshGameObject = GetOrCreateGameObjectForMesh(meshId);
                var meshCollider = meshGameObject.GetComponent<MeshCollider>();
                var mesh = meshGameObject.GetComponent<MeshFilter>().mesh;
                var meshAttributes = shouldComputeNormals ? MeshVertexAttributes.Normals : MeshVertexAttributes.None;

                // Request generation
                s_MeshSubsystem.GenerateMeshAsync(meshId, mesh, meshCollider, meshAttributes, OnMeshGenerated);

                // Update internal state
                m_MeshesBeingGenerated.Add(meshId, m_MeshesNeedingGeneration[meshId]);
                m_MeshesNeedingGeneration.Remove(meshId);
            }
        }

        void Update()
        {
            UpdateExample();
        }


        void OnMeshGenerated(MeshGenerationResult result)
        {
            if (result.Status != MeshGenerationStatus.Success)
            {
                // Handle error, regenerate, etc.
                if (textMesh)
                {
                    Debug.LogWarning(result.MeshId + " failed to generate.");
                }
            }

            m_MeshesBeingGenerated.Remove(result.MeshId);
        }
    }

}