using System;
using System.IO;
using System.Net.Sockets;
using Microsoft.Xna.Framework;

namespace MonoSync.Sample.Tweening
{
    internal class ServerSideClient
    {
        private readonly byte[] _buffer = new byte[512];
        private readonly TcpClient _client;
        public  Player Player { get; }
        private readonly NetworkStream _networkStream;

        public ServerSideClient(TcpClient client, Player player)
        {
            _client = client;
            Player = player;
            _networkStream = client.GetStream();
        }

     
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
                    switch (_buffer[0])
                    {
                        case Commands.CLICK_COMMAND:
                        {
                            var x = BitConverter.ToSingle(_buffer, 1);
                            var y = BitConverter.ToSingle(_buffer, 5);
                            Player.TargetPosition = new Vector2(x, y);
                            break;
                        }
                        case Commands.TICK_UPDATE:
                            // Refine Tick
                            Tick = BitConverter.ToInt32(_buffer, 1);
                            break;
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
            binaryWriter.Write(Commands.WORLD_DATA);
            binaryWriter.Write(worldData);
            _networkStream.Write(memoryStream.ToArray());
        }
    }
}