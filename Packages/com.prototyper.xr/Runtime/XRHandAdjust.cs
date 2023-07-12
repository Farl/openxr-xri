using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

namespace SS
{
    public class XRHandAdjust : XRBaseInteractable
    {
        [SerializeField] private Transform adjustTransform;
        [SerializeField] private bool _interactable;

        private Vector3 origPosition;
        private Vector3 origScale;
        private Quaternion origRotation;
        Coroutine selectingCoroutine;
        SelectEnterEventArgs selectArgs = null;

        private void Output()
        {
            Debug.LogError($"{debugID}\nT={adjustTransform.localPosition} R={adjustTransform.localEulerAngles} S={adjustTransform.localScale}");
        }

        private string debugID => $"{adjustTransform.name} ({GetInstanceID()})";
        private string debugDumpID => $"{debugID} Dump";
        private string debugToggleID => $"{debugID}";

        public bool interactable
        {
            get
            {
                return _interactable;
            }
            set
            {
                foreach (var coll in colliders)
                {
                    coll.enabled = value;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            interactable = _interactable;

            if (adjustTransform == null)
                adjustTransform = transform;
            origPosition = adjustTransform.localPosition;
            origRotation = adjustTransform.localRotation;
            origScale = adjustTransform.localScale;

            DebugMenu.AddButton("Adjust", debugDumpID,
                (obj) => { Output(); },
                (obj) => {}
            );
            DebugMenu.AddToggle("Adjust", debugToggleID,
                () => interactable,
                (b) => interactable = b
            );
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DebugMenu.Remove(debugDumpID);
            DebugMenu.Remove(debugToggleID);
        }

        private IEnumerator SelectingCoroutine()
        {
            var interactorTransform = selectArgs.interactorObject.transform;
            var origInteractorMatrix = interactorTransform.localToWorldMatrix;

            var origMatrix = adjustTransform.localToWorldMatrix;
            while (selectArgs.interactableObject == this as IXRInteractable)
            {
                var currMatrix = interactorTransform.localToWorldMatrix;
                
                var transformMatrix = currMatrix * origInteractorMatrix.inverse;

                adjustTransform.FromMatrix(transformMatrix * origMatrix);

                yield return null;
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            selectArgs = args;
            selectingCoroutine = StartCoroutine(SelectingCoroutine());
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            StopCoroutine(selectingCoroutine);
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);
        }
    }
}
