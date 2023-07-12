using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_VIVE_WAVE && USE_VIVE_WAVE_TOOLKIT
using Wave.OpenXR.Toolkit.CompositionLayer.Passthrough;
using Wave.OpenXR.CompositionLayer;
using Wave.OpenXR.CompositionLayer.Passthrough;
#endif

namespace SS
{
    public class ViveHelper : MonoBehaviour
    {
        #region Enums & Classes
        enum PassthroughType
        {
            None = 0,
            Planar,
            Projected
        }

#if !USE_VIVE_WAVE
    enum LayerType
    {
        Overlay = 1,
        Underlay = 2
    }
#endif

        #endregion

        #region Inspector
        [SerializeField] PassthroughType PassthroughMethod;
        [SerializeField] LayerType layerType = LayerType.Underlay;
        int ID;
        [SerializeField] Mesh UsingMesh;
        [SerializeField] Transform Trans;
        [Tooltip("Cam should be the transform of the the root of the camera rig in world space")]
        [SerializeField] Transform Cam;
        #endregion

        #region Private & Protected

        private Coroutine updateCoroutine;

        void Awake()
        {

        }

        void OnDestroy()
        {

        }

        void OnEnable()
        {
            EnablePassthrough(true);
        }

        void OnDisable()
        {
            EnablePassthrough(false);
        }

        void EnablePassthrough(bool enable)
        {
            if (enable)
            {
                if (PassthroughMethod == PassthroughType.None)
                    return;

                // Check if already enabled
                if (ID != 0)
                {
                    EnablePassthrough(false);
                }

#if USE_VIVE_WAVE
                // Enable passthrough
                switch (PassthroughMethod)
                {
                    case PassthroughType.Planar:
                        ID = CompositionLayerPassthroughAPI.CreatePlanarPassthrough(layerType);
                        break;


                    case PassthroughType.Projected:

                        if (UsingMesh != null)
                        {
                            ID = CompositionLayerPassthroughAPI.CreateProjectedPassthrough(layerType);

                            // Use the SetProjectedPassthroughMesh() function to set the mesh (shape) of the passthrough.
                            int[] indices = new int[UsingMesh.triangles.Length];
                            for (int i = 0; i < UsingMesh.triangles.Length; i++)
                            {
                                indices[i] = UsingMesh.triangles[i];
                            }
                            CompositionLayerPassthroughAPI.SetProjectedPassthroughMesh(ID, UsingMesh.vertices, indices, true);

                            // and use the SetProjectedPassthroughMeshTranform() function to set the transform (position, rotation, and scale) of the passthrough.
                            UpdatePosition();
                        }
                        StartCoroutine(UpdateCoroutine());
                        break;
                }
#else
            Debug.LogError("Missing Vive Wave Plugin");
#endif
            }
            else
            {
                // Disable passthrough
#if USE_VIVE_WAVE
                if (ID != 0)
                    CompositionLayerPassthroughAPI.DestroyPassthrough(ID);
#endif
                ID = 0;

                if (updateCoroutine != null)
                {
                    StopCoroutine(updateCoroutine);
                    updateCoroutine = null;
                }
            }
        }

        IEnumerator UpdateCoroutine()
        {
            while (PassthroughMethod == PassthroughType.Projected && Cam != null && Trans != null)
            {
                UpdatePosition();
                yield return null;
            }
            updateCoroutine = null;
        }

        void UpdatePosition()
        {
            if (Cam == null || Trans == null)
                return;
            if (PassthroughMethod != PassthroughType.Projected)
                return;
            if (ID == 0)
                return;
#if USE_VIVE_WAVE
            CompositionLayerPassthroughAPI.SetProjectedPassthroughMeshTransform(ID, ProjectedPassthroughSpaceType.Worldlock, Cam.InverseTransformDirection(Trans.position), Quaternion.Inverse(Cam.rotation) * Trans.rotation, Trans.lossyScale);
#endif
        }
        #endregion
    }

}