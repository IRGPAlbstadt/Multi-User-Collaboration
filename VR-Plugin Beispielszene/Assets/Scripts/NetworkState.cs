using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    [Serializable]
    [ProtoContract]
    public class NetworkState
    {
        [ProtoMember(1)]
        public NetworkData[] NetworkData;
    }
}
