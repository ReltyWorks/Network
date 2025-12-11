/*
* 이 코드는 자동생성된 코드이므로 편집하지 마시오.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.NetworkContracts
{
    public enum PacketId : ushort
    {
        S_ConnectionSuccess = 0x0001,
        S_ConnectionFailure = 0x0002,
        S_ChatSend = 0x1010,
        C_ChatSend = 0x1020,
    }
}
