using UnityEngine;
using System.Collections.Generic;

using LiteNetLib;
using LiteNetLib.Utils;

using Assets.Scripts;
using Zinnia.Tracking.Follow;



public enum NetworkTags
{
    NT_S_Receiv_Player_Position,
    NT_S_Receiv_Player_Orientation,
    //NT_S_Receiv_Object_Position,
    NT_S_Send_Players_Pos_Array
}

class Globals
{
    public static string IP = "localhost";
    public static int Port = 15000;
    public static string Key = "Server_app_key";
}

public class Network : MonoBehaviour, INetEventListener {

    private NetManager _netClient;

    private NetDataWriter dataWriter;
    private Dictionary<long, NetPlayer> netPlayersDictionary;
    private Queue<NetworkData> m_networkData = new Queue<NetworkData>();

    [SerializeField]
    private Broker m_broker;

    
    public ObjectFollower HeadSet;
    public ObjectFollower LeftController;
    public ObjectFollower RightController;
    public GameObject NetHead;
    public GameObject NetLeftController;
    public GameObject NetRightController;

    private Vector3 lastNetworkedPosition_head = Vector3.zero;
    private Vector3 lastNetworkedPosition_left = Vector3.zero;
    private Vector3 lastNetworkedPosition_right = Vector3.zero;
    private Quaternion lastNetworkedRotation_head = Quaternion.identity; //neu
    private Quaternion lastNetworkedRotation_left = Quaternion.identity; //neu
    private Quaternion lastNetworkedRotation_right = Quaternion.identity; //neu

    private float lastDistance_head = 0.0f;
    private float lastDistance_left = 0.0f;
    private float lastDistance_right = 0.0f;
    private float lastAngle_head = 0.0f; // neu
    private float lastAngle_left = 0.0f; // neu
    private float lastAngle_right = 0.0f; // neu

    const float MIN_DISTANCE_TO_SEND_POSITION = 0.01f; 
    const float MIN_ANGLE_TO_SEND_POSITION = 0.01f; //neu

    private NetPeer serverPeer;

    private List<NetworkData> m_outData = new List<NetworkData>();

    public void OnPeerConnected(NetPeer peer)
    {
        serverPeer = peer;
        //Debug.Log($"OnPeerConnected: {peer.EndPoint.Host} : {peer.EndPoint.Port}");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        //Debug.Log($"OnPeerConnected: {peer.EndPoint.Host} : {peer.EndPoint.Port} Reason: {disconnectInfo.Reason.ToString()}");
    }

    public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
    {
        //Debug.LogError($"OnNetworkError: {socketErrorCode}");
    }

    public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
    {
        // Debug.Log("OnNetworkReceive");
        if (reader.Data == null)
            return;

        if (reader.Data.Length >= 0)
        {
            NetworkTags networkTag = (NetworkTags)reader.GetInt();
            if (networkTag == NetworkTags.NT_S_Send_Players_Pos_Array)
            {
                while(reader.Position < reader.AvailableBytes)
                {
                    long playerid = reader.GetLong();
                    int byteLength = reader.GetInt();

                    var bytes = new byte[byteLength];
                    reader.GetBytes(bytes, byteLength);
                    var data = DataConverter.ToNetworkData(bytes);
                    foreach (var networkData in data.NetworkData)
                    {
                        if (string.IsNullOrEmpty(networkData.GUID))
                        {
                            if (!netPlayersDictionary.ContainsKey(playerid))
                                netPlayersDictionary.Add(playerid, new NetPlayer());

                            netPlayersDictionary[playerid].ReceivedData.Push(networkData);
                        }
                        else
                            m_networkData.Enqueue(networkData);
                    }
                }
            }
            reader.Clear();
        }
    }

    public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
    {
        //Debug.Log($"OnNetworkReceive: {reader.Data.Length}");
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
       // Debug.Log("OnNetworkLatencyUpdate");
    }

    public void SendNetworkData(NetworkData data)
    {
        m_outData.Add(data);
    }

    void Start()
    {
        _netClient = new NetManager(this, Globals.Key);
        _netClient.Start();
        _netClient.UpdateTime = 15;

        netPlayersDictionary = new Dictionary<long, NetPlayer>();
        dataWriter = new NetDataWriter();

       // Debug.Log("_netClient.IsRunning OK !");

        _netClient.Connect(Globals.IP, Globals.Port);

       // Debug.Log("netClient Connect !");
    }

    private void Update()
    {
        _netClient.PollEvents();

        var peer = _netClient.GetFirstPeer();

        if (peer != null && peer.ConnectionState == ConnectionState.Connected)
        {
            //Headsetbewegungen prüfen
            lastDistance_head = Vector3.Distance(lastNetworkedPosition_head, HeadSet.transform.position);
            var deltaRotation_head = Quaternion.Inverse(lastNetworkedRotation_head) * HeadSet.transform.rotation; // neu

            var mag_head = deltaRotation_head.eulerAngles.magnitude; //Berechnen des Betrags des Euler-Winkels

            //Linker Controller Bewegungen prüfen
            lastDistance_left = Vector3.Distance(lastNetworkedPosition_left, LeftController.transform.position);
            var deltaRotation_left = Quaternion.Inverse(lastNetworkedRotation_left) * LeftController.transform.rotation; // neu

            var mag_left = deltaRotation_head.eulerAngles.magnitude; //Berechnen des Betrags des Euler-Winkels

            //Rechter Controller Bewegungen prüfen 
            lastDistance_right = Vector3.Distance(lastNetworkedPosition_right, RightController.transform.position);
            var deltaRotation_right = Quaternion.Inverse(lastNetworkedRotation_right) * RightController.transform.rotation; // neu

            var mag_right = deltaRotation_head.eulerAngles.magnitude; //Berechnen des Betrags des Euler-Winkels


            if (lastDistance_head >= MIN_DISTANCE_TO_SEND_POSITION || mag_head >= MIN_ANGLE_TO_SEND_POSITION) // neu
            {
                var data_head = new NetworkData()
                {
                    Position = HeadSet.transform.position,
                    Rotation = HeadSet.transform.rotation,
                    GUID = "",
                    Part = 1
                };


                SendNetworkData(data_head);

                lastNetworkedPosition_head = HeadSet.transform.position;
                lastNetworkedRotation_head = HeadSet.transform.rotation;

            }

            if (lastDistance_left >= MIN_DISTANCE_TO_SEND_POSITION || mag_left >= MIN_ANGLE_TO_SEND_POSITION)
           {
                var data_left = new NetworkData()
                {
                    Position = LeftController.transform.position,
                    Rotation = LeftController.transform.rotation,
                    GUID = "",
                    Part = 2
                };

                SendNetworkData(data_left);

                lastNetworkedPosition_left = LeftController.transform.position;
                lastNetworkedRotation_left = LeftController.transform.rotation;
                
           }

            if(lastDistance_right >= MIN_DISTANCE_TO_SEND_POSITION || mag_right >= MIN_ANGLE_TO_SEND_POSITION)
            {
                var data_right = new NetworkData()
                {
                    Position = RightController.transform.position,
                    Rotation = RightController.transform.rotation,
                    GUID = "",
                    Part = 3
                };

                SendNetworkData(data_right);

                lastNetworkedPosition_right = RightController.transform.position;
                lastNetworkedRotation_right = RightController.transform.rotation;
            }
            
        }
        else
        {
            // _netClient.SendDiscoveryRequest(new byte[] { 1 }, Globals.Serveur_Port);
           // Debug.Log("peer = null");
        }

        foreach (var player in netPlayersDictionary)
        {
            if (!player.Value.GameObjectAdded)
            {
                player.Value.GameObjectAdded = true;
                player.Value.m_GameObject_head = Instantiate(NetHead, Vector3.zero, Quaternion.identity);
                player.Value.m_GameObject_left = Instantiate(NetLeftController, Vector3.zero, Quaternion.identity);
                player.Value.m_GameObject_right = Instantiate(NetRightController, Vector3.zero, Quaternion.identity);
            }

            else
                player.Value.Apply();
        }

        Debug.Log(m_networkData.Count);
        while(m_networkData.Count > 0)
        {
            m_broker.Apply(m_networkData.Dequeue());
        }

        if (m_outData.Count > 0)
            SendNetworkData();
    }

    private void SendNetworkData()
    {
        var state = new NetworkState()
        {
            NetworkData = m_outData.ToArray()
        };

        dataWriter.Reset();
        dataWriter.Put((int)NetworkTags.NT_S_Receiv_Player_Position);
        dataWriter.Put(DataConverter.ToByte(state));
        serverPeer?.Send(dataWriter, SendOptions.Sequenced);

        m_outData.Clear();
    }
    
    private void OnApplicationQuit()
    {
        if (_netClient != null)
            if (_netClient.IsRunning)
                _netClient.Stop();
    }

}
