using Project.Scripts.FSM.Predicate;
using Project.Scripts.FSM.State;

namespace Project.Scripts.FSM.Transitions
{
    public class Transition : ITransition
    {
        public IState To { get; }
        public IPredicate Condition { get; }

        public Transition(IState to, IPredicate condition)
        {
            To = to;
            Condition = condition;
        }
    }
}