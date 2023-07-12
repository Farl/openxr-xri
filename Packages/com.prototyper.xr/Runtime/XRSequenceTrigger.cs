using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace SS
{
    public class XRSequenceTrigger : MonoBehaviour, IXRTrigger, ICondition
    {
        #region Public Classes / Enums
        [System.Serializable]
        public class TriggerEvents
        {
            public UnityEvent onActivate;
            public UnityEvent onDeactivate;
        }
        public enum SequenceCheckMethod
        {
            Activate,
            Hover,
            Select,
        }
        [System.Flags]
        public enum SequenceCheckMethodFlag
        {
            Activate = 1 << SequenceCheckMethod.Activate,
            Hover = 1 << SequenceCheckMethod.Hover,
            Select = 1 << SequenceCheckMethod.Select,
        }
        #endregion

        #region Inspector
        [SerializeField] private List<Animator> animators = new List<Animator>();
        [SerializeField] private string animParamHover = @"isHovered";    // isEnter
        [SerializeField] private TriggerEvents triggerEvents;
        [Header("Sequence Check")]
        [SerializeField] SequenceCheckMethodFlag checkMethod = SequenceCheckMethodFlag.Activate;
        [SerializeField] private bool checkOnEnter = true;
        [SerializeField] private bool checkImmediate = false;
        [Header("Action")]
        [SerializeField] private XRSequenceAction action;
        [SerializeField] private bool triggerOnce = true;
        #endregion

        #region Public

        public System.Action<IXRTrigger> onActivate { get => onEnter; set { onEnter = value; } }
        public System.Action<IXRTrigger> onDeactivate { get => onLeave; set { onLeave = value; } }
        public System.Action<IXRTrigger> onEnter;
        public System.Action<IXRTrigger> onLeave;

        public void OnEnter(TriggerHoverEnterArgs args)
        {
            onEnter?.Invoke(this);
            SetAnimatorBool(animParamHover, true);
            var currArgs = args;

            if (checkOnEnter && ((int)checkMethod & (int)SequenceCheckMethodFlag.Hover) != 0)
            {
                OnTriggerCheck(currArgs, (b) =>
                {
                    if (action)
                    {
                        action.OnHoverEntered(currArgs, b);
                    }
                });
            }
            else
            {
                if (action)
                {
                    action.OnHoverEntered(currArgs, false);
                }
            }
        }

        public void OnLeave(TriggerHoverExitArgs args)
        {
            onLeave?.Invoke(this);
            SetAnimatorBool(animParamHover, false);
            var currArgs = args;

            if (!checkOnEnter && ((int)checkMethod & (int)SequenceCheckMethodFlag.Hover) != 0)
            {
                OnTriggerCheck(currArgs, (b) =>
                {
                    if (action)
                    {
                        action.OnHoverExited(currArgs, b);
                    }
                });
            }
            else
            {
                if (action)
                {
                    action.OnHoverExited(currArgs, false);
                }    
            }
        }

        public void OnActivate(TriggerActivateEventArgs args)
        {
            triggerEvents.onActivate?.Invoke();
            var currArgs = args;

            if (checkOnEnter && ((int)checkMethod & (int)SequenceCheckMethodFlag.Activate) != 0)
            {
                OnTriggerCheck(currArgs, (b) =>
                {
                    if (action)
                    {
                        action.OnActivate(currArgs, b);
                    }
                });
            }
            else
            {
                if (action)
                {
                    action.OnActivate(currArgs, false);
                }
            }
        }

        public void OnDeactivate(TriggerDeactivateEventArgs args)
        {
            triggerEvents.onDeactivate?.Invoke();
            var currArgs = args;

            if (!checkOnEnter && ((int)checkMethod & (int)SequenceCheckMethodFlag.Activate) != 0)
            {
                OnTriggerCheck(currArgs, (b) =>
                {
                    if (action)
                    {
                        action.OnDeactivate(currArgs, b);
                    }
                });
            }
            else
            {
                if (action)
                {
                    action.OnDeactivate(currArgs, false);
                }
            }
        }

        public void OnSelectEnter(TriggerSelectEnterArgs args)
        {
            var currArgs = args;

            if (checkOnEnter && ((int)checkMethod & (int)SequenceCheckMethodFlag.Select) != 0)
            {
                OnTriggerCheck(currArgs, (b) =>
                {
                    if (action)
                    {
                        action.OnSelectEnter(currArgs, b);
                    }
                });
            }
            else
            {
                if (action)
                {
                    action.OnSelectEnter(currArgs, false);
                }
            }
        }

        public void OnSelectExit(TriggerSelectExitArgs args)
        {
            var currArgs = args;

            if (!checkOnEnter && ((int)checkMethod & (int)SequenceCheckMethodFlag.Select) != 0)
            {
                OnTriggerCheck(currArgs, (b) => {
                    if (action)
                    {
                        action.OnSelectExit(currArgs, b);
                    }
                });
            }
            else
            {
                if (action)
                {
                    action.OnSelectExit(currArgs, false);
                }
            }
        }
        #endregion

        #region Private/Protected
        private bool isTriggered = false;
        private void SetAnimatorBool(string name, bool value)
        {
            foreach (var a in animators)
            {
                if (a != null)
                {
                    a.SetBool(name, value);
                }
            }
        }

        protected virtual void Awake()
        {
            if (animators.Count <= 0)
            {
                var a = GetComponent<Animator>();
                animators.Add(a);
            }
        }

        private void OnSequenceCheckInternal(bool checkSuccess, TriggerBaseEventArgs args)
        {
            OnSequenceCheck(checkSuccess, args);
        }

        protected virtual void OnSequenceCheck(bool checkSuccess, TriggerBaseEventArgs args)
        {

        }

        private void OnTriggerCheck(TriggerBaseEventArgs args, Action<bool> callback)
        {
            var currArgs = args;
            if (triggerOnce && isTriggered)
            {
                return;
            }
            SequenceManager.OnTrigger(args,
                (b) =>
                {
                    if (b)
                    {
                        isTriggered = true;
                    }
                    callback?.Invoke(b);
                    OnSequenceCheckInternal(b, currArgs);
                }, checkImmediate);
        }
        #endregion

    }

}