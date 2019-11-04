using System.Collections.Generic;
using UnityEngine;

namespace VegetationGenerator
{
    // Note: this state code was created by myself for a different project and reused here

    public enum StateTransitionResult
    {
        STACK_PUSH,//Enters new state, no exit functions are executed
        STACK_SWAP,//Exits current state, enters new state
        STACK_POP, //Exits current state, no enter functions are executed
        NO_ACTION  //Self-explanatory
    }

    public struct StateTransition
    {
        public StateTransition(System.Func<MonoBehaviour, State, StateTransitionResult> transition)
        {
            TargetState = null;
            TransitionActions = new List<System.Action>();
            Transition = transition;
        }

        public State TargetState;
        public List<System.Action> TransitionActions;
        public System.Func<MonoBehaviour, State, StateTransitionResult> Transition;
    }

    public abstract class State
    {

        public List<StateTransition> Transitions { get; set; } = new List<StateTransition>();

        public abstract void Enter(MonoBehaviour origin);
        public abstract void Exit(MonoBehaviour origin);
        public abstract void Update(MonoBehaviour origin);

        public KeyValuePair<StateTransitionResult, StateTransition> CheckTransitions(MonoBehaviour origin)
        {
            foreach (StateTransition transition in Transitions)
            {
                StateTransitionResult result = transition.Transition(origin, this);
                if (result == StateTransitionResult.NO_ACTION) continue;
                return new KeyValuePair<StateTransitionResult, StateTransition>(result, transition);
            }
            return new KeyValuePair<StateTransitionResult, StateTransition>(StateTransitionResult.NO_ACTION, new StateTransition());
        }
    }

    public class StateMachine
    {
        private StateMachine() { }

        public StateMachine(MonoBehaviour origin, State startingState)
        {
            _origin = origin;
            _stateStack.Push(startingState);
            startingState.Enter(origin);
        }

        private Stack<State> _stateStack = new Stack<State>();
        private MonoBehaviour _origin = null;

        public void Update()
        {
            State currentState = _stateStack.Peek();
            KeyValuePair<StateTransitionResult, StateTransition> transitionResult = currentState.CheckTransitions(_origin);

            //Order of execution: Exit, Actions, Enter, Update
            switch (transitionResult.Key)
            {
                case StateTransitionResult.NO_ACTION:
                    break;

                case StateTransitionResult.STACK_POP:
                    currentState.Exit(_origin);
                    _stateStack.Pop();
                    foreach (System.Action action in transitionResult.Value.TransitionActions)
                    {
                        action();
                    }
                    currentState = _stateStack.Peek();
                    break;

                case StateTransitionResult.STACK_PUSH:
                    _stateStack.Push(transitionResult.Value.TargetState);
                    foreach (System.Action action in transitionResult.Value.TransitionActions)
                    {
                        action();
                    }
                    currentState = transitionResult.Value.TargetState;
                    currentState.Enter(_origin);
                    break;

                case StateTransitionResult.STACK_SWAP:
                    currentState.Exit(_origin);
                    _stateStack.Pop();
                    _stateStack.Push(transitionResult.Value.TargetState);
                    foreach (System.Action action in transitionResult.Value.TransitionActions)
                    {
                        action();
                    }
                    currentState = transitionResult.Value.TargetState;
                    currentState.Enter(_origin);
                    break;
            }
            currentState.Update(_origin);
        }
    }
}
