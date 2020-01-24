using System;
using System.IO;
using System.Net.Sockets;
using Microsoft.Xna.Framework;

namespace MonoSync.Sample.Tweening
{
    internal class ServerSideClient
    {
        private readonly byte[] _buffer = new byte[512];
        private readonly Action<Vector2> _clickHandler;
        private readonly TcpClient _client;
        private readonly NetworkStream _networkStream;

        public ServerSideClient(TcpClient client, int playerId, Action<Vector2> clickHandler)
        {
            _client = client;
            _clickHandler = clickHandler;
            PlayerId = playerId;
            _networkStream = client.GetStream();
        }

        public int PlayerId { get; }
        public int Tick { get; set; }

        public event EventHandler Disconnected;

        public async void BeginReceiving()
        {
            while (true)
            {
                try
                {
                    int length = await _networkStream.ReadAsync(_buffer, 0, _buffer.Length);
                    if (length == 0)
                    {
                        Disconnect();
                        return;
                    }
                    if (_buffer[0] == Commands.CLICK_COMMAND)
                    {
                        var x = BitConverter.ToSingle(_buffer, 1);
                        var y = BitConverter.ToSingle(_buffer, 5);
                        _clickHandler(new Vector2(x, y));
                    }
                    else if (_buffer[0] == Commands.TICK_UPDATE)
                    {
                        // Refine Tick
                        Tick = BitConverter.ToInt32(_buffer, 1);
                    }
                }
                catch
                {
                    Disconnect();
                    return;
                }
            }
        }

        private void Disconnect()
        {
            _client.Close();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void SendWorld(byte[] worldData)
        {
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte) Commands.WORLD_DATA);
            binaryWriter.Write(worldData);
            _networkStream.Write(memoryStream.ToArray());
        }

        public void NotifySendRate(in int newSendRate)
        {
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(Commands.SEND_RATE);
            binaryWriter.Write(newSendRate);
            _networkStream.Write(memoryStream.ToArray());
        }
    }
}