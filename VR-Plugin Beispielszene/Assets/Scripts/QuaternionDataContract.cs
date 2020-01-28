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
    public class QuaternionDataContract
    {
        [ProtoMember(1)]
        public float x;
        [ProtoMember(2)]
        public float y;
        [ProtoMember(3)]
        public float z;
        [ProtoMember(4)]
        public float w;

        public static implicit operator QuaternionDataContract(Quaternion quaternion)
        {
            return new QuaternionDataContract()
            {
                x = quaternion.x,
                y = quaternion.y,
                z = quaternion.z,
                w = quaternion.w
            };
        }
        public static implicit operator Quaternion(QuaternionDataContract quat)
        {
            return new Quaternion(quat.x, quat.y, quat.z, quat.w);
        }
    }
}
