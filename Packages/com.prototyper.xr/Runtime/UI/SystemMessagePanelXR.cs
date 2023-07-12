using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS
{
    public class SystemMessagePanelXR : SystemMessagePanel
    {
        public override bool Setup(SystemMessageUI.RequestData requestData)
        {
            XRHandBase.RegisterRayInteractor(name, true);
            return base.Setup(requestData);
        }

        public override void Hide(SystemMessageUI.RequestData requestData, bool keepRequest = false)
        {
            base.Hide(requestData, keepRequest);
            XRHandBase.RegisterRayInteractor(name, false);
        }

        void OnDestroy()
        {
            XRHandBase.RegisterRayInteractor(name, false);
        }
    }

}