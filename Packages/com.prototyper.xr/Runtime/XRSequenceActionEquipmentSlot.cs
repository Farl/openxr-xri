using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace SS
{
    public class XRSequenceActionEquipmentSlot : XRSequenceAction
    {
        public enum AttachMethod
        {
            Activate,
            SelectExit,
        }
        [SerializeField] private AttachMethod attachMethod = AttachMethod.Activate;
        [SerializeField] private Transform slotTransform;
        [SerializeField] private bool checkSequence = true;
        [SerializeField] private bool autoFixAttachYAngle = true;
        [SerializeField] private UnityEvent onInsert;
        [SerializeField] private UnityEvent onExtract;
        [SerializeField] public XRSequenceActionEquipmentSlotGroup group;
        [SerializeField] private bool attachToHideObject = true;

        private IXREquipmentInteractable equipmentInteractable;

        private void Awake()
        {
            if (slotTransform == null)
                slotTransform = transform;
        }

        public override void OnHoverEntered(TriggerHoverEnterArgs args, bool checkSuccess)
        {
            base.OnHoverEntered(args, checkSuccess);
        }

        public override void OnHoverExited(TriggerHoverExitArgs args, bool checkSuccess)
        {
            base.OnHoverExited(args, checkSuccess);
        }

        public override void OnActivate(TriggerActivateEventArgs args, bool checkSuccess)
        {
            if (attachMethod == AttachMethod.Activate)
            {
                AttachTo(args, checkSuccess);
            }
            base.OnActivate(args, checkSuccess);
        }

        public override void OnDeactivate(TriggerDeactivateEventArgs args, bool checkSuccess)
        {
            base.OnDeactivate(args, checkSuccess);
        }

        private void AttachTo(TriggerBaseEventArgs args, bool checkSuccess)
        {
            //Debug.Log($"AttachTo: {args} {checkSuccess}");
            if (!checkSequence || checkSuccess)
            {
                equipmentInteractable = args.equipmentInteractable;
                equipmentInteractable.AttachTo(slotTransform,
                    () =>
                    {
                        onInsert?.Invoke();
                        if (group != null)
                        {
                            group.OnAttach();
                        }
                    },
                    autoFixAttachYAngle, attachToHideObject
                    );
            }
        }

        public override void OnSelectEntered(SelectEnterEventArgs args, bool checkSuccess)
        {
            if (equipmentInteractable != null)
            {
                equipmentInteractable.ReleaseAttachTo(args);
                onExtract?.Invoke();
            }
            base.OnSelectEntered(args, checkSuccess);
        }

        public override void OnSelectExited(SelectExitEventArgs args, bool checkSuccess)
        {
            if (equipmentInteractable != null)
            {
                equipmentInteractable.ReleaseAttachToExited(args);
                equipmentInteractable = null;
            }
            base.OnSelectExited(args, checkSuccess);
        }

        public override void OnSelectEnter(TriggerSelectEnterArgs args, bool checkSuccess)
        {
            base.OnSelectEnter(args, checkSuccess);
        }

        public override void OnSelectExit(TriggerSelectExitArgs args, bool checkSuccess)
        {
            if (equipmentInteractable == null)
            {
                if (attachMethod == AttachMethod.SelectExit)
                {
                    AttachTo(args, checkSuccess);
                }
            }
            base.OnSelectExit(args, checkSuccess);
        }
    }
}
