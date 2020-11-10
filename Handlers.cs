using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerLib
{
    /// <summary>
    /// Handler delegate for server incoming packets
    /// </summary>
    /// <param name="packet"></param>
    public delegate void ClientPacketHandler(ServerPacket packet);

    /// <summary>
    /// Handler delegate for client incoming packets
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="packet"></param>
    public delegate void ServerPacketHandler(int clientID, ClientPacket packet);

}