using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS
{

    public class XRButtonController : ButtonController
    {
        [SerializeField] protected XRButton xrButton;

        protected override void Awake()
        {
            base.Awake();
            if (!xrButton)
            {
                xrButton = GetComponent<XRButton>();
            }
            if (xrButton)
            {
                xrButton.onInteract += OnClickNonSystemCall;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (xrButton)
            {
                xrButton.onInteract -= OnClickNonSystemCall;
            }
        }
    }

}