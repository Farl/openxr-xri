using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
    public interface IXREquipmentConstraint
    {
        bool IsLockConstraint => false;

        bool CheckConstraint(IXRHand hand, IXREquipment equipment);

        void ApplyConstraint(IXRHand hand, IXREquipment equipment);

        void RemoveConstraint(IXRHand hand, IXREquipment equipment);

        bool UpdateConstraint(IXRHand hand, IXREquipment equipment);

        bool LateUpdateConstraint(IXRHand hand, IXREquipment equipment);
    }
}