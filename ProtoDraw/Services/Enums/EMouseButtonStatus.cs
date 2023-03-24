using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectNXAML.Services.Enums
{
    public enum EMouseButtonStatus : uint
    {
        none =      0,
        LeftOn =    0b1,
        MiddleOn =  0b10,
        RightOn =   0b100,
        maxEnum =   0xFFFFFFFF
    }
}
