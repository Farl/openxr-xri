using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS;

public class XRSequenceActionEquipmentSlotGroup : MonoBehaviour
{
    [SerializeField] private XRSequenceActionEquipmentSlot[] slots;

    private int currentSlotIndex = 0;
    
    void Awake()
    {
        bool isFirst = true;
        for (var i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot != null)
            {
                slot.group = this;
                if (isFirst)
                {
                    slot.gameObject.SetActive(true);
                    isFirst = false;
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }
    }

    public void OnAttach()
    {
        // Activate next and deactive current slot
        slots[currentSlotIndex].gameObject.SetActive(false);

        currentSlotIndex = currentSlotIndex + 1;
        while (currentSlotIndex < slots.Length && slots[currentSlotIndex] == null)
        {
            currentSlotIndex++;
        }
        if (currentSlotIndex < slots.Length)
        {
            slots[currentSlotIndex].gameObject.SetActive(true);
        }
    }

    void OnDestroy()
    {

    }
}
