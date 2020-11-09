using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace GameServerLib
{
    /// <summary>
    /// Basic server class used to establish connetion with clients and take care of all "low level" neworking stuff
    /// </summary>
    public class Server
    {
        private TcpListener tcpListener;
        /// <summary>
        /// Port that server is running on
        /// </summary>
        public int Port { get; private set; }
        static bool waitForPlayers;

        /// <summary>
        /// Clients connected to the server, where key is client's ID
        /// </summary>
        public Dictionary<int, ServerClient> clients = new Dictionary<int, ServerClient>();

        public Dictionary<int, ServerPacketHandler> packetHandlers;

        /// <summary>
        /// Create server with packet handlers for packet receiving
        /// </summary>
        /// <param name="packetHandlers"></param>
        public Server(Dictionary<int, ServerPacketHandler> packetHandlers)
        {
            this.packetHandlers = packetHandlers;
        }

        /// <summary>
        /// Starts waiting for clients
        /// </summary>
        /// <param name="port"></param>
        public async void StartWaitingForClients(int port)
        {
            Port = port;
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            waitForPlayers = true;

            while (waitForPlayers)
            {
                ServerClient connectedClient = new ServerClient(await tcpListener.AcceptTcpClientAsync(), clients.Count, this);


                await connectedClient.EstablishConnection();
                clients.Add(clients.Count, connectedClient);
                // More advanced cancelation of waiting
                // for now waiting can be stopped only after new client connects
            }
        }
        public static void StopWaitingForClients()
        {
            waitForPlayers = false;
        }
        /// <summary>
        /// Sends packet to client with specified ID
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="ID"></param>
        public async void SendPacketToClient(ServerPacket packet, int ID)
        {
            await clients[ID].SendPacket(packet);
        }
        public async void SendPacketToAllClients(ServerPacket packet)
        {
            for (int i = 0; i < clients.Count; i++)
                await clients[i].SendPacket(packet);
        }
        internal void HandlePacket(ClientPacket packet)
        {
            ClientPacket recievedPacket = new ClientPacket(packet);
            packetHandlers[recievedPacket.packetCode](clients[packet.clientID], packet);
        }

        internal void SendPacket(TcpClient client, ServerPacket packet)
        {

        }
    }

}