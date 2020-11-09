using GameServerLib.Tools;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServerLib
{
    /// <summary>
    /// Game's side client connecting to the server
    /// </summary>
    public class Client
    {
        private delegate void PacketHandler(Packet packet);
        readonly Dictionary<ServerPacket, PacketHandler> recievedPacketHandlers = new Dictionary<ServerPacket, PacketHandler>();

        private TcpClient client;
        private NetworkStream stream;
        
        public string Nickname { get; set; }
        public int ID { get; set; }
        public bool Connected { get; private set; }
        
        const int bufferSize = 2048;
        readonly byte[] recieveBuffer;
        
        public Client()
        {
            client = new TcpClient
            {
                ReceiveBufferSize = bufferSize,
                SendBufferSize = bufferSize,
                NoDelay = true
            };
            recieveBuffer = new byte[bufferSize];
            Connected = false;
        }
        /// <summary>
        /// Connect asynchronously
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task<bool> Connect(string hostname, int port)
        {
            if (hostname != "" && port > 0)
            {
                IAsyncResult result = client.BeginConnect(hostname, port, null, null);
                await Task.Run(() => result.AsyncWaitHandle.WaitOne(3000, false));
        
                if (client.Connected)
                {
                    stream = client.GetStream();
                    stream.BeginRead(recieveBuffer, 0, bufferSize, DataRecieveCallback, null);
                    Connected = true;
                    return true;
                }
                else
                {
                    client.Close();
                    return false;
                }
            }
            else
                return false;
        
        }
        private void DataRecieveCallback(IAsyncResult result)
        {
            try
            {
                int recievedLength = stream.EndRead(result);
                if (recievedLength <= 0)
                {
                    
                    return;
                }
        
                byte[] recievedData = new byte[recievedLength];
                Array.Copy(recieveBuffer, recievedData, recievedLength);
                HandlePacket(recievedData);
        
                stream.BeginRead(recieveBuffer, 0, bufferSize, DataRecieveCallback, null);
            }
            catch
            {

            }
        }
        private void HandlePacket(byte[] data)
        {
            Packet packet = new Packet(data);
            ServerPacket packetID = (ServerPacket)packet;
            recievedPacketHandlers[packetID](packet);
        }
        /// <summary>
        /// Sends packet to a server
        /// </summary>
        /// <param name="packet"></param>
        public async void SendPacket(Packet packet)
        {
            byte[] writeBuffer = packet.GetBytes();
        
            if (writeBuffer.Length > bufferSize)
                throw new Exception("Write buffer was too large to send!");
        
            await stream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
        }
    }
}
