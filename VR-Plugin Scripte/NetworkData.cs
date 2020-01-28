using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    [ProtoContract]
    public class NetworkData
    {
        [ProtoMember(1)]
        public Vector3DataContract Position;
        [ProtoMember(2)]
        public QuaternionDataContract Rotation;
        [ProtoMember(3)]
        public string GUID;
        [ProtoMember(4)]
        public int Part;
    }
}
