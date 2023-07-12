using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SS
{
    // Do something when Interactable/Trigger been interacted
    public class XRSequenceAction : MonoBehaviour
    {
        [SerializeField] private XRSequenceAction nextAction;

        public virtual void OnActivate(TriggerActivateEventArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnActivate(args, checkSuccess);
        }

        public virtual void OnDeactivate(TriggerDeactivateEventArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnDeactivate(args, checkSuccess);
        }

        public virtual void OnActivated(ActivateEventArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnActivated(args, checkSuccess);
        }

        public virtual void OnDeactivated(DeactivateEventArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnDeactivated(args, checkSuccess);
        }

        public virtual void OnSelectEntered(SelectEnterEventArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnSelectEntered(args, checkSuccess);
        }

        public virtual void OnSelectExited(SelectExitEventArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnSelectExited(args, checkSuccess);
        }

        public virtual void OnSelectEnter(TriggerSelectEnterArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnSelectEnter(args, checkSuccess);
        }

        public virtual void OnSelectExit(TriggerSelectExitArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnSelectExit(args, checkSuccess);
        }

        public virtual void OnHoverEntered(TriggerHoverEnterArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnHoverEntered(args, checkSuccess);
        }

        public virtual void OnHoverExited(TriggerHoverExitArgs args, bool checkSuccess)
        {
            if (nextAction)
                nextAction.OnHoverExited(args, checkSuccess);
        }
    }

}