using UnityEngine;
using Assets.Scripts;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class NetPlayer : IGUIDProvider
    {
        // Properties
        public string GUID => new System.Guid().ToString();
        public Stack<NetworkData> ReceivedData { get; set; } = new Stack<NetworkData>();



        // Members
        public GameObject m_GameObject_head;
        public GameObject m_GameObject_left;
        public GameObject m_GameObject_right;

        //public GameObject NetHead;
        //public GameObject NetLeftController;
        //public GameObject NetRightController;
        
        public bool GameObjectAdded { get; set; }                                                                                       

        // Constructor
        public NetPlayer()
        {
            GameObjectAdded = false; 
            //m_GameObject_head = GameObject.Instantiate(NetHead, Vector3.zero, Quaternion.identity);
            //m_GameObject_left = GameObject.Instantiate(NetLeftController, Vector3.zero, Quaternion.identity);
            //m_GameObject_right = GameObject.Instantiate(NetRightController, Vector3.zero, Quaternion.identity);
        }

        // Methods
        public void Apply()
        {

            while(ReceivedData.Count > 0)
            {
                var data = ReceivedData.Pop();

                if (data == null)
                    return;

                if (data.Part == 1)
                {
                    m_GameObject_head.transform.position = data.Position;
                    m_GameObject_head.transform.rotation = data.Rotation;
                }

                if (data.Part == 2)
                {
                    m_GameObject_left.transform.position = data.Position;
                    m_GameObject_left.transform.rotation = data.Rotation;
                }

                if (data.Part == 3)
                {
                    m_GameObject_right.transform.position = data.Position;
                    m_GameObject_right.transform.rotation = data.Rotation;
                }
            }

            //m_GameObject_head.transform.position = ReceivedData.Position;
            //m_GameObject_left.transform.position = ReceivedData.Position;
            //m_GameObject_right.transform.position = ReceivedData.Position;
            //m_GameObject_head.transform.rotation = ReceivedData.Rotation;
            //m_GameObject_left.transform.rotation = ReceivedData.Rotation;
            //m_GameObject_right.transform.rotation = ReceivedData.Rotation;
            //ReceivedData = null;
        }
    }
}