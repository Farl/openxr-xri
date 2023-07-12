using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
    public class MultiSelectCheck : MonoBehaviour, ICondition
    {
        public List<GameObject> checkList = new List<GameObject>();
    }
}
