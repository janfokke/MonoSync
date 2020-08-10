using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoSync.Sample.Tweening
{
    public class Client : SimpleGameComponent
    {
        private readonly byte[] _buffer = new byte [1024];
        private readonly TcpClient _tcpClient;
        private TargetSynchronizerRoot<Map> _gameWorldSyncRoot;
        private NetworkStream _stream;
        private TaskCompletionSource<Map> _connectionTaskCompletionSource;

        public Client()
        {
            _tcpClient = new TcpClient();
        }

        public async Task<Map> Join()
        {
            _connectionTaskCompletionSource = new TaskCompletionSource<Map>();

            await _tcpClient.ConnectAsync(IPAddress.Loopback, 1234);
            _stream = _tcpClient.GetStream();
            BeginReceiving();
            return await _connectionTaskCompletionSource.Task;
        }

        public override void Update(GameTime gameTime)
        {
            if (_gameWorldSyncRoot != null)
            {
                _gameWorldSyncRoot.Update();
                if (_gameWorldSyncRoot.Clock.OwnTick % 60 == 0)
                {
                    // Sending tick every second to refine precision on the server
                    SendTick();
                }
            }
        }

        private void SendTick()
        {
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte) Commands.TICK_UPDATE);
            binaryWriter.Write(_gameWorldSyncRoot.Clock.OwnTick);
            _stream.Write(memoryStream.ToArray());
        }

        public async void BeginReceiving()
        {
            while (true)
            {
                
                    int count = await _stream.ReadAsync(_buffer, 0, _buffer.Length);
                    if (count == 0)
                    {
                        Environment.Exit(0);
                    }
                    
                    switch (_buffer[0])
                    {
                        case Commands.WORLD_DATA:
                        {
                            byte[] worldData = _buffer[1..];
                            if (_gameWorldSyncRoot == null)
                            {
                                InitializeSyncTargetRoot(worldData);
                                SendTick();
                            }
                            else
                            {
                                _gameWorldSyncRoot.Read(worldData);
                            }

                            break;
                        }
                    }
                
            }
        }

        private void InitializeSyncTargetRoot(byte[] worldData)
        {
            _gameWorldSyncRoot = new TargetSynchronizerRoot<Map>(worldData);
            _gameWorldSyncRoot.Settings.Serializers.AddSerializer(new ColorSerializer());
            _gameWorldSyncRoot.Settings.Serializers.AddSerializer(new Vector2Serializer());
            _connectionTaskCompletionSource.SetResult(_gameWorldSyncRoot.Reference);
        }

        public void SendMouseClick(Vector2 position)
        {
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte) Commands.CLICK_COMMAND);
            binaryWriter.Write(position.X);
            binaryWriter.Write(position.Y);
            _stream.Write(memoryStream.ToArray());
        }
    }
}