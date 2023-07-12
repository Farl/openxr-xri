using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

namespace SS
{
    public class XRHeadBase : MonoBehaviour, IXRHead
    {
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private InputActionReference recenterAction;
        [SerializeField] private float resetTime = 1f;
        [SerializeField] private Transform fixTransform;
        [SerializeField] private Transform resetTransform;

        private static XRHeadBase Instance;
        private static Vector3 fixLocalPos = Vector3.zero;
        private static Vector3 fixLocalEulerAngles = Vector3.zero;

        public bool IsRecenterPressed { get; set; } = false;

        private float resetTimer = 1f;

        public static void Recenter()
        {
            if (Instance == null)
                return;
            Instance.RecenterInternal();
        }

        public static void ClearFixTransform()
        {
            fixLocalPos = Vector3.zero;
            fixLocalEulerAngles = Vector3.zero;
        }

        private void RecenterInternal()
        {
            if (xrOrigin == null)
                xrOrigin = GetComponentInParent<XROrigin>();

            if (xrOrigin == null || resetTransform == null || fixTransform == null)
                return;

            var xrOriginTrans = xrOrigin.transform;

            var camTrans = xrOrigin.Camera.transform;

            var camOffsetTrans = xrOrigin.CameraFloorOffsetObject.transform;

            var diffAngle = resetTransform.eulerAngles.y - camTrans.eulerAngles.y;
            fixTransform.Rotate(0, diffAngle, 0);

            var diffPos = resetTransform.position - camTrans.position;
            fixTransform.position += diffPos;

            fixLocalPos = fixTransform.localPosition;
            fixLocalEulerAngles = fixTransform.localEulerAngles;

            var camForward = camTrans.forward;
            camForward.y = 0;
            var camOffsetForward = camOffsetTrans.forward;
            camOffsetForward.y = 0;

            //xrOriginTrans.rotation = Quaternion.FromToRotation(camForward, camOffsetForward) * xrOriginTrans.rotation;
        }

        public static void XRSubSystemRecenter()
        {

            var xrSettings = XRGeneralSettings.Instance;
            if (xrSettings == null)
            {
                Debug.Log($"XRGeneralSettings is null.");
                return;
            }

            var xrManager = xrSettings.Manager;
            if (xrManager == null)
            {
                Debug.Log($"XRManagerSettings is null.");
                return;
            }

            var xrLoader = xrManager.activeLoader;
            if (xrLoader == null)
            {
                Debug.Log($"XRLoader is null.");
                return;
            }

            Debug.Log($"Loaded XR Device: {xrLoader.name}");

            var xrDisplay = xrLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
            Debug.Log($"XRDisplay: {xrDisplay != null}");

            if (xrDisplay != null)
            {
                if (xrDisplay.TryGetDisplayRefreshRate(out float refreshRate))
                {
                    Debug.Log($"Refresh Rate: {refreshRate}hz");
                }
            }

            var xrInput = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();
            Debug.Log($"XRInput: {xrInput != null}");

            if (xrInput != null)
            {
                xrInput.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
                xrInput.TryRecenter();
            }

            /*
            var subSystems = new List<UnityEngine.XR.XRInputSubsystem>();
            SubsystemManager.GetInstances<UnityEngine.XR.XRInputSubsystem>(subSystems);
            if (subSystems != null)
            {
                foreach (var subSystem in subSystems)
                {
                    subSystem.TryRecenter();
                }
            }
            */
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            if (fixTransform)
            {
                fixTransform.localPosition = fixLocalPos;
                fixTransform.localEulerAngles = fixLocalEulerAngles;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (recenterAction != null && recenterAction.action != null)
            {
                var prevIsRecenterPressed = IsRecenterPressed;
                IsRecenterPressed = recenterAction.action.IsPressed();
                if (prevIsRecenterPressed != IsRecenterPressed)
                {
                    if (IsRecenterPressed)
                    {
                        resetTimer = resetTime;
                    }
                    else
                    {
                    }
                }
                else if (IsRecenterPressed)
                {
                    if (resetTimer > 0)
                    {
                        resetTimer -= Time.deltaTime;
                        if (resetTimer <= 0)
                        {
                            Recenter();
                        }
                    }
                }
            }
        }
    }
}
