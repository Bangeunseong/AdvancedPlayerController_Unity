using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.FSM.Predicate;
using Project.Scripts.FSM.State;
using Project.Scripts.FSM.Transitions;

namespace Project.Scripts.FSM
{
    public class StateMachine
    {
        private StateNode current;
        private Dictionary<Type, StateNode> nodes = new();
        private HashSet<ITransition> anyTransitions = new();

        public IState CurrentState => current.State;
        
        public void Update()
        {
            var transition = GetTransition();
            if (transition != null)
            {
                ChangeState(transition.To);
                // If i use action predicate for stateMachine, then it needs to be reset
                // foreach (var node in nodes.Values)
                //     ResetActionPredicateFlags(node.Transitions);
                // ResetActionPredicateFlags(anyTransitions);
            }
            
            current.State?.Update();
        }

        public void FixedUpdate()
        {
            current.State?.FixedUpdate();
        }

        public void SetState(IState state)
        {
            current = nodes[state.GetType()];
            current.State?.OnEnter();
        }
        
        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }

        public void AddAnyTransition(IState to, IPredicate condition)
        {
            anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
        }
        
        private void ChangeState(IState state)
        {
            if (state == current.State) return;
            
            var previous = current.State;
            var next = nodes[state.GetType()].State;
            
            previous?.OnExit();
            next?.OnEnter();
            current = nodes[state.GetType()];
        }

        private ITransition GetTransition()
        {
            foreach(var transition in anyTransitions)
                if (transition.Condition.Evaluate()) 
                    return transition;
            
            foreach(var transition in current.Transitions)
                if (transition.Condition.Evaluate()) 
                    return transition;
            
            return null;
        }
        
        private StateNode GetOrAddNode(IState state)
        {
            var node = nodes.GetValueOrDefault(state.GetType());

            if (node == null)
            {
                node = new StateNode(state);
                nodes.Add(state.GetType(), node);
            }

            return node;
        }

        private class StateNode
        {
            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }

            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public void AddTransition(IState to, IPredicate condition)
            {
                Transitions.Add(new Transition(to, condition));
            }
        }
    }
}