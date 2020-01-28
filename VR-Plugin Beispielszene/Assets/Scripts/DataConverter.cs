using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ProtoBuf;
using System.IO;

namespace Assets.Scripts
{
    public static class DataConverter
    {
        public static Byte[] ToByte(NetworkState data)
        {
            
            using (MemoryStream stream = new MemoryStream())
             {
              Serializer.Serialize(stream, data);
              return stream.ToArray();
             }
             
            
            //string str = JsonUtility.ToJson(data);
           // return Encoding.UTF8.GetBytes(str);
        }

        public static NetworkState ToNetworkData(Byte[] bytes)
        {
            NetworkState newNetworkdata;
            using (var stream = new MemoryStream(bytes))
            {
                newNetworkdata = Serializer.Deserialize<NetworkState>(stream);
                return newNetworkdata;
            }
            //string str = Encoding.UTF8.GetString(bytes);
            //return JsonUtility.FromJson<NetworkData>(str);
        }
    }
}
