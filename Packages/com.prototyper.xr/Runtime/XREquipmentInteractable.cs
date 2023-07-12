using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace SS
{
    public class XREquipmentInteractable : XRButton, IXREquipmentInteractable
    {
        #region Static
        private static List<XREquipmentInteractable> interactables = new List<XREquipmentInteractable>();
        public static XREquipmentInteractable[] GetAllInteractables()
        {
            return interactables.ToArray();
        }
        #endregion

        #region Inspector
        #endregion

        #region Public
        public string EquipmentName =>
            (Equipment != null) ? Equipment.EquipmentName : (templateEquipment != null) ? templateEquipment.EquipmentName : null;
        public string OriginalEquipmentName =>
            (Equipment != null) ? Equipment.OriginalEquipmentName : (templateEquipment != null) ? templateEquipment.OriginalEquipmentName : null;

        public bool IsMatchEquipment(string equipmentName)
        {
            if (Equipment != null)
                return Equipment.IsMatchEquipment(equipmentName);

            if (templateEquipment != null)
                return templateEquipment.IsMatchEquipment(equipmentName);

            return false;
        }

        public GameObject EquipmentObject => spawnInstance;

        public Transform EquipmentTransform => spawnInstanceTransform;

        public IXREquipment Equipment => spawnEquipment;

        public List<Collider> Colliders => colliders;

        public void AttachTo(Transform targetTransform, System.Action callback, bool autoFixYAngle, bool attachToHideObject)
        {
            if (!spawnInstanceTransform)
                return;

            if (autoFixYAngle)
            {
                var dot = Vector3.Dot(spawnInstanceTransform.forward, targetTransform.forward);
                if (dot < 0)
                    targetTransform.Rotate(new Vector3(0, 180, 0), Space.Self);
            }

            attachToTrans = targetTransform;
            attachToCallback = callback;
            this.attachToHideObject = attachToHideObject;
            isAttachTo = true;
        }

        // Back to hand
        public void ReleaseAttachTo(BaseInteractionEventArgs args)
        {
            GetHandFromEventArgs(args);
            if (hand != null)
            {
                isAttachTo = false;
                isReleased = false;
            }
        }

        public void ReleaseAttachToExited(BaseInteractionEventArgs args)
        {
            isReleased = true;
        }

        public void RegisterStateChanged(IXREquipmentChanged changeHandler)
        {
            this.onHold += changeHandler.OnHold;
            this.onRelease += changeHandler.OnRelease;
            this.onReleaseDone += changeHandler.OnReleaseDone;
            this.onAttach += changeHandler.OnAttach;
            this.onDetach += changeHandler.OnDetach;
        }

        public void UnregisterStateChanged(IXREquipmentChanged changeHandler)
        {
            this.onHold -= changeHandler.OnHold;
            this.onRelease -= changeHandler.OnRelease;
            this.onReleaseDone -= changeHandler.OnReleaseDone;
            this.onAttach -= changeHandler.OnAttach;
            this.onDetach -= changeHandler.OnDetach;
        }
        #endregion

        #region Inspector

        [Header("Equipment")]
        [SerializeField] public GameObject template;

        [Header("Animator Extented")]
        [SerializeField] private string animParamCorrect = "isCorrect";
        [Header("Attach")]
        [SerializeField] private bool attachToHideObject = false;

        #endregion

        #region Private / Protected
        private IXREquipment templateEquipment;

        private GameObject spawnInstance;
        private Transform spawnInstanceTransform;
        private IXREquipment spawnEquipment;

        private IXRHand hand;
        private Transform handTransform;
        private Transform attachPosTrans;
        private Matrix4x4 pivotMatrix;

        private Coroutine holdLateCoroutine;

        private bool isReleased = false;
        private bool isUseLateUpdate = false;
        private bool isAttachTo = false;
        private System.Action attachToCallback;

        private Matrix4x4 targetMatrix;
        private Vector3 targetPos;
        private Quaternion targetRot;
        private Transform attachToTrans;

        private System.Action<IXREquipmentInteractable> onHold;
        private System.Action<IXREquipmentInteractable> onRelease;
        private System.Action<IXREquipmentInteractable> onReleaseDone;
        private System.Action<IXREquipmentInteractable> onAttach;
        private System.Action<IXREquipmentInteractable> onDetach;

        protected override void Awake()
        {
            interactables.Add(this);
            base.Awake();
            if (template != null)
            {
                templateEquipment = template.GetComponentInChildren<IXREquipment>();
                if (templateEquipment == null)
                {
                    Debug.LogError("Missing IXREquipment in template", this);
                }
            }
        }

        protected override void OnDestroy()
        {
            interactables.Remove(this);
            base.OnDestroy();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (holdLateCoroutine != null)
            {
                DetachAndClearReference();
                StopCoroutine(holdLateCoroutine);
                holdLateCoroutine = null;
            }
        }

        protected override void OnActivated(ActivateEventArgs args)
        {
            base.OnActivated(args);
            if (showDebugInfo)
                Debug.Log($"{name} Activated");
            if (hand != null)
            {
                hand.OnActivated(args);
            }
        }

        protected override void OnDeactivated(DeactivateEventArgs args)
        {
            base.OnDeactivated(args);
            if (showDebugInfo)
                Debug.Log($"{name} Deactivated");
            if (hand != null)
            {
                hand.OnDeactivated(args);
            }
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);

            // Check if correct equipment
            if (!string.IsNullOrEmpty(animParamCorrect))
                SetAnimatorBool(animParamCorrect, SequenceManager.CheckEquipment(templateEquipment));
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            if (showDebugInfo)
                Debug.Log($"Grab {name}");

            if (hand != null)
            {
                hand.OnSelectEntered(args);
            }

            if (holdLateCoroutine != null)
                return;
            holdLateCoroutine = StartCoroutine(HoldLateCoroutine(args));
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            if (showDebugInfo)
                Debug.Log($"Release {name}");

            if (hand != null)
            {
                hand.OnSelectExited(args);
            }

            base.OnSelectExited(args);

            // Only release without attach to anything
            if (isAttachTo == false)
                isReleased = true;
        }

        private IEnumerator HoldLateCoroutine(SelectEnterEventArgs args)
        {
            if (template == null)
                yield break;

            GetHandFromEventArgs(args);

            if (hand == null)
            {
                DetachAndClearReference();
                yield break;
            }

            // Prepare template state
            if (templateEquipment != null)
            {
                templateEquipment.BeforeSpawn();
            }

            spawnInstance = GameObject.Instantiate(template);

            // Recover template state
            if (templateEquipment != null)
            {
                templateEquipment.AfterSpawn();
            }

            spawnInstance.name = template.name;
            spawnInstanceTransform = spawnInstance.transform;
            spawnEquipment = spawnInstance.GetComponentInChildren<IXREquipment>();

            // Spawn & attach
            hand.Attach(spawnInstance, spawnInstanceTransform, this);

            if (spawnEquipment != null)
            {
                spawnEquipment.Equip(hand);
            }
            else
            {
                Debug.LogError("Missing IXREquipment", this);
            }

            // Disable all colliders
            foreach (var coll in colliders)
            {
                coll.enabled = false;
            }

            // Hold
            template.SetActive(false);
            isReleased = false;
            isUseLateUpdate = true;
            float time = 0.2f;
            float timer = time;

            // Attach
            Transform templateTrans = template.transform;
            Vector3 origPos = templateTrans.position;
            Quaternion origRot = templateTrans.rotation;
            Vector3 fromPos = origPos;
            Quaternion fromRot = origRot;
            UpdatePivotMatrix();

            if (showDebugInfo)
                Debug.Log($"Attach to hand");

            onHold?.Invoke(this);

            while (spawnInstance != null && (!isReleased && !isAttachTo))
            {
                var targetTransform = attachPosTrans;
                targetMatrix = targetTransform.localToWorldMatrix * pivotMatrix.inverse;
                targetPos = TransformExtensions.ExtractTranslationFromMatrix(ref targetMatrix);
                targetRot = TransformExtensions.ExtractRotationFromMatrix(ref targetMatrix);
                if (timer < 0)
                {
                    spawnInstanceTransform.SetPositionAndRotation(targetPos, targetRot);
                    break;
                }
                else
                {
                    float factor = timer / time;
                    targetPos = Vector3.Lerp(targetPos, fromPos, factor);
                    targetRot = Quaternion.Slerp(targetRot, fromRot, factor);
                    yield return null;
                    timer -= Time.deltaTime;
                }
            }

            isUseLateUpdate = false;

            // Hold and Wait for release
            Coroutine grabCoroutine = StartCoroutine(HoldCoroutine());
            yield return new WaitUntil(() => isReleased || isAttachTo);
            StopCoroutine(grabCoroutine);

            bool releaseFromAttachTo = false;

            // Attach to
            if (isAttachTo)
            {
                if (showDebugInfo)
                    Debug.Log($"Attach to slot");

                onAttach?.Invoke(this);
                    
                releaseFromAttachTo = true;

                if (hand != null)
                {
                    hand.Place(spawnInstance, spawnInstanceTransform, this);
                    hand = null;
                }

                fromPos = spawnInstanceTransform.position;
                fromRot = spawnInstanceTransform.rotation;

                timer = time;
                isUseLateUpdate = true;

                while (spawnInstance != null && (isAttachTo))
                {
                    var targetTransform = attachToTrans;
                    targetMatrix = targetTransform.localToWorldMatrix;
                    targetPos = TransformExtensions.ExtractTranslationFromMatrix(ref targetMatrix);
                    targetRot = TransformExtensions.ExtractRotationFromMatrix(ref targetMatrix);
                    if (timer < 0)
                    {
                        spawnInstanceTransform.SetPositionAndRotation(targetPos, targetRot);
                        break;
                    }
                    else
                    {
                        float factor = timer / time;
                        targetPos = Vector3.Lerp(targetPos, fromPos, factor);
                        targetRot = Quaternion.Slerp(targetRot, fromRot, factor);
                        yield return null;
                        timer -= Time.deltaTime;
                    }
                }

                if (attachToHideObject && spawnInstance != null)
                {
                    spawnInstance.SetActive(false);
                }

                attachToCallback?.Invoke();

                isUseLateUpdate = false;

                yield return new WaitUntil(() => !isAttachTo);
            }

            // From attach-to point back to hand (still grab)
            if (releaseFromAttachTo)
            {
                if (showDebugInfo)
                    Debug.Log($"Release from slot");

                onDetach?.Invoke(this);

                // Activate object
                if (spawnInstance != null)
                {
                    spawnInstance.SetActive(true);
                }

                fromPos = spawnInstanceTransform.position;
                fromRot = spawnInstanceTransform.rotation;

                // Attach again
                hand.Attach(spawnInstance, spawnInstanceTransform, this);

                timer = time;
                isUseLateUpdate = true;
                while (spawnInstance != null && (!isReleased))
                {
                    if (timer < 0)
                    {
                        targetPos = spawnInstanceTransform.position = attachPosTrans.position;
                        targetRot = spawnInstanceTransform.rotation = attachPosTrans.rotation;
                        break;
                    }
                    else
                    {
                        float factor = timer / time;
                        targetPos = Vector3.Lerp(attachPosTrans.position, fromPos, factor);
                        targetRot = Quaternion.Slerp(attachPosTrans.rotation, fromRot, factor);
                        yield return null;
                        timer -= Time.deltaTime;
                    }
                }
                isUseLateUpdate = false;

                grabCoroutine = StartCoroutine(HoldCoroutine());
                // Attach on hand & waiting until released
                yield return new WaitUntil(() => isReleased);
                StopCoroutine(grabCoroutine);
            }

            // Release
            isUseLateUpdate = true;
            timer = time;

            if (showDebugInfo)
                Debug.Log($"Release");

            onRelease?.Invoke(this);

            if (spawnInstance != null)
            {
                spawnInstance.SetActive(true);
            }

            fromPos = spawnInstanceTransform.position;
            fromRot = spawnInstanceTransform.rotation;

            while (spawnInstance != null)
            {
                if (timer < 0)
                {
                    targetPos = spawnInstanceTransform.position = origPos;
                    targetRot = spawnInstanceTransform.rotation = origRot;
                    break;
                }
                else
                {
                    float factor = timer / time;
                    targetPos = Vector3.Lerp(origPos, fromPos, factor);
                    targetRot = Quaternion.Slerp(origRot, fromRot, factor);
                    yield return null;
                    timer -= Time.deltaTime;
                }
            }

            isUseLateUpdate = false;

            // Detach from hand
            DetachAndClearReference();
            onReleaseDone?.Invoke(this);

            holdLateCoroutine = null;
        }

        private IEnumerator HoldCoroutine()
        {
            while (spawnInstance != null && hand != null)
            {
                hand.UpdateHolding(this);

                if (spawnEquipment != null)
                {
                    spawnEquipment.UpdateEquipment(hand);
                }
                yield return null;  // After Update() of MonoBehaviour
            }
        }

        private void LateUpdate()
        {
            if (!isUseLateUpdate)
                return;
            if (spawnInstanceTransform != null)
            {
                spawnInstanceTransform.position = targetPos;
                spawnInstanceTransform.rotation = targetRot;
            }
        }

        private void UpdatePivotMatrix()
        {
            if (spawnEquipment != null && spawnInstanceTransform != null)
            {
                pivotMatrix = Matrix4x4.identity;
                var pivotTrans = hand.IsLeft? spawnEquipment.PivotLeftTransform: spawnEquipment.PivotRightTransform;
                if (pivotTrans)
                {
                    pivotMatrix = spawnInstanceTransform.localToWorldMatrix.inverse * pivotTrans.localToWorldMatrix;
                }
            }
        }

        private void GetHandFromEventArgs(BaseInteractionEventArgs args)
        {
            handTransform = args.interactorObject.transform;
            var handParent = handTransform.parent;
            var currHand = handParent.GetComponent<IXRHand>();
            if (currHand != null && currHand.CanAttach())
            {
                attachPosTrans = currHand.AttachTransform;
                hand = currHand;
            }
        }

        private void DetachAndClearReference()
        {
            if (spawnInstance != null)
            {
                if (hand != null)
                {
                    hand.Detach(spawnInstance, spawnInstanceTransform, this);
                    hand = null;
                }
                else
                {
                    Destroy(spawnInstance);
                }
            }
            if (spawnEquipment != null)
            {
                spawnEquipment.Strip(hand);
            }

            // Recover intial state
            if (template != null)
                template.SetActive(true);

            // Enable all colliders
            foreach (var coll in colliders)
            {
                coll.enabled = true;
            }

            // Clean reference
            spawnInstance = null;
            spawnInstanceTransform = null;
            spawnEquipment = null;
        }

        #endregion
    }
}
