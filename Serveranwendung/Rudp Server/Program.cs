using System;
using System.Collections.Generic;
//using System.Net;//pour Com R-UDP LiteNetLib 8
//using System.Net.Sockets;//pour Com R-UDP LiteNetLib 8
using LiteNetLib;//pour Com R-UDP LiteNetLib 7
using LiteNetLib.Utils;//pour Com R-UDP LiteNetLib 7
using System.Threading;//pour Threading

#region Ex
#endregion Ex

public enum NetworkTags
{
    NT_S_Receiv_Player_Position,
    NT_S_Receiv_Object_Position,
    NT_S_Send_Players_Pos_Array
}

class Globals
{
    public static string IP = "localhost";
    public static int Port = 15000;
    public static string Key = "Server_app_key";
    public static int MaxPlayer = 10;
}

namespace Rudp_Server
{
    public class ServerPlayers_Pose
    {
        public NetPeer NetPeer { get; set; }
        public byte[] Bytes { get; set; }

        public ServerPlayers_Pose(NetPeer peer_Id)
        {
            NetPeer = peer_Id;
        }
    }

    class Program : INetEventListener
    {
        private NetManager _netManager;

        private Dictionary<long, ServerPlayers_Pose> _dictionary_Server_Players_Pose;

        NetDataWriter _netDataWriter;

        #region EventListener

        public void OnPeerConnected(NetPeer _netPeer)
        {
            try
            {
                Console.WriteLine($"OnPeerConnected == Client connected _netPeer.ConnectId No[ {_netPeer.ConnectId} ]-->_netPeer.EndPoint {_netPeer.EndPoint}");
                Console.WriteLine($"_netPeer.EndPoint.Host: {_netPeer.EndPoint.Host} _netPeer.EndPoint.Port {_netPeer.EndPoint.Port}");

                if (!_dictionary_Server_Players_Pose.ContainsKey(_netPeer.ConnectId))
                    _dictionary_Server_Players_Pose.Add(_netPeer.ConnectId, new ServerPlayers_Pose(_netPeer));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPeerConnected Error: {ex.Message}");
            }
        }

        public void OnNetworkReceive(NetPeer _netPeer, NetDataReader _netDataReader)
        {
            try
            {
                if (_netDataReader.Data == null)
                    return;

                NetworkTags networkTag = (NetworkTags)_netDataReader.GetInt();
                if (networkTag == NetworkTags.NT_S_Receiv_Player_Position)
                {
                    var bytes = new byte[_netDataReader.Data.Length - sizeof(int)];
                    _netDataReader.GetBytes(bytes, bytes.Length);
                    _dictionary_Server_Players_Pose[_netPeer.ConnectId].Bytes = bytes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnNetworkReceive Error: {ex.Message}");
            }
        }

        public void OnPeerDisconnected(NetPeer _netPeer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"[Server] Peer disconnected: " + _netPeer.ConnectId + ", reason: " + disconnectInfo.Reason);
            try
            {
                Console.WriteLine($"OnPeerDisconnected: {_netPeer.EndPoint.Host} : {_netPeer.EndPoint.Port} Reason: {disconnectInfo.Reason.ToString()}");

                if (_dictionary_Server_Players_Pose.ContainsKey(_netPeer.ConnectId))
                    _dictionary_Server_Players_Pose.Remove(_netPeer.ConnectId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPeerDisconnected Error: {ex.Message}");
            }
        }

        public void OnNetworkError(NetEndPoint _netendPoint, int socketErrorCode)
        {
            try
            {
                Console.WriteLine($"OnNetworkError: {socketErrorCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnNetworkError Error: {ex.Message}");
            }
        }

        public void OnNetworkReceiveUnconnected(NetEndPoint _netendPoint, NetDataReader _netDataReader, UnconnectedMessageType messageType)
        {
            try
            {
                Console.WriteLine($"OnNetworkReceiveUnconnected");

                if (messageType == UnconnectedMessageType.DiscoveryRequest)
                {
                    _netManager.SendDiscoveryResponse(new byte[] { 1 }, _netendPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnNetworkReceiveUnconnected Error: {ex.Message}");
            }
        }

        public void OnNetworkLatencyUpdate(NetPeer _netPeer, int latency)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnNetworkLatencyUpdate Error: {ex.Message}");
            }
        }

        #endregion EventListener

        public void SendPlayerPositions()
        {
            try
            {
                Dictionary<long, ServerPlayers_Pose> _dictionary_send_to_Players = new Dictionary<long, ServerPlayers_Pose>(_dictionary_Server_Players_Pose);

                int To_Player = 0;
                foreach (var send_To_Player in _dictionary_send_to_Players)
                {
                    if (send_To_Player.Value == null)
                    {
                        Console.WriteLine($"Player Absent No: {To_Player}");
                        continue;
                    }
                    To_Player += 1;

                    _netDataWriter.Reset();
                    _netDataWriter.Put((int)NetworkTags.NT_S_Send_Players_Pos_Array);

                    int amountPlayersMoved = 0;

                    foreach (var Players_Pos in _dictionary_send_to_Players)
                    {
                        if (send_To_Player.Key == Players_Pos.Key)
                            continue;

                        if (Players_Pos.Value.Bytes == null)
                            continue;

                        _netDataWriter.Put(Players_Pos.Key);
                        _netDataWriter.Put(Players_Pos.Value.Bytes.Length);
                        _netDataWriter.Put(Players_Pos.Value.Bytes);
                        amountPlayersMoved++;
                    }

                    if (amountPlayersMoved > 0)
                        send_To_Player.Value.NetPeer.Send(_netDataWriter, SendOptions.ReliableOrdered);
                }

                foreach (var item in _dictionary_send_to_Players)
                    item.Value.Bytes = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendPlayerPositions Error: {ex.Message}");
            }
        }

        public void Run()
        {
            try
            {
                _dictionary_Server_Players_Pose = new Dictionary<long, ServerPlayers_Pose>();

                _netManager = new NetManager(this, Globals.MaxPlayer, Globals.Key);

                _netDataWriter = new NetDataWriter();

                if (_netManager.Start(Globals.Port))
                    Console.WriteLine($"{Globals.MaxPlayer} player NetManager started listening on port {Globals.Port}");
                else
                {
                    Console.WriteLine("Server cold not start!");
                    return;
                }

                while (_netManager.IsRunning)
                {
                    _netManager.PollEvents();

                    SendPlayerPositions();

                    /*System.Threading.*/
                    Thread.Sleep(15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Run Error: {ex.Message}");
            }
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
            Console.ReadKey();
        }
    }
}