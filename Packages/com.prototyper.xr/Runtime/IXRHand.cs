using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

namespace SS
{
    public interface IXRHand
    {
        public bool IsLeft => false;
        public Transform HandModelTransform => null;
        public Transform HandTransform => null;
        public Transform AttachTransform => null;
        public Animator HandAnimator => null;
        public void OnActivated(ActivateEventArgs args);
        public void OnDeactivated(DeactivateEventArgs args);
        public void OnSelectEntered(SelectEnterEventArgs args);
        public void OnSelectExited(SelectExitEventArgs args);
        public void UpdateHolding(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable selectInteractable);
        public bool CanAttach();
        public void Attach(GameObject go, Transform transform, IXREquipmentInteractable selectInteractable);
        public void Detach(GameObject go, Transform transform, IXREquipmentInteractable selectInteractable);
        public void Place(GameObject go, Transform transform, IXREquipmentInteractable selectInteractable);
        public bool SendHapticImpulse(float amplitude, float duration);
        public string CurrentEquipmentName => null;
        public bool IsPrimaryButtonPressed => false;
        public bool IsPrimaryButtonPressDown => false;
        public float TriggerValue => 0;
        public float SelectValue => 0;
    }
}