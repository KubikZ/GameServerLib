using GameServerLib.Tools;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameServerLib
{
    /// <summary>
    /// Basic client implementation, only for establising connection and communication with server
    /// </summary>
    public class ServerClient
    {
        public readonly Server server;
        public readonly TcpClient tcpClient;

        private NetworkStream stream;

        public readonly int ID;
        public string Nickname { get; set; }
        public bool Connected { get; set; }

        private int bufferSize = 2048;
        private byte[] recieveBuffer;

        public ServerClient(TcpClient client, int id, Server server)
        {
            tcpClient = client;
            ID = id;
            this.server = server;

            client.ReceiveBufferSize = bufferSize;
            client.SendBufferSize = bufferSize;
            client.NoDelay = true;
            recieveBuffer = new byte[bufferSize];
            stream = client.GetStream();

            stream.BeginRead(recieveBuffer, 0, bufferSize, DataRecieveCallback, null);
        }


        public async virtual Task EstablishConnection()
        { 
            ServerPacket packet = new ServerPacket(0);
            packet.Write(ID);
            await SendPacket(packet);

            await Task.Run(async () =>
            {
                while (Nickname == "")
                    await Task.Delay(100);
            });
        }
        private void DataRecieveCallback(IAsyncResult result)
        {
            try
            {
                int recievedLength = stream.EndRead(result);
                if (recievedLength <= 0)
                {
                    Disconnect();
                    return;
                }

                byte[] recievedData = new byte[recievedLength];
                Array.Copy(recieveBuffer, recievedData, recievedLength);

                ClientPacket recievedPacket = new ClientPacket(recievedData, ID);
                server.HandlePacket(recievedPacket);
                
                Debug.Log($"Read packet. Bytes read: {recievedData.Length}, packet code: {recievedPacket.GetPacketCode()}");

                stream.BeginRead(recieveBuffer, 0, bufferSize, DataRecieveCallback, null);
            }
            catch (Exception e)
            {
                Debug.Log("Error when recieving data! Error: " + e.Message);
                Disconnect();
            }
        }
        public async Task SendPacket(ServerPacket packet)
        {
            byte[] writeBuffer = packet.GetBytes();
            if (writeBuffer.Length > bufferSize)
                throw new Exception("Write buffer was too large to send!");
            try
            {
                await stream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                Debug.Log($"Sent packet to client {ID}. Bytes written: {writeBuffer.Length}, pakcet code: {packet.GetPacketCode()}");
            }
            catch
            {
                Debug.Log("Failed to send data to client!");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            tcpClient.Close();
            // OnServerError
        }
    }
}