﻿using System;

namespace Project.Scripts.FSM.Predicate
{
    public class ActionPredicate : IPredicate {
        public bool flag;

        public ActionPredicate(ref Action eventReaction) => eventReaction += () => { flag = true; };

        public bool Evaluate() {
            bool result = flag;
            flag = false;
            return result;
        }
    }
}