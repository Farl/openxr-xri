using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace SS
{
    public class VRHelper : MonoBehaviour
    {
        public InputActionManager inputActionManager;
        public GameObject simulatorObject;
        public InputActionReference startSimulation;

        private void Start()
        {
            if (inputActionManager)
            {
#if UNITY_EDITOR
            inputActionManager.enabled = true;
#else
                inputActionManager.enabled = false;
#endif
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (simulatorObject != null && !simulatorObject.activeSelf && startSimulation != null && startSimulation.action.IsPressed())
            {
                simulatorObject.SetActive(true);
                this.enabled = false;
            }
        }
    }
}

