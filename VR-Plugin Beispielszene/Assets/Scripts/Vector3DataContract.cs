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
    public class Vector3DataContract
    {
        [ProtoMember(1)]
        public float x;
        [ProtoMember(2)]
        public float y;
        [ProtoMember(3)]
        public float z;

        public static implicit operator Vector3DataContract(Vector3 vector)
        {
            return new Vector3DataContract()
            {
                x = vector.x,
                y = vector.y,
                z = vector.z
            };
        }
        public static implicit operator Vector3(Vector3DataContract vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }
    }
}
