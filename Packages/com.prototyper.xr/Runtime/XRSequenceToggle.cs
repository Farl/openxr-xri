using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

namespace SS
{
    public class XRSequenceToggle : XRToggle, ICondition
    {

        #region Public Classes/Enums

        [System.Serializable]
        public class SequenceCheckEvents
        {
            public UnityEvent onCheckSuccess;
            public UnityEvent onCheckFail;
        }

        #endregion

        #region Inspector

        [Header("Sequence Check")]
        [SerializeField] private SequenceCheckEvents checkEvents;

        [Header("Action")]
        [SerializeField] private XRSequenceAction action;

        #endregion

        #region Public

        #endregion

        #region Private/Protected
        bool checkSuccess = true;

        protected virtual void OnSequenceCheck(bool checkSuccess, BaseInteractionEventArgs args)
        {
            if (checkSuccess)
                checkEvents.onCheckSuccess?.Invoke();
            else
                checkEvents.onCheckFail?.Invoke();
        }

        protected override void OnInteract(BaseInteractionEventArgs args)
        {
            base.OnInteract(args);
            checkSuccess = true;
            var currArgs = args;
            SequenceManager.OnInteractable(this, args, (b) =>
            {
                checkSuccess = b;
                OnSequenceCheck(checkSuccess, currArgs);
            });
        }

        protected override void OnActivated(ActivateEventArgs args)
        {
            base.OnActivated(args);
            var currArgs = args;
            if (action != null) { action.OnActivated(currArgs, checkSuccess); }
        }

        protected override void OnDeactivated(DeactivateEventArgs args)
        {
            base.OnDeactivated(args);
            var currArgs = args;
            if (action != null) { action.OnDeactivated(currArgs, checkSuccess); }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            var currArgs = args;
            if (action != null) { action.OnSelectEntered(currArgs, checkSuccess); }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            var currArgs = args;
            if (action != null) { action.OnSelectExited(currArgs, checkSuccess); }
        }

        #endregion
    }

}