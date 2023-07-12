using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
    public class MultiSelectController : ChooseGroup, IConditionChecker
    {
        [SerializeField] private int maxSelectCount = 2;
        [SerializeField] private string soundEventSelected = @"Select";
        [SerializeField] private string soundEventDeselected = @"Deselect";

        private HashSet<IChoose> chooseMap = new HashSet<IChoose>();

        private List<IChoose> chooseList = new List<IChoose>();

        #region Public

        public bool Check(ICondition condition, bool partialCheck)
        {
            MultiSelectCheck check = condition as MultiSelectCheck;
            if (check == null)
                return false;
            return Check(check, partialCheck);
        }

        public void Execute()
        {
            ClearSelect();
        }

        public override void Choose(IChoose interactable, bool notify)
        {
            // Check if already choose
            if (chooseList.Contains(interactable))
            {
                // Unselect
                chooseList.Remove(interactable);
                interactable.SetIsOn(false, notify, this);
                SoundSystem.PlayOneShot(soundEventDeselected);
                return;
            }

            chooseList.Add(interactable);
            if (chooseList.Count > maxSelectCount)
            {
                // Select too much, unselect first one
                chooseList[0].SetIsOn(false, notify, this);
                chooseList.RemoveAt(0);
            }

            // Choose
            interactable.SetIsOn(true, notify, this);

            if (chooseList.Count == maxSelectCount)
            {
                if (SequenceManager.CheckConditionChecker(this, checkOnly: false))
                {
                }
            }
            else
            {
                if (!SequenceManager.CheckConditionChecker(this, checkOnly: true))
                {
                    ClearSelect();
                }
                else
                {
                    SoundSystem.PlayOneShot(soundEventSelected);
                }
            }
        }

        public override void Register(IChoose interactable)
        {
            if (!chooseMap.Contains(interactable))
                chooseMap.Add(interactable);
        }

        public override void Unregister(IChoose interactable)
        {
            chooseMap.Remove(interactable);
        }


        #endregion

        private bool PartialCheck(MultiSelectCheck check)
        {
            var hashSet = new HashSet<IChoose>(chooseList);

            foreach (var go in check.checkList)
            {
                if (go != null)
                {
                    var comp = go.GetComponent<IChoose>();
                    if (comp != null && hashSet.Contains(comp))
                        return true;
                    hashSet.Remove(comp);
                }
            }

            return false;
        }

        private bool Check(MultiSelectCheck check, bool partialCheck)
        {
            if (partialCheck)
                return PartialCheck(check);

            if (check.checkList.Count != chooseList.Count)
                return false;

            var hashSet = new HashSet<IChoose>(chooseList);

            foreach (var go in check.checkList)
            {
                if (go != null)
                {
                    var comp = go.GetComponent<IChoose>();
                    if (comp == null || !hashSet.Contains(comp))
                        return false;
                    hashSet.Remove(comp);
                }
            }

            return hashSet.Count == 0;
        }

        private void ClearSelect()
        {
            foreach (var c in chooseList)
            {
                if (c != null)
                {
                    c.SetIsOn(false, true, this);
                }
            }
            chooseList.Clear();
        }
        private void OnValidate()
        {
            if (maxSelectCount <= 0)
                maxSelectCount = 1;
        }
    }
}
