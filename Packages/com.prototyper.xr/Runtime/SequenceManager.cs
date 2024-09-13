using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SS
{
    public static class SequenceManager
    {
        public static ISequenceController CurrentController { get; private set; }

        public static void Register(ISequenceController controller) { if (CurrentController == null) { CurrentController = controller; } }

        public static void Unregister(ISequenceController controller) { if (CurrentController == controller) { CurrentController = null; } }

        public static void OnTrigger(TriggerBaseEventArgs args, System.Action<bool> onCheck, bool immediate = false)
        {
            if (CurrentController == null) { onCheck?.Invoke(true); return; }
            CurrentController.OnTrigger(args, onCheck, immediate);
        }

        public static void OnInteractable(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable, BaseInteractionEventArgs args, System.Action<bool> onCheck = null, bool immediate = false)
        {
            if (CurrentController == null) { onCheck?.Invoke(true); return; }
            CurrentController.OnInteractable(interactable, args, onCheck, immediate);
        }

        public static bool CheckEquipment(IXREquipment equipment)
        {
            if (CurrentController == null) return true;
            return CurrentController.CheckEquipment(equipment);
        }
        public static bool CheckConditionChecker(IConditionChecker checker, bool checkOnly = false, bool immediate = false)
        {
            if (CurrentController == null) return true;
            return CurrentController.CheckConditionChecker(checker, checkOnly, immediate);
        }
    }

}