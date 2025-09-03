using Project.Scripts.FSM.Predicate;
using Project.Scripts.FSM.State;

namespace Project.Scripts.FSM.Transitions
{
    public interface ITransition
    {
        IState To { get; }
        IPredicate Condition { get; }
    }
}