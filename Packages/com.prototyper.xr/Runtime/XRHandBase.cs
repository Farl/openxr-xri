using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

namespace SS
{
    [RequireComponent(typeof(Rigidbody))]
    public class XRHandBase : MonoBehaviour, IXRHand
    {
        #region Public Class/Enum
        public enum State
        {
            Idle = 0,
            Equipment = 1,
        }

        [System.Serializable]
        public class EquipmentData
        {
            public string name;
            public int stateID;
        }
        #endregion

        #region Public
        public bool IsLeft => name.Contains("Left", System.StringComparison.InvariantCultureIgnoreCase);
        public Transform HandModelTransform => handModelTransform;
        public Transform HandTransform
        {
            get
            {
                if (cachedTransform == null)
                    cachedTransform = transform;
                return cachedTransform;
            }
        }
        public Animator HandAnimator => animator;
        public Transform AttachTransform => attachTransform;
        public bool IsPrimaryButtonPressed { get; private set; } = false;
        public bool IsPrimaryButtonPressDown { get; private set; } = false;
        public bool IsDebugMenuButtonPressed { get; private set; } = false;
        public float TriggerValue { get; private set; } = 0;
        public float SelectValue { get; private set; } = 0;
        public string CurrentEquipmentName
        {
            get
            {
                if (CurrentEquipment == null) return null;
                return CurrentEquipment.EquipmentName;
            }
        }
        public IXREquipment CurrentEquipment
        {
            get
            {
                if (currEquipmentInteractable == null) return null;
                return currEquipmentInteractable.Equipment;
            }
        }

        public void UpdateHolding(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable)
        {
            // Animation
            if (animator && animator.isActiveAndEnabled)
            {
                animator.SetFloat("triggerValue", TriggerValue);
            }
        }

        public bool CanAttach()
        {
            return (currAttachmentObject == null);
        }

        public void Attach(GameObject go, Transform trans, IXREquipmentInteractable interactable)
        {
            currEquipmentInteractable = interactable;

            if (currAttachmentObject != null)
            {
                Detach(currAttachmentObject, currAttachmentTransform, interactable);
            }

            // Exception
            if (attachTransform == null)
            {
                attachTransform = transform;
            }

            if (showDebugInfo)
                Debug.Log($"Attach {go.name} of {interactable.name}");

            currAttachmentObject = go;
            currAttachmentTransform = trans;

            currAttachmentObject.SetActive(true);
            currAttachmentTransform.SetParent(attachTransform);
            currAttachmentTransform.localPosition = Vector3.zero;
            currAttachmentTransform.localRotation = Quaternion.identity;

            // Hierarchy has been changed
            if (animator)
                animator.Rebind();

            stateMachine.SetNextState(State.Equipment);
        }

        public void Place(GameObject go, Transform trans, IXREquipmentInteractable interactable)
        {
            if (currAttachmentObject == go)
            {
                if (showDebugInfo)
                    Debug.Log($"Place {go.name} of {interactable.name}");

                currAttachmentObject = null;
                currAttachmentTransform = null;
                currEquipmentInteractable = null;

                trans.SetParent(null);

                // Hierarchy has been changed
                if (animator)
                    animator.Rebind();

                stateMachine.SetNextState(State.Idle);
            }
        }

        public void Detach(GameObject go, Transform trans, IXREquipmentInteractable interactable)
        {
            if (go == currAttachmentObject)
            {
                if (showDebugInfo)
                    Debug.Log($"Detach {go.name} of {interactable.name}");

                currAttachmentObject = null;
                currAttachmentTransform = null;
                currEquipmentInteractable = null;

                // Hierarchy has been changed
                if (animator)
                    animator.Rebind();

                stateMachine.SetNextState(State.Idle);
                Destroy(go);
            }
        }

        public void OnActivated(ActivateEventArgs args)
        {
            var interactor = activateInteractor = args.interactorObject;

            if (currTrigger != null)
            {
                var eventArgs = new TriggerActivateEventArgs()
                {
                    interactorObj = interactor,
                    triggerObj = currTrigger,
                    equipmentTransform = currAttachmentTransform,
                    equipmentInteractable = currEquipmentInteractable,
                };
                currTrigger.OnActivate(eventArgs);
            }
        }

        public void OnDeactivated(DeactivateEventArgs args)
        {
            var interactor = args.interactorObject;

            if (currTrigger != null)
            {
                var eventArgs = new TriggerDeactivateEventArgs()
                {
                    interactorObj = interactor,
                    triggerObj = currTrigger,
                    equipmentTransform = currAttachmentTransform,
                    equipmentInteractable = currEquipmentInteractable
                };
                currTrigger.OnDeactivate(eventArgs);
            }
        }

        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            var interactor = args.interactorObject;

            if (currTrigger != null)
            {
                var eventArgs = new TriggerSelectEnterArgs()
                {
                    interactorObj = interactor,
                    triggerObj = currTrigger,
                    equipmentTransform = currAttachmentTransform,
                    equipmentInteractable = currEquipmentInteractable,
                };
                currTrigger.OnSelectEnter(eventArgs);
            }
        }

        public void OnSelectExited(SelectExitEventArgs args)
        {
            var interactor = args.interactorObject;

            if (currTrigger != null)
            {
                var eventArgs = new TriggerSelectExitArgs()
                {
                    interactorObj = interactor,
                    triggerObj = currTrigger,
                    equipmentTransform = currAttachmentTransform,
                    equipmentInteractable = currEquipmentInteractable,
                };
                currTrigger.OnSelectExit(eventArgs);
            }
        }

        public bool SendHapticImpulse(float amplitude, float duration)
        {
            if (xrController != null)
            {
                xrController.SendHapticImpulse(amplitude, duration);
            }
            return true;
        }
        #endregion

        #region Inspector

        [Header("Visual model")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform handModelTransform;

        [Header("Controller")]
        [SerializeField] private XRBaseController xrController;
        [SerializeField] private InputActionReference selectAction;
        [SerializeField] private InputActionReference triggerAction;
        [SerializeField] private InputActionReference primaryButtonAction;
        [SerializeField] private InputActionReference debugMenuAction;

        [Header("Equipment")]
        [SerializeField] private List<EquipmentData> supportedEquipments;
        [SerializeField] private Transform attachTransform;
        [SerializeField] private float grabSpeed = 1f;

        [Header("Interactor")]
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor debugMenuInteractor;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        [Header("Legacy")]
        [SerializeField] private GameObject controllerObject;

        #endregion

        #region Static
        static private List<XRHandBase> xrHandBases = new List<XRHandBase>();
        static private StringFlag rayInteractorFlag = new StringFlag();
        static private XRHandBase primaryHand;

        static private void OnDebugMenuToggle(bool isOn)
        {
            RegisterRayInteractor("DebugMenu", isOn);
        }

        static public void RegisterRayInteractor(string flag, bool enabled)
        {
            if (enabled)
            {
                rayInteractorFlag.AddFlag(flag);
            }
            else
            {
                rayInteractorFlag.RemoveFlag(flag);
            }
        }
        #endregion

        #region Private/Protected
        private Transform cachedTransform;
        private StateMachine<State> stateMachine;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRActivateInteractor activateInteractor = null;
        private IXRTrigger currTrigger = null;
        private IXRTrigger prevTrigger = null;
        private List<IXRTrigger> triggerList = new List<IXRTrigger>();
        private IXREquipmentConstraint currConstraint = null;
        private IXREquipmentConstraint prevConstraint = null;
        private List<IXREquipmentConstraint> constraintList = new List<IXREquipmentConstraint>();

        private float targetValue = 0f;
        private float currValue = 0f;

        private GameObject currAttachmentObject;
        private Transform currAttachmentTransform;
        private IXREquipmentInteractable currEquipmentInteractable;
        private bool rayInteractorPrevEnabled = false;

        private Coroutine holdingCoroutine;

        protected virtual void OnDestroy()
        {
            xrHandBases.Remove(this);
        }

        private void Awake()
        {
#if !FINAL
            if (xrHandBases.Count == 0)
                DebugMenu.onMenuToggle += OnDebugMenuToggle;
#endif

            xrHandBases.Add(this);
            if (primaryHand == null && !this.IsLeft)
                primaryHand = this;
            
            if (controllerObject)
            {
                controllerObject.SetActive(false);
            }

            if (debugMenuInteractor)
                debugMenuInteractor.gameObject.SetActive(false);

            // Remove everything under Attachment position
            if (attachTransform)
            {
                var childrenList = new List<Transform>();
                foreach (Transform ct in attachTransform)
                {
                    childrenList.Add(ct);
                }
                foreach (var ct in childrenList)
                {
                    Destroy(ct.gameObject);
                }
            }

            // State machine
            stateMachine = new StateMachine<State>((sm, from, to) => true);
            stateMachine.AddStates(OnEnterState, OnLeaveState, OnUpdateState,
                new State[] { State.Idle, State.Equipment });
            stateMachine.SetNextState(State.Idle);

            if (xrController == null)
            {
                xrController = GetComponent<XRController>();
            }
        }

        private void OnEnterState(StateMachine<State> sm, State from, State to)
        {
            switch (to)
            {
                case State.Idle:
                    if (animator)
                        animator.SetInteger("state", (int)to);
                    break;
                case State.Equipment:
                    if (!string.IsNullOrEmpty(CurrentEquipmentName))
                    {
                        var equipData = supportedEquipments.Find(
                            (x) => x.name.Equals(CurrentEquipmentName, System.StringComparison.InvariantCulture)
                        );
                        if (equipData != null)
                        {
                            if (animator)
                                animator.SetInteger("state", equipData.stateID);
                        }
                    }
                    break;
            }
        }

        private void OnLeaveState(StateMachine<State> sm, State from, State to)
        {
            switch (from)
            {
                case State.Idle:
                    break;
                case State.Equipment:
                    break;
            }
        }

        private void OnUpdateState(StateMachine<State> sm)
        {
            switch (sm.CurrentState)
            {
                case State.Idle:
                    break;
                case State.Equipment:
                    break;
            }
        }

        private void UpdateInput()
        {
            // Input: trigger
            TriggerValue = triggerAction ? triggerAction.action.ReadValue<float>() : 0f;

            // Input: grab(select)
            SelectValue = selectAction ? selectAction.action.ReadValue<float>() : 0f;

            // Input: primary button
            var prevIsPrimaryButtonPressed = IsPrimaryButtonPressed;
            if (primaryButtonAction)
            {
                IsPrimaryButtonPressed = primaryButtonAction.action.IsPressed();
            }
            IsPrimaryButtonPressDown = IsPrimaryButtonPressed && !prevIsPrimaryButtonPressed;

            if (primaryHand != this && (TriggerValue > 0.5f || SelectValue > 0.5f))
            {
                primaryHand = this;
            }

#if !FINAL
            // Input: debug menu
            var prevIsDebugMenuButtonPressed = IsDebugMenuButtonPressed;
            IsDebugMenuButtonPressed = debugMenuAction? debugMenuAction.action.IsPressed(): false;
            if (IsDebugMenuButtonPressed && !prevIsDebugMenuButtonPressed)
            {
                if (UIManager.IsShow("DebugMenu"))
                {
                    UIManager.HideUI("DebugMenu");
                }
                else
                {
                    UIManager.ShowUI("DebugMenu");
                }

            }
#endif
        }

        private void Update()
        {
            UpdateInput();

            if (selectAction != null)
            {
                targetValue = TriggerValue;
            }

            if (animator)
            {
                animator.SetFloat("grabValue", SelectValue);
            }


            currValue = Mathf.MoveTowards(currValue, targetValue, grabSpeed * Time.deltaTime);
        }


        private void LateUpdate()
        {
            LateUpdateConstraint();

            bool shouldEnabled = this == primaryHand && !rayInteractorFlag.IsEmpty;
            if (shouldEnabled != rayInteractorPrevEnabled)
            {
                if (showDebugInfo)
                    Debug.Log($"{name} {shouldEnabled}");
                if (debugMenuInteractor)
                {
                    debugMenuInteractor.gameObject.SetActive(shouldEnabled);
                }
                rayInteractorPrevEnabled = shouldEnabled;
            }
        }

        private void FixedUpdate()
        {
            UpdateTrigger();
            UpdateConstraint();
        }

        private void LateUpdateConstraint()
        {
            if (currConstraint != null && currConstraint.IsLockConstraint)
            {
                if (currConstraint.LateUpdateConstraint(this, CurrentEquipment))
                {
                    //
                }
                else
                {
                    currConstraint.RemoveConstraint(this, CurrentEquipment);
                    currConstraint = null;
                }
            }
        }

        private void UpdateConstraint()
        {
            if (currConstraint != null && currConstraint.IsLockConstraint)
            {
                if (currConstraint.UpdateConstraint(this, CurrentEquipment))
                {
                    //
                }
                else
                {
                    currConstraint.RemoveConstraint(this, CurrentEquipment);
                    currConstraint = null;
                }
            }
            else
            {
                if (constraintList.Count > 0)
                {
                    currConstraint = constraintList[0];
                }
                else
                {
                    currConstraint = null;
                }

                if (prevConstraint != currConstraint)
                {
                    // Leave
                    if (prevConstraint != null)
                    {
                        prevConstraint.RemoveConstraint(this, CurrentEquipment);
                    }

                    // Enter
                    if (currConstraint != null)
                    {
                        currConstraint.ApplyConstraint(this, CurrentEquipment);
                    }

                    prevConstraint = currConstraint;
                }
                else if (currConstraint != null)
                {
                    currConstraint.UpdateConstraint(this, CurrentEquipment);
                }
            }
            constraintList.Clear();
        }

        private void UpdateTrigger()
        {
            if (triggerList.Count > 0)
            {
                currTrigger = triggerList[0];
            }
            else
            {
                currTrigger = null;
            }

            if (prevTrigger != currTrigger)
            {
                // Leave
                if (prevTrigger != null)
                {
                    prevTrigger.OnLeave(
                        new TriggerHoverExitArgs
                        {
                            triggerObj = prevTrigger,
                            interactorObj = null,
                            equipmentTransform = currAttachmentTransform,
                            equipmentInteractable = currEquipmentInteractable
                        }
                    );
                }

                // Enter
                if (currTrigger != null)
                {
                    SendHapticImpulse(0.5f, 0.1f);
                    currTrigger.OnEnter(
                        new TriggerHoverEnterArgs
                        {
                            triggerObj = currTrigger,
                            interactorObj = null,
                            equipmentTransform = currAttachmentTransform,
                            equipmentInteractable = currEquipmentInteractable
                        }
                    );
                }

                prevTrigger = currTrigger;
            }

            triggerList.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
        }

        private void OnTriggerStay(Collider other)
        {
            var otherTransform = other.transform;

            var controller = otherTransform.GetComponent<IXRTrigger>();
            if (controller != null)
                triggerList.Add(controller);

            var constraint = otherTransform.GetComponentInParent<IXREquipmentConstraint>();
            if (constraint != null && constraint.CheckConstraint(this, CurrentEquipment))
                constraintList.Add(constraint);

        }
        #endregion
    }

}