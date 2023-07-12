using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

namespace SS
{
    public class XRToggle : XRButton, IChoose
    {
        #region Public Classes / Enums
        [System.Serializable]
        public class ToggleEvents
        {
            public UnityEvent<bool> onValueChanged;
        }
        #endregion

        #region Inspector
        [Header("Toggle")]
        [SerializeField] private bool isOn = false;
        [SerializeField] private ChooseGroup chooseGroup;
        [SerializeField] private string animParamChoiced = @"isChoiced";
        [SerializeField] private ToggleEvents toggleEvents;
        #endregion

        #region Public

        public bool IsOn
        {
            get => isOn;
        }

        public void SetIsOn(bool newIsOn, bool notify, IChooseGroup fromChooseGroup)
        {
            if ((IChooseGroup)chooseGroup == fromChooseGroup)
            {
                SetIsOn(newIsOn, notify);
            }
        }

        public bool SetIsOn(bool newIsOn, bool notify)
        {
            bool prevIsOn = isOn;

            isOn = newIsOn;

            if (notify)
            {
                if (prevIsOn != isOn)
                {
                    toggleEvents.onValueChanged?.Invoke(isOn);
                }
            }

            SetAnimatorBool(animParamChoiced, isOn);

            return isOn;
        }
        #endregion

        #region Private / Protected

        protected override void Awake()
        {
            base.Awake();
            if (chooseGroup != null) { chooseGroup.Register(this); }
            if (isOn)
            {
                if (chooseGroup != null)
                {
                    chooseGroup.Choose(this, false);
                }
                else
                {
                    SetIsOn(isOn, false);
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (chooseGroup != null) { chooseGroup.Unregister(this); }
        }

        protected override void OnInteract(BaseInteractionEventArgs args)
        {
            base.OnInteract(args);

            if (chooseGroup != null)
            {
                chooseGroup.Choose(this, true);
            }
            else
            {
                SetIsOn(!isOn, true);
            }
        }
        #endregion
    }
}
