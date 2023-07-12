
namespace SS
{
    public interface IChooseGroup
    {
        public string name => null;

        public void Choose(IChoose choose, bool notify);

        public void Register(IChoose choose);

        public void Unregister(IChoose choose);
    }

}