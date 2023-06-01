using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

namespace SS
{
    /// <summary>
    /// Example extension showing how to supply a mesh from native code.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "XR Meshing Subsystem Test",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Example feature extension showing how supply a mesh from native code.",
        DocumentationLink = Constants.k_DocumentationURL,
        OpenxrExtensionStrings = "",
        Version = "0.0.1",
        FeatureId = featureId)]
#endif
    public class XRMeshingSubsystemTestFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.prototyper.xr.meshing.subsystem.test";

        private static List<XRMeshSubsystemDescriptor> s_MeshDescriptors =
            new List<XRMeshSubsystemDescriptor>();

        /// <inheritdoc />
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(s_MeshDescriptors, "Sample Meshing");
        }

        /// <inheritdoc />
        protected override void OnSubsystemStart()
        {
            StartSubsystem<XRMeshSubsystem>();
        }

        /// <inheritdoc />
        protected override void OnSubsystemStop()
        {
            StopSubsystem<XRMeshSubsystem>();
        }

        /// <inheritdoc />
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRMeshSubsystem>();
        }
    }
}
