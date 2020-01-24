using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoSync.SyncSource;

namespace MonoSync.Sample.Tweening
{
    public class Server : SimpleGameComponent
    {
        private readonly Dictionary<int, ServerSideClient> _clients = new Dictionary<int, ServerSideClient>();
        private readonly SyncSourceRoot _gameWorldSyncSourceRoot;
        private readonly List<ServerSideClient> _newClients = new List<ServerSideClient>();
        private readonly TcpListener _tcpListener;
        private int _playerCounter;
        private int _serverTick;

        public Server()
        {
            Map = new Map();
            Map.Players.Add(0, new Player(Utils.RandomColor()));

            var settings = SyncSourceSettings.Default;
            settings.TypeEncoder = new TweenGameTypeEncoder();
            settings.FieldDeserializerResolverFactory = new TweenGameFieldSerializerFactory();
            _gameWorldSyncSourceRoot = new SyncSourceRoot(Map, settings);
            _tcpListener = new TcpListener(IPAddress.Loopback, 1234);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(ConnectionCallback, null);
        }

        public Map Map { get; }

        private void ConnectionCallback(IAsyncResult ar)
        {
            TcpClient tcpClient = _tcpListener.EndAcceptTcpClient(ar);
            int playerId = ++_playerCounter;
            Map.Players.Add(playerId, new Player(Utils.RandomColor()));
            var client = new ServerSideClient(tcpClient, playerId, pos => Map.Players[playerId].TargetPosition = pos);
            client.Disconnected += ServerSideClientOnDisconnect;
            _newClients.Add(client);
            _tcpListener.BeginAcceptTcpClient(ConnectionCallback, null);
        }

        public void HandleClick(Vector2 position)
        {
            Map.Players[0].TargetPosition = position;
        }

        private void ServerSideClientOnDisconnect(object sender, EventArgs e)
        {
            if (sender is ServerSideClient serverSideClient)
            {
                Map.Players.Remove(serverSideClient.PlayerId);
                _clients.Remove(serverSideClient.PlayerId);
            }
        }

        public override void Update(GameTime gameTime)
        {
            IncrementClientTicks();

            if (_serverTick++ % 15 == 0)
            {
                BroadcastWorld();
            }
        }

        private void IncrementClientTicks()
        {
            foreach (ServerSideClient serverSideClient in _clients.Values)
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
                foreach (ServerSideClient client in _clients.Values)
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
                    _clients.Add(newClient.PlayerId, newClient);
                    newClient.SendWorld(fullSerialize);
                }

                _newClients.Clear();
            }
        }
    }
}