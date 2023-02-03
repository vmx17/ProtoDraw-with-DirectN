using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This code is based on: https://an-embedded-engineer.hateblo.jp/entry/2019/04/21/150550
namespace DirectNXAML.Model
{
    public abstract class CADInputStateMachine
    {
        // current state
        public State CurrentState { get; private set; }
        // previous state
        public State PreviousState { get; private set; }
        // get initial state just after state machine started
        protected abstract State GetInitialState();
        // change to the initial state
        protected void ChangeToInitialState()
        {
            var initial_state = this.GetInitialState();
            this.ChangeState(initial_state);
        }
        public CADInputStateMachine()
        {
        }
        public void SendTrigger(Trigger trigger)
        {
            this.CurrentState?.SendTrigger(this, trigger);
        }
        public void ChangeState(State new_state, Effect effect = null)
        {
            if (this.CurrentState != new_state)
            {
                var old_state = this.CurrentState;
                this.CurrentState?.ExecuteExitAction(this);
                this.CurrentState = new_state;
                this.PreviousState = old_state;
                effect?.Execute(this);
                this.CurrentState?.ExecuteEntryAction(this);
            }
        }
        public void Update()
        {
            this.CurrentState?.ExecuteDoAction(this);
        }
        public T GetAs<T>() where T : StateMachine
        {
            if (this is T stm)
            {
                return stm;
            }
            else
            {
                throw new InvalidOperationException($"State Machine is not {nameof(T)}");
            }
        }
    }
}
