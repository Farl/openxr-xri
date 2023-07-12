using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace SS
{
    public class StateMachine<T> where T : struct, IConvertible
    {
        #region Public
        public delegate void EnterStateDelegate(StateMachine<T> stateMachine, T from, T to);
        public delegate void LeaveStateDelegate(StateMachine<T> stateMachine, T from, T to);
        public delegate void UpdateStateDelegate(StateMachine<T> stateMachine);
        public delegate bool CanEnterStateDelegate(StateMachine<T> stateMachine, T from, T to);

        public T CurrentState { get; private set; }
        public T PreviousState { get; private set; }

        public CanEnterStateDelegate OnCanEnterState;

        public StateMachine(CanEnterStateDelegate onCanEnter = null)
        {
            CurrentState = default(T);
            PreviousState = CurrentState;
            OnCanEnterState += onCanEnter;
            _CurrentStateInfo = new StateInfo() { state = CurrentState };
            _StateMap.Add(CurrentState, _CurrentStateInfo);
        }

        public float Timer
        {
            get { return _StateTimer; }
            set { _StateTimer = value; }
        }

        public void AddStates(
            EnterStateDelegate enterAction = null,
            LeaveStateDelegate leaveAction = null,
            UpdateStateDelegate updateAction = null, params T[] states)
        {
            foreach (var state in states)
            {
                AddState(state, enterAction, leaveAction, updateAction);
            }
        }


        public void AddState(T state,
            EnterStateDelegate enterAction = null,
            LeaveStateDelegate leaveAction = null,
            UpdateStateDelegate updateAction = null)
        {
            StateInfo si;
            if (!_StateMap.TryGetValue(state, out si))
            {
                si = new StateInfo() { state = state };
                _StateMap.Add(state, si);
            }
            if (si != null)
            {
                if (enterAction != null)
                    si.enterAction += enterAction;
                if (leaveAction != null)
                    si.leaveAction += leaveAction;
                if (updateAction != null)
                    si.updateAction += updateAction;
            }
        }

        public void SetNextState(T nextState)
        {
            if (_IsStateChanging)
            {
                if (_NextStateQueue != null)
                    Debug.LogError("There's already a queueing state.");
                else
                {
                    _NextStateQueue = nextState;
                }
                return;
            }

            if (OnCanEnterState != null)
            {
                if (!OnCanEnterState.Invoke(this, CurrentState, nextState))
                {
                    return;
                }
            }
            StateInfo nextStateInfo = CheckCanEnter(nextState);
            if (nextStateInfo != null)
            {
                _IsStateChanging = true;
                _NextState = nextState;

                Leave(nextState, nextStateInfo);
                Enter(nextState, nextStateInfo);
                _StateTimer = 0;

                PreviousState = CurrentState;
                CurrentState = _NextState;

                _CurrentStateInfo = nextStateInfo;
                _IsStateChanging = false;
            }
            else
            {
                Debug.LogError($"Missing state {nextState.ToString()}");
            }

            if (_NextStateQueue != null)
            {
                var queuedNextState = _NextStateQueue;
                _NextStateQueue = null;
                SetNextState((T)queuedNextState);
            }
        }

        public void Update()
        {
            if (_CurrentStateInfo != null && _CurrentStateInfo.updateAction != null)
            {
                _CurrentStateInfo.updateAction(this);
                _StateTimer += Time.deltaTime;
            }
        }

        public override string ToString()
        {
            return $@"{GetType().ToString()} = {CurrentState}
Prev = {PreviousState}, Next = {_NextState}";
        }
        #endregion

        #region Private

        private T _NextState { get; set; }

        private class StateInfo
        {
            public T state;
            public EnterStateDelegate enterAction;
            public LeaveStateDelegate leaveAction;
            public UpdateStateDelegate updateAction;
        }

        private Dictionary<T, StateInfo> _StateMap = new Dictionary<T, StateInfo>();
        private T? _NextStateQueue = null;
        private StateInfo _CurrentStateInfo;
        private StateInfo _NextStateInfo;
        private float _StateTimer = 0;
        private bool _IsStateChanging = false;

        private StateInfo CheckCanEnter(T nextState)
        {
            if (_StateMap.TryGetValue(nextState, out var si))
            {
                return si;
            }
            return null;
        }

        private void Leave(T nextState, StateInfo nextStateInfo)
        {
            if (_CurrentStateInfo != null && _CurrentStateInfo.leaveAction != null)
            {
                _CurrentStateInfo.leaveAction(this, CurrentState, _NextState);
            }
        }

        private void Enter(T _nextState, StateInfo nextStateInfo)
        {
            if (nextStateInfo != null && nextStateInfo.enterAction != null)
            {
                _NextStateInfo = nextStateInfo;
                _NextStateInfo.enterAction(this, CurrentState, _NextState);
            }
        }

        #endregion

    }
}
