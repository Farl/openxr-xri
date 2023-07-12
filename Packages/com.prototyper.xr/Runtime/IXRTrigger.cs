
namespace SS
{
    public interface IXRTrigger
    {
        public string name => null;

        public System.Action<IXRTrigger> onActivate { get; set; }
        public System.Action<IXRTrigger> onDeactivate { get; set; }

        public void OnEnter(TriggerHoverEnterArgs args);
        public void OnLeave(TriggerHoverExitArgs args);

        public void OnActivate(TriggerActivateEventArgs args);

        public void OnDeactivate(TriggerDeactivateEventArgs args);
        public void OnSelectEnter(TriggerSelectEnterArgs args);
        public void OnSelectExit(TriggerSelectExitArgs args);
    }

}