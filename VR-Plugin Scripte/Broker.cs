using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts;

namespace Assets.Scripts
{
    public class Broker : MonoBehaviour
    {
        [SerializeField]
        private Network m_network;

        private TransformSync[] m_transformSyncElement;

        public void Start()
        {
            m_transformSyncElement = GameObject.FindObjectsOfType<TransformSync>();
        }

        public void Update()
        {
            foreach (var item in m_transformSyncElement)
            {
                if (item.HasChanged)
                {
                    m_network.SendNetworkData(new NetworkData()
                    {
                        Position = item.transform.position,
                        Rotation = item.transform.rotation,
                        GUID = item.GUID,
                        Part = 0
                    });

                    item.ResetTransform();
                }
            }
        }

        public void Apply(NetworkData networkData)
        {
            var match = m_transformSyncElement.FirstOrDefault(s => s.GUID == networkData.GUID);
            if (match == null)
                return;

            match.transform.position = networkData.Position;
            match.transform.rotation = networkData.Rotation;
            match.ResetTransform();
        }

    }
}
