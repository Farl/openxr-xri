
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SS
{
    public class TriggerBaseEventArgs
    {
        public IXRInteractor interactorObj;
        public IXRTrigger triggerObj;
        public IXREquipmentInteractable equipmentInteractable;
        public Transform equipmentTransform;
    }

    public class TriggerHoverEnterArgs : TriggerBaseEventArgs { }
    public class TriggerHoverExitArgs : TriggerBaseEventArgs { }

    public class TriggerActivateEventArgs : TriggerBaseEventArgs { }
    public class TriggerDeactivateEventArgs : TriggerBaseEventArgs { }

    public class TriggerSelectEnterArgs : TriggerBaseEventArgs { }
    public class TriggerSelectExitArgs : TriggerBaseEventArgs { }

}
