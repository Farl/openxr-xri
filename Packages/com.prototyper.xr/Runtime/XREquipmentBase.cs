using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
    public partial class XREquipmentBase : MonoBehaviour, IXREquipment
    {
        [SerializeField] private string equipmentName = string.Empty;
        [SerializeField] private List<Component> disableOnSpawn;
        [SerializeField] private Transform pivotRightTransform;
        [SerializeField] private Transform pivotLeftTransform;

        public virtual string OriginalEquipmentName => (!string.IsNullOrEmpty(equipmentName)) ? equipmentName : name;
        public virtual string EquipmentName => (!string.IsNullOrEmpty(equipmentName))? equipmentName: name;
        public virtual bool IsMatchEquipment(string equipmentName) => EquipmentName == equipmentName;
        public Transform PivotRightTransform => pivotRightTransform;
        public Transform PivotLeftTransform => pivotLeftTransform;
        public Transform cachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                    _cachedTransform = transform;
                return _cachedTransform;
            }
        }
        public virtual void UpdateEquipment(IXRHand hand) { }
        public virtual void Equip(IXRHand hand) { }
        public virtual void Strip(IXRHand hand) { }

        public void BeforeSpawn()
        {
            foreach (var comp in disableOnSpawn)
            {
                EnableComponent(comp, false);
            }
        }

        public void AfterSpawn()
        {
            foreach (var comp in disableOnSpawn)
            {
                EnableComponent(comp, true);
            }
        }

        private Transform _cachedTransform;

        private void EnableComponent(Component comp, bool value)
        {
            var behaviour = comp as Behaviour;
            if (behaviour)
            {
                behaviour.enabled = value;
                return;
            }
            var collider = comp as Collider;
            if (collider)
            {
                collider.enabled = value;
                return;
            }
            var renderer = comp as Renderer;
            if (renderer)
            {
                renderer.enabled = value;
                return;
            }
        }
    }

}