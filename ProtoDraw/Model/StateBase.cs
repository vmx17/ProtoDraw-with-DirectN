using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectNXAML.Model
{
    public abstract class StateMachine
    {
        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; }
        protected abstract State GetInitialState();
        public StateMachine()
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
        // call everytime
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
        protected void ChangeToInitialState()
        {
            var initial_state = this.GetInitialState();
            this.ChangeState(initial_state);
        }
    }

}
