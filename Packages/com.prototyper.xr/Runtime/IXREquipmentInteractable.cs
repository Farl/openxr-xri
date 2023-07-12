using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SS
{
    public interface IXREquipmentChanged
    {
        public void OnHold(IXREquipmentInteractable equipment);
        public void OnRelease(IXREquipmentInteractable equipment);
        public void OnReleaseDone(IXREquipmentInteractable equipment);
        public void OnAttach(IXREquipmentInteractable equipment);
        public void OnDetach(IXREquipmentInteractable equipment);
    }

public interface IXREquipmentInteractable
    {
        public string name => null;

        public string EquipmentName => null;

        public bool IsMatchEquipment(string equipmentName);

        public GameObject EquipmentObject => null;

        public Transform EquipmentTransform => null;

        public IXREquipment Equipment => null;

        public List<Collider> Colliders => null;

        public void RegisterStateChanged(IXREquipmentChanged changeHandler);
        public void UnregisterStateChanged(IXREquipmentChanged changeHandler);

        public void AttachTo(Transform targetTransform, System.Action callback, bool autoFixYAngle, bool attachToHideObject);

        public void ReleaseAttachTo(BaseInteractionEventArgs args);

        public void ReleaseAttachToExited(BaseInteractionEventArgs args);
    }

}