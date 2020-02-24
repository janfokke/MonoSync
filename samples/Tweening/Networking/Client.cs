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
        private SyncTargetRoot<Map> _gameWorldSyncRoot;
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
                var settings = SyncSourceSettings.Default;
                settings.TypeEncoder = new TweenGameTypeEncoder();
                settings.SourceFieldDeserializerResolverFactory = new TweenGameFieldSerializerFactory();
                _gameWorldSyncRoot.Update();
                if (_gameWorldSyncRoot.OwnTick % 60 == 0)
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
            binaryWriter.Write(_gameWorldSyncRoot.OwnTick);
            _stream.Write(memoryStream.ToArray());
        }

        public async void BeginReceiving()
        {
            while (true)
            {
                try
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
                        case Commands.SEND_RATE:
                        {
                            int sendRate = BitConverter.ToInt32(_buffer, 1);
                            _gameWorldSyncRoot.SendRate = 60 / sendRate;
                            break;
                        }
                    }
                }
                catch
                {
                    Environment.Exit(0);
                }
            }
        }

        private void InitializeSyncTargetRoot(byte[] worldData)
        {
            var settings = SyncTargetSettings.Default;
            settings.TypeEncoder = new TweenGameTypeEncoder();
            settings.TargetFieldDeserializerResolverFactory = new TweenGameFieldSerializerFactory();
            _gameWorldSyncRoot = new SyncTargetRoot<Map>(worldData, settings) {SendRate = 15};
            _connectionTaskCompletionSource.SetResult(_gameWorldSyncRoot.Root);
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