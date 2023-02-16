using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectNXAML.Model;
using y2Lib;

namespace DirectNXAML.StateMachines
{
    public class StateContextBase
    {
        protected enum StateBaseIDEnum : int
        {
            none = -1,
            Master = 0,
            Driving,
            Robot,
            Sensing,
            maxEnum
        }
        protected StateBaseIDEnum ID { get; set; }

        protected UIBroker the;
        private StateBase StateNow { get; set; } = null;
        private StateBase PrevState { get; set; } = null;
        private StateBase NextState { get; set; } = null;
        protected int m_MachineLayer;

        /// <summary>
        /// 自己ループも含む状態遷移の度に呼ばれる共通処理
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public delegate void OnEveryTransitEvent(object obj, MessageIntEventArgs e);
        public event OnEveryTransitEvent OnEveryTransit;
        /// <summary>
        /// 他の状態に変わる遷移の度に呼ばれる共通処理
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public delegate void OnChangeTransitEvent(object obj, MessageIntEventArgs e);
        public event OnChangeTransitEvent OnChangeTransit;
        /// <summary>
        /// 最後の状態遷移を終え、FSMから抜ける直前に走る処理
        /// </summary>
        public delegate void OnExitFSMHandler(StateBaseIDEnum id);
        public OnExitFSMHandler OnExitFSM;

        /// <summary>
        /// IDから状態を特定するディクショナリー
        /// </summary>
        internal Dictionary<string, StateBaseIDEnum> IdStateDic;
        /// <summary>
        /// Lock用オブジェクト（ワーク）
        /// </summary>
        static object Locker = new object();

        /// <summary>
        /// 外部から強制的に状態を設定する
        /// 開始時など
        /// </summary>
        /// <param name="state"></param>
        public void SetCurrentState(StateBaseIDEnum state, int layer)
        {
            if (the == null)
                the = GlobalBroker.Instance;
            m_MachineLayer = layer + 1;
            string pre = (StateNow == null) ? "" : StateNow.StateID;
            string next = (state == null) ? "" : state.StateID;
            if ((next == "") || !IdStateDic.ContainsKey(next))
            {
                throw new InvalidStateTransition("setting state null or not in dictionary at MasterState.");
            }
            if (pre != next)
            {
                OnEveryTransit?.Invoke(this, new MessageIntEventArgs());
                StateNow?.OnLeave?.Invoke();
                PrevState = StateNow;
                OnChangeTransit?.Invoke(this, new MessageIntEventArgs(state.StateID, m_MachineLayer));
                StateNow = state;
                StateNow.OnEnter?.Invoke();
            }
            else
            {
                // ここを「入り直し」と見なせばOnExit, OnChangeTransit, OnEnterをもう一回
                // やることになり、pre!=nextと同じだが、プログラムエラーの可能性が大きいので。
                the.LogWrite("Warning:100-10: force to transit same state. Taken no action.");
            }
        }
        public void BeginTransit(GUIStateBase start_state, int layer)
        {
            SetCurrentState(start_state, layer);
            Transit();
        }
        public async Task BeginTransitAsync(GUIStateBase start_state, int layer)
        {
            SetCurrentState(start_state, layer);
            //await Task.Run(() => SetCurrentState(start_state)); // Transitを実行するというタスクを開始し、完了まで待つ
            await Task.Run(() => Transit());    // Transitを実行するというタスクを開始し、完了まで待つ
                                                // なのでこれは順次実行だけど、BeginTransitとの違いは戻り値。戻り値がTaskなので上位から呼び出すときにawaitできる。
        }
        /// <summary>
        /// メソッドベースの状態遷移
        /// </summary>
        public void Transit()
        {
            string pre, next;
            // ループ前、最初の状態のみの処理
            if (StateNow == null)
            {
                throw new InvalidStateTransition("StateNow is null at MasterStateContext.");
            }

            while (StateNow != null)
            {
                lock (Locker)
                {
                    OnEveryTransit?.Invoke(this, new MessageIntEventArgs());    // 自己遷移も含め毎回呼び出されるもの。遷移予約の確認とか？

                    if (StateNow.DoStateWork()) // 状態の制御の本体
                    {
                        pre = StateNow.StateID;
                        next = StateNow.GetNextStateID();   // これは要らんかったかも
                        try
                        {
                            if ((next != null) && (next != ""))
                            {
                                if (pre != next)    // 遷移決定
                                {
                                    PrevState = IdStateDic[StateNow.StateID];
                                    StateNow = IdStateDic[next];
                                    PrevState.OnLeave?.Invoke();
                                    OnChangeTransit?.Invoke(this, new MessageIntEventArgs(StateNow.StateID, m_MachineLayer));
                                    StateNow.OnEnter?.Invoke();
                                }
                            }
                            else
                            {
                                // null or nothing 最後のステートの後処理
                                StateNow.OnLeave?.Invoke(); // 一応現状態はnullじゃないので
                                OnChangeTransit?.Invoke(this, new MessageIntEventArgs("", m_MachineLayer)); // 一応nullに遷移するわけなので
                                StateNow = null;    // これで抜ける
                            }
                        }
                        catch (System.ComponentModel.InvalidAsynchronousStateException e)
                        {
                            ;   // もはやFormControlMain()のスレッドは無い
                        }
                        catch (Exception e)
                        {
                            the.LogWrite($"Error:100-10: invalid state transition from \"{pre}\" to \"{next}\".\n" + e.Message);
                            throw new InvalidStateTransition(e.Message + $"\npre: {pre}, next: {next}");
                        }
                    }
                    else
                    {
                        // falseが返ると自己ループ。でもStateNowがnullだと抜ける
                        ;
                    }
                }
            }
            OnExitFSM?.Invoke(ID);
        }
    }
}
