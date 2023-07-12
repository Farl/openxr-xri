using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
    public class AnimatorParameterController : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private AnimatorControllerParameterType parameterType;
        [SerializeField]
        private string parameter;

        void OnEnable()
        {
            if (animator)
            {
                switch (parameterType)
                {
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameter, true);
                        break;
                }
            }
        }

        private void OnDisable()
        {
            if (animator)
            {
                switch (parameterType)
                {
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameter, false);
                        break;
                }
            }
        }
    }
}
