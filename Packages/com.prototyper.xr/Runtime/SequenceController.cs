using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;

namespace SS
{
    public class SequenceController : MonoBehaviour, ISequenceController
    {
        public bool CheckConditionChecker(IConditionChecker checker, bool checkOnly = false, bool immediate = false)
        {
            return true;
        }

        public bool CheckEquipment(IXREquipment equipment)
        {
            return true;
        }

        public void OnInteractable(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable, BaseInteractionEventArgs args, Action<bool> onCheck = null, bool immediate = false)
        {
            onCheck?.Invoke(true);
        }

        public void OnTrigger(TriggerBaseEventArgs args, Action<bool> onCheck, bool immediate = false)
        {
            onCheck?.Invoke(true);
        }
    }
}
