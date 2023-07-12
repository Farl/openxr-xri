using UnityEngine;

namespace SS
{
    public interface IChoose
    {
        public bool IsOn => false;
        public bool SetIsOn(bool newIsOn, bool notify);
        public void SetIsOn(bool newIsOn, bool notify, IChooseGroup chooseGroup);

        public string name { get; }
        public GameObject gameObject { get; }
    }

}