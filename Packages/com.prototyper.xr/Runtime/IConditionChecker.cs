
namespace SS
{
    public interface IConditionChecker
    {
        public bool Check(ICondition condition, bool partialCheck);
        public void Execute();
    }
}
