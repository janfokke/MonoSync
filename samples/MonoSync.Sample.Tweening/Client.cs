using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoSync.SyncSource;
using MonoSync.SyncTarget;

namespace MonoSync.Sample.Tweening
{
    public class Client : SimpleGameComponent
    {
        private readonly byte[] _buffer = new byte [1024];
        private readonly TcpClient _tcpClient;
        private Action<Map> _connectCallback;
        private SyncTargetRoot<Map> _gameWorldSyncRoot;
        private NetworkStream _stream;

        public Client()
        {
            _tcpClient = new TcpClient();
        }

        public void Connect(Action<Map> connectedCallback)
        {
            _connectCallback = connectedCallback;
            _tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, 1234));
            _stream = _tcpClient.GetStream();
            Receive();
        }

        public override void Update(GameTime gameTime)
        {
            if (_gameWorldSyncRoot != null)
            {
                var settings = SyncSourceSettings.Default;
                settings.TypeEncoder = new TweenGameTypeEncoder();
                settings.FieldDeserializerResolverFactory = new TweenGameFieldSerializerFactory();
                _gameWorldSyncRoot.Update();
                if (_gameWorldSyncRoot.OwnTick % 60 == 0)
                {
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

        public void Receive()
        {
            _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int count = _stream.EndRead(ar);
                if (count == 0)
                {
                    Environment.Exit(0);
                }

                if (_buffer[0] == Commands.WORLD_DATA)
                {
                    byte[] worldData = _buffer[1..];
                    if (_gameWorldSyncRoot == null)
                    {
                        var settings = SyncTargetSettings.Default;
                        settings.TypeEncoder = new TweenGameTypeEncoder();
                        settings.FieldDeserializerResolverFactory = new TweenGameFieldSerializerFactory();
                        _gameWorldSyncRoot = new SyncTargetRoot<Map>(worldData, settings);
                        _gameWorldSyncRoot.SendRate = 15;
                        _connectCallback(_gameWorldSyncRoot.Root);
                        SendTick();
                    }
                    else
                    {
                        _gameWorldSyncRoot.Read(worldData);
                    }
                }

                Receive();
            }
            catch
            {
                Environment.Exit(0);
            }
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