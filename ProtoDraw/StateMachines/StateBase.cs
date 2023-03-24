using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectNXAML.Model;

namespace DirectNXAML.StateMachines
{/// <summary>
 /// Concrete State Interface
 /// </summary>
    public interface IStateBase
    {
        /// <summary>
        /// 状態での処理の中身を書き、続行可能であれば通常trueを返す
        /// もしくは
        /// 状態処理の開始条件（普段なら状態遷移条件）を判定し、可能ならtrueを返す。この場合はGetNextState内で状態での処理の中身を書く
        /// </summary>
        /// <returns></returns>
        bool DoStateWork();
        /// <summary>
        /// 次の状態を返す
        /// もしくは
        /// 状態での処理の中身を書いて次の状態を返す
        /// </summary>
        /// <returns></returns>
        string GetNextStateID();
    }

    /// <summary>
    /// 状態クラス
    /// 実際の駆動をイベントで行うか、メソッドで行うかも実装次第。
    /// staticの共有オブジェクト（例えばメインフォームやグローバル変数クラス）が欲しいなら、ここが、その継承にstaticで追加しても良い？
    /// これを継承する状態は基本的にstatic/singletonを前提にしている。つまり、遷移の際にnewしない。
    /// </summary>
    abstract public class StateBase : IStateBase, IDisposable
    {
        /// <summary>
        /// Universal singleton fields
        /// </summary>
        protected static UIBroker the;

        /// <summary>
        /// 状態名表示などに使う固有文字列ワーク 設定ほぼ必須
        /// </summary>
        public string StateID
        {
            get;
            protected set;
        }
        public int StateLayer
        {
            get;
            protected set;
        }

        // LogWrite三点セット
        public delegate void LogWriteHandler(string message, bool overwrite = false);
        protected static LogWriteHandler LogHandle;
        protected static void LogWrite(string message, bool overwrite = false)
        {
            LogHandle?.Invoke(message, overwrite);
        }

        /// <summary>
        /// この状態の処理を行う（引数無しイベントベース）
        /// 異なる状態、正確には異なるStateIDを持つ状態から遷移してきた時に走る処理
        /// </summary>
        public delegate void OnEnterStateHandler();
        public OnEnterStateHandler OnEnter;
        protected void StateEnter()
        {
            OnEnter?.Invoke();
        }

        /// <summary>
        /// 他の状態に遷移する時、最後に走る処理
        /// </summary>
        public delegate void OnExitStateHandler();
        public OnExitStateHandler OnLeave;
        protected void StateExit()
        {
            OnLeave?.Invoke();
        }

        /// <summary>
        /// この状態の処理を行う本体。（メソッドベース）自己に遷移する場合は毎回走る処理
        /// 他の状態に遷移する時はtureを返す。インストール不十分などのプログラム続行不能エラーは例外発火。
        /// 自己ループする時はfalseを返す。その場合、DoStateWork()のみが繰り返される。
        /// これを単に遷移条件の判定とし、GetNextState()にその状態での動作を書いても良い
        /// </summary>
        /// <returns>true if NOT FATAL error</returns>
        abstract public bool DoStateWork();

        /// <summary>
        /// 自分自身も含め、次の遷移状態を判定する（メソッドベース）
        /// DoStateWork()を遷移条件の判定のみに用いた場合、ここにその状態での動作を書き、次の遷移先も決定して抜ける
        /// </summary>
        /// <returns>次の状態</returns>
        abstract public string GetNextStateID();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public StateBase()
        {
            //if (the == null) the = UIBroker.Instance;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~StateBase()
        {
            this.Dispose(false);    // IntPtrなど、unmanagedをdisposeする起点
        }
        /// <summary>
        /// 明示的に呼ぶDispose
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);         // trueで明示的であることを示す
            GC.SuppressFinalize(this);  // 全リソースが破棄されているので、ガベージコレクタが走ってnullアクセスするのを防ぐ
        }
        /// <summary>
        /// disposeしたことを記憶するフラグ
        /// protectedなDispose()メソッドを何回呼んでも一回しか走らせない
        /// </summary>
        private bool disposed = false;
        /// <summary>
        /// このクラスが継承したクラスがDispose()を持つためのprotected
        /// 継承先のクラスからこのクラスのDispose()をコールする。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!this.disposed)     // 未開放の場合
                {
                    // unmanagedリソースがあれば解放する。フィールドとかIntPtr ptr;
                    // if (ptr != IntPtr.Zero) {
                    //		Marshal.FreeHGlobal(ptr);
                    //		ptr = IntPtr.Zero; } とか。

                    if (disposing)  // 明示的にマネージから呼ばれた場合
                    {
                        // managedリソースを解放する。何かのstreamがnullじゃなかったらClose()してnull埋めとか。
                    }
                    this.disposed = true;   // 解放したことを覚えておく
                }
            }
            finally
            {
                //base.Dispose(disposing);	// 基底クラスがある場合(overrideして作った場合)
            }
        }

        /// <summary>
        /// 次状態ID保持ワーク（必要に応じて使う）
        /// </summary>
        protected string NextStateID { get; set; } = "";

        /// <summary>
        /// プログラム終了予約 StateReservedより優先
        /// 状態遷移マシンをまたぐのでstatic
        /// setは様々な状態遷移マシンの状態で。参照時は最上位であるMasterまで持って行く（消さない）。
        /// </summary>
        protected static bool ReserveEnd { get; set; } = false;

        /// <summary>
        /// ホールドした時の状態格納領域
        /// ホールドした瞬間にいた状態を保存する
        /// これはステートマシン毎に存在するので、使用するオブジェクトはステートマシンを超えてはならない。
        /// </summary>
        protected string StateHeld { get; set; } = "";
        protected void ClearStateHeld()
        {
            StateHeld = "";
        }

        /// <summary>
        /// 予約された遷移先情報
        /// </summary>
        protected string StateReserved { get; set; } = "";
        protected void ClearStateReserved()
        {
            StateReserved = "";
        }

        /// <summary>
        /// 状態遷移イベントハンドラー
        /// </summary>
        public virtual void OnStateEnter() { }
        public virtual void OnStateExit() { }
    }
}
