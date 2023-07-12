using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
    public interface IXREquipment
    {
        string EquipmentName => null;
        string OriginalEquipmentName => null;
        bool IsMatchEquipment(string equipmentName);
        Transform PivotLeftTransform => null;
        Transform PivotRightTransform => null;

        void BeforeSpawn();
        void AfterSpawn();
        void Equip(IXRHand hand);
        void Strip(IXRHand hand);
        void UpdateEquipment(IXRHand hand);
    }

}