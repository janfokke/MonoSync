using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoSync.Sample.Tweening
{
    public class Server : SimpleGameComponent
    {
        private readonly List<ServerSideClient> _clients = new List<ServerSideClient>();
        private readonly SourceSynchronizerRoot _gameWorldSyncSourceRoot;
        private readonly List<ServerSideClient> _newClients = new List<ServerSideClient>();
        private readonly TcpListener _tcpListener;
        
        private int _serverTick;
        private int _sendRate = 4;

        public Map Map { get; }

        public Server()
        {
            Map = new Map();
            Self = new Player(Utils.RandomColor());
            Map.Players.Add(Self);

            _gameWorldSyncSourceRoot = new SourceSynchronizerRoot(Map);
            _tcpListener = new TcpListener(IPAddress.Loopback, 1234);
            _tcpListener.Start();
        }

        public Player Self { get; set; }

        public async void StartListening()
        {
            while (true)
            {
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();

                var player = new Player(Utils.RandomColor());
                Map.Players.Add(player);
                var client = new ServerSideClient(tcpClient, player);
                client.BeginReceiving();
                client.Disconnected += ClientDisconnected;
                _newClients.Add(client);
            }
        }

        public void HandleClick(Vector2 position)
        {
            Self.TargetPosition = position;
        }

        private void ClientDisconnected(object sender, EventArgs e)
        {
            if (sender is ServerSideClient serverSideClient)
            {
                Map.Players.Remove(serverSideClient.Player);
                _clients.Remove(serverSideClient);
            }
        }

        public override void Update(GameTime gameTime)
        {
            IncrementClientTicks();

            if (_serverTick++ % (60 / SendRate) == 0)
            {
                BroadcastWorld();
            }
        }

        public int SendRate
        {
            get => _sendRate;
            set
            {
                value = Math.Clamp(value, 1, 60);
                _sendRate = value;
            }
        }

        private void IncrementClientTicks()
        {
            foreach (ServerSideClient serverSideClient in _clients)
            {
                serverSideClient.Tick++;
            }
        }

        private void BroadcastWorld()
        {
            using WriteSession writeSession = _gameWorldSyncSourceRoot.BeginWrite();

            SynchronizationPacket synchronizationPacket = writeSession.WriteChanges();

            if (_clients.Count != 0)
            {
                foreach (ServerSideClient client in _clients)
                {
                    byte[] data = synchronizationPacket.SetTick(client.Tick);
                    client.SendWorld(data);
                }
            }

            if (_newClients.Count != 0)
            {
                byte[] fullSerialize = writeSession.WriteFull();
                foreach (ServerSideClient newClient in _newClients)
                {
                    _clients.Add(newClient);
                    newClient.SendWorld(fullSerialize);
                }
                _newClients.Clear();
            }
        }

        
    }
}