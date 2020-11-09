using System;
using System.Collections.Generic;

namespace GameServerLib
{   
    /// <summary>
    /// Sent from server to client
    /// </summary>
    public class ServerPacket : Packet
    {
        public readonly int packetCode;

        public ServerPacket(int packetCode) : base((int)packetCode)
        {
            this.packetCode = packetCode;
        }
        public ServerPacket(byte[] recievedData) : base(recievedData)
        { }

        public int GetPacketCode()
        {
            return packetCode;
        }
    }
    /// <summary>
    /// Sent from client to server
    /// </summary>
    public class ClientPacket : Packet
    {
        public readonly int clientID;

        /// <summary>
        /// Creates client packet for sending it to server
        /// </summary>
        /// <param name="packetCode"></param>
        /// <param name="clientID"></param>
        public ClientPacket(int packetCode, int clientID) : base(packetCode)
        {
            this.clientID = clientID;
            Write(clientID);
        }
        /// <summary>
        /// Creates client packet from recieved packet
        /// </summary>
        /// <param name="recievedData"></param>
        /// <param name="clientID"></param>
        public ClientPacket(Packet packet) : base(packet.GetBytes())
        {
            clientID = packet.ReadInt();
        }
        /// <summary>
        /// Creates packet recieved from client
        /// </summary>
        /// <param name="recievedData"></param>
        /// <param name="clientID"></param>
        public ClientPacket(byte[] recievedData, int clientID) : base(recievedData)
        {
            Write(clientID);
        }

        public int GetPacketCode()
        {
            return packetCode;
        }
    }


    public class Packet
    {
        List<byte> buffer;
        int readPosition;
        public readonly int packetCode;

        /// <summary>
        /// Creates packet for sending
        /// </summary>
        /// <param name="packetCode"></param>
        public Packet(int packetCode)
        {
            buffer = new List<byte>();
            Write(packetCode);
        }
        /// <summary>
        /// Creates packet for receiving
        /// </summary>
        /// <param name="recievedData"></param>
        public Packet(byte[] recievedData)
        {
            buffer = new List<byte>();
            WriteBytes(recievedData);
            packetCode = ReadInt();
        }

        public byte[] GetBytes()
        {
            return buffer.ToArray();
        }

        #region Write
        public void WriteBytes(byte[] data)
        {
            buffer.AddRange(data);
        }
        public void Write(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
        public void Write(bool value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
        public void Write(string value)
        {
            byte[] writeBuffer = System.Text.Encoding.UTF8.GetBytes(value);
            Write(writeBuffer.Length);
            WriteBytes(writeBuffer);
        }
        #endregion

        #region Read
        public byte ReadByte()
        {
            if (buffer.Count - readPosition >= 1)
            {
                byte readBuffer = buffer[readPosition++];

                return readBuffer;
            }
            else
                throw new Exception("End of stream! Could not read byte!");
        }
        public byte[] ReadBytes(int length, bool moveReadPosition = true)
        {
            if (buffer.Count - readPosition >= length)
            {
                byte[] readBuffer = buffer.GetRange(readPosition, length).ToArray();
                if (moveReadPosition)
                    readPosition += length;

                return readBuffer;
            }
            else
                throw new Exception("End of stream! Could not read bytes!");
        }
        public int ReadInt(bool moveReadPosition = true)
        {
            return BitConverter.ToInt32(ReadBytes(sizeof(int), moveReadPosition), 0);
        }
        public bool ReadBoolean()
        {
            return BitConverter.ToBoolean(ReadBytes(1), 0);
        }
        public string ReadString()
        {
            int stringLength = ReadInt();
            return System.Text.Encoding.UTF8.GetString(ReadBytes(stringLength));
        }
        #endregion

    }

}