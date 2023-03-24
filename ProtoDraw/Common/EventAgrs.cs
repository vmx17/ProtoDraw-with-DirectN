using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace y2Lib
{
    public class BoolEventArgs : EventArgs
    {
        public bool Result { get; private set; } = false;
        public BoolEventArgs(bool result)
        {
            Result = result;
        }
    }
    public delegate void BoolEventHandler(object sender, BoolEventArgs e);

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; private set; } = "";
        public MessageEventArgs(string _msg)
        {
            Message = _msg;
        }
    }
    public delegate void MessageEventHandler(object sender, MessageEventArgs e);

    public class MessageIntColorEventArgs : EventArgs
    {
        public string Message { get; set; } = "";
        public int Number { get; set; } = 0;
        public Color Color { get; set; } = default;
        public MessageIntColorEventArgs(string _mes = "", int _num = 0, Color _color = default)
        {
            Color = _color;
            Message = _mes;
            Number = _num;
        }
    }
    public delegate void MMessageIntColorHandler(object sender, MessageIntColorEventArgs e);

    public class IntMessageIntColorEventArgs : EventArgs
    {
        public int Id { get; set; } = 0;
        public string Message { get; set; } = "";
        public int Number { get; set; } = 0;
        public Color Color { get; set; } = default;
        public IntMessageIntColorEventArgs(int _id = 0, string _mes = "", int _num = 0, Color _color = default)
        {
            Id = _id;
            Color = _color;
            Message = _mes;
            Number = _num;
        }
    }
    public delegate void IntMessageIntColorEventHandler(object sender, IntMessageIntColorEventArgs e);

    public class MessageIntEventArgs : EventArgs
    {
        public string Message { get; set; } = "";
        public int Number { get; set; } = 0;
        public MessageIntEventArgs(string _mes = "", int _num = 0)
        {
            Message = _mes;
            Number = _num;
        }
    }
    public delegate void MessageIntEventHandler(object sender, MessageIntEventArgs e);

    public class InitializeProgressBarEventArgs : EventArgs
    {
        public int Id = 0;
        public int Initial = 0;
        public int Max = 100;
        public int Min = 0;
        public Color Color = Color.LightGreen;
        public InitializeProgressBarEventArgs(int _id, int _max = 100, int _min = 0, int _init = 0, Color _color = default)
        {
            Id = _id;
            Initial = _init;
            Min = _min;
            Max = _max;
            Color = (_color == default) ? Color.LightGreen : _color;
        }
    }
    public delegate void InitializeProgressBarEventHandler(object sender, InitializeProgressBarEventArgs e);
}
