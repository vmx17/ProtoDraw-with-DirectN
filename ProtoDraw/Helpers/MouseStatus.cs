using DirectNXAML.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectNXAML.Helpers
{
    public static class MouseStatus
    {
        public static uint GetButtons(bool left, bool middle, bool right)
        {
            uint mouse_button_status = (uint)EMouseButtonStatus.none;
            mouse_button_status |= (uint)(left ? EMouseButtonStatus.LeftOn : EMouseButtonStatus.none);
            mouse_button_status |= (uint)(middle ? EMouseButtonStatus.MiddleOn : EMouseButtonStatus.none);
            mouse_button_status |= (uint)(right ? EMouseButtonStatus.RightOn : EMouseButtonStatus.none);
            return mouse_button_status;
        }
        public static bool IsLeftOn(uint status)
        {
            return (status &= (uint)EMouseButtonStatus.LeftOn) != 0;
        }
        public static bool IsMiddleOn(uint status)
        {
            return (status &= (uint)EMouseButtonStatus.MiddleOn) != 0;
        }
        public static bool IsRightOn(uint status)
        {
            return (status &= (uint)EMouseButtonStatus.RightOn) != 0;
        }
    }
}
