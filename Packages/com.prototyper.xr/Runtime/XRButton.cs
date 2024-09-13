using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

namespace SS
{
    public class XRButton : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
    {
        #region Public Classes / Enums
        public enum InteractableMessage
        {
            Select = 0,
            Activate = 1,
        }

        [System.Flags]
        public enum InteractableMessageFlag
        {
            Select = 1 << InteractableMessage.Select,
            Activate = 1 << InteractableMessage.Activate,
        }

        [System.Serializable]
        public class InteractEvents
        {
            public UnityEvent onInteract;
        }

        public enum AnimatorParameterType
        {
            Bool,
            Trigger,
        }

        protected enum ButtonState
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled,
        }

        #endregion

        #region Inspector
        [Header("Debug")]
        [SerializeField] protected bool showDebugInfo = false;

        [Header("Animation")]
        [SerializeField] public List<Animator> animators = new List<Animator>();
        [SerializeField] private AnimatorParameterType animatorParameterType = AnimatorParameterType.Bool;
        [SerializeField] private string animParamHover = @"isHovered";    // Highlighted, isEnter
        [SerializeField] private string animParamNormal = @"";
        [SerializeField] private string animParamPressed = @"";
        [SerializeField] private string animParamSelected = @"";
        [SerializeField] private string animParamDisabled = @"";

        [Header("Interaction")]
        [SerializeField] protected InteractableMessageFlag interactMethod = InteractableMessageFlag.Select;
        [SerializeField] protected bool interactOnEnter = false;
        [SerializeField] private InteractEvents interactEvents;

        #endregion

        #region Public
        public System.Action onInteract;
        #endregion

        #region Private / Protected
        private StateMachine<ButtonState> stateMachine = new StateMachine<ButtonState>();

        private bool isHovering = false;
        private bool isPressed = false;

        protected void SetAnimatorBool(string name, bool value)
        {
            if (string.IsNullOrEmpty(name))
                return;
            foreach (var a in animators)
            {
                if (a != null)
                {
                    a.SetBool(name, value);
                }
            }
        }
        protected void SetAnimatorTrigger(string param, bool reset = false)
        {
            if (string.IsNullOrEmpty(param))
                return;
            if (showDebugInfo)
                Debug.Log($"Button {this.name} SetAnimatorTrigger {param}");
            foreach (var a in animators)
            {
                if (a != null)
                {
                    if (reset)
                        a.ResetTrigger(param);
                    else
                        a.SetTrigger(param);
                }
            }
        }

        protected void SetAnimatorTrigger(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Normal:
                    SetAnimatorTrigger(animParamNormal);
                    SetAnimatorTrigger(animParamHover, true);
                    SetAnimatorTrigger(animParamPressed, true);
                    SetAnimatorTrigger(animParamSelected, true);
                    SetAnimatorTrigger(animParamDisabled, true);
                    break;
                case ButtonState.Highlighted:
                    SetAnimatorTrigger(animParamHover);
                    SetAnimatorTrigger(animParamNormal, true);
                    SetAnimatorTrigger(animParamPressed, true);
                    SetAnimatorTrigger(animParamSelected, true);
                    SetAnimatorTrigger(animParamDisabled, true);
                    break;
                case ButtonState.Pressed:
                    SetAnimatorTrigger(animParamPressed);
                    SetAnimatorTrigger(animParamNormal, true);
                    SetAnimatorTrigger(animParamHover, true);
                    SetAnimatorTrigger(animParamSelected, true);
                    SetAnimatorTrigger(animParamDisabled, true);
                    break;
                case ButtonState.Selected:
                    SetAnimatorTrigger(animParamSelected);
                    SetAnimatorTrigger(animParamNormal, true);
                    SetAnimatorTrigger(animParamHover, true);
                    SetAnimatorTrigger(animParamPressed, true);
                    SetAnimatorTrigger(animParamDisabled, true);
                    break;
                case ButtonState.Disabled:
                    SetAnimatorTrigger(animParamDisabled);
                    SetAnimatorTrigger(animParamNormal, true);
                    SetAnimatorTrigger(animParamHover, true);
                    SetAnimatorTrigger(animParamPressed, true);
                    SetAnimatorTrigger(animParamSelected, true);
                    break;
            }
        }

        private void EnterState(StateMachine<ButtonState> sm, ButtonState fromState, ButtonState toState)
        {
            switch (toState)
            {
                case ButtonState.Normal:
                    if (animatorParameterType == AnimatorParameterType.Trigger)
                        SetAnimatorTrigger(toState);
                    break;
                case ButtonState.Highlighted:
                    if (animatorParameterType == AnimatorParameterType.Trigger)
                        SetAnimatorTrigger(toState);
                    else if (animatorParameterType == AnimatorParameterType.Bool)
                        SetAnimatorBool(animParamHover, true);
                    break;
                case ButtonState.Pressed:
                    if (animatorParameterType == AnimatorParameterType.Trigger)
                        SetAnimatorTrigger(toState);
                    else if (animatorParameterType == AnimatorParameterType.Bool)
                        SetAnimatorBool(animParamPressed, true);
                    break;
                case ButtonState.Selected:
                    if (animatorParameterType == AnimatorParameterType.Trigger)
                        SetAnimatorTrigger(toState);
                    break;
                case ButtonState.Disabled:
                    if (animatorParameterType == AnimatorParameterType.Trigger)
                        SetAnimatorTrigger(toState);
                    break;
            }
        }

        // Leave state
        private void LeaveState(StateMachine<ButtonState> sm, ButtonState fromState, ButtonState toState)
        {
            switch (fromState)
            {
                case ButtonState.Normal:
                    break;
                case ButtonState.Highlighted:
                    if (animatorParameterType == AnimatorParameterType.Bool)
                        SetAnimatorBool(animParamHover, false);
                    break;
                case ButtonState.Pressed:
                    if (animatorParameterType == AnimatorParameterType.Bool)
                        SetAnimatorBool(animParamPressed, false);
                    break;
                case ButtonState.Selected:
                    break;
                case ButtonState.Disabled:
                    break;
            }
        }

        // Update state
        private void UpdateState(StateMachine<ButtonState> sm)
        {
            switch (sm.CurrentState)
            {
                case ButtonState.Normal:
                    if (isPressed)
                    {
                        sm.SetNextState(ButtonState.Pressed);
                    }
                    else if (isHovering == true)
                    {
                        sm.SetNextState(ButtonState.Highlighted);
                    }
                    break;
                case ButtonState.Highlighted:
                    if (isPressed)
                    {
                        sm.SetNextState(ButtonState.Pressed);
                    }
                    else if (isHovering == false)
                    {
                        sm.SetNextState(ButtonState.Normal);
                    }
                    break;
                case ButtonState.Pressed:
                    if (!isPressed)
                    {
                        if (isHovering)
                        {
                            sm.SetNextState(ButtonState.Highlighted);
                        }
                        else
                        {
                            sm.SetNextState(ButtonState.Normal);
                        }
                    }
                    break;
                case ButtonState.Selected:
                    break;
                case ButtonState.Disabled:
                    break;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            stateMachine.AddStates(EnterState, LeaveState, UpdateState, ButtonState.Normal, ButtonState.Highlighted, ButtonState.Pressed, ButtonState.Selected, ButtonState.Disabled);
            stateMachine.SetNextState(ButtonState.Normal);

            if (animators.Count == 0)
            {
                var animator = GetComponent<Animator>();
                if (animator != null)
                {
                    animators.Add(animator);
                }
            }
        }

        protected override void OnHoverEntering(HoverEnterEventArgs args)
        {
            base.OnHoverEntering(args);
        }

        protected override void OnHoverExiting(HoverExitEventArgs args)
        {
            base.OnHoverExiting(args);
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            isHovering = true;
            if (showDebugInfo)
                Debug.Log($"Button {name} State={stateMachine.CurrentState.ToString()} Hover={isHovering} Pressed={isPressed}");
            stateMachine.Update();
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            isHovering = false;
            if (showDebugInfo)
                Debug.Log($"Button {name} State={stateMachine.CurrentState.ToString()} Hover={isHovering} Pressed={isPressed}");
            stateMachine.Update();
        }

        protected override void OnActivated(ActivateEventArgs args)
        {
            base.OnActivated(args);
            if (((uint)interactMethod & (uint)InteractableMessageFlag.Activate) != 0)
            {
                isPressed = true;
                stateMachine.Update();
                if (interactOnEnter == true)
                {
                    OnInteract(args);
                }
            }
        }

        protected override void OnDeactivated(DeactivateEventArgs args)
        {
            base.OnDeactivated(args);
            if (((uint)interactMethod & (uint)InteractableMessageFlag.Activate) != 0)
            {
                if (interactOnEnter == false)
                {
                    OnInteract(args);
                }
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            if (((uint)interactMethod & (uint)InteractableMessageFlag.Select) != 0)
            {
                isPressed = true;
                stateMachine.Update();
                if (interactOnEnter == true)
                {
                    OnInteract(args);
                }
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            if (((uint)interactMethod & (uint)InteractableMessageFlag.Select) != 0)
            {
                if (interactOnEnter == false)
                {
                    OnInteract(args);
                }
            }
        }

        protected virtual void OnInteract(BaseInteractionEventArgs args)
        {
            interactEvents.onInteract?.Invoke();
            onInteract?.Invoke();

            isPressed = false;
            stateMachine.Update();
        }
        #endregion
    }
}
