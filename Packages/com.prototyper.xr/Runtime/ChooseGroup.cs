using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
    public abstract class ChooseGroup : MonoBehaviour, IChooseGroup
    {
        public abstract void Choose(IChoose choose, bool notify);

        public abstract void Register(IChoose choose);

        public abstract void Unregister(IChoose choose);
    }
}
