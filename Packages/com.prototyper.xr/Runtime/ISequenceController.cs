
using UnityEngine.XR.Interaction.Toolkit;

namespace SS
{
    public interface ISequenceController
    {

        public void OnTrigger(TriggerBaseEventArgs args, System.Action<bool> onCheck, bool immediate = false);

        public void OnInteractable(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable, BaseInteractionEventArgs args, System.Action<bool> onCheck = null, bool immediate = false);

        public bool CheckEquipment(IXREquipment equipment);

        public bool CheckConditionChecker(IConditionChecker checker, bool checkOnly = false, bool immediate = false);
    }
}

