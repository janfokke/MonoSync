using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using MonoSync.Attributes;
using Myra;

namespace MonoSync.Sample.Tweening
{
    public class MainGame : Game
    {
        private readonly ScreenManager _screenManager = new ScreenManager();
        private Client _client;
        private Server _server;

        public MainGame()
        {
            _ = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1f / 60f);
        }

        public Vector2 Linear { get; set; } = new Vector2(200, 50);

        public void LoadMenu()
        {
            _screenManager.LoadScreen(new Menu(this));
        }

        public void HostGame()
        {
            _server = new Server();
            var tweenGame = new TweenGame(this, _server.Map);
            tweenGame.Click += (o, position) => _server.HandleClick(position);
            _screenManager.LoadScreen(tweenGame);
        }

        public void JoinGame()
        {
            _client = new Client();
            _client.Connect(map =>
            {
                Player self = map.Players.Last().Value;

                // Configure Position property synchronization behaviour to highestTick to avoid player snapping back, because server version is older
                self.GetSyncTargetProperty(x => x.Position).SynchronizationBehaviour =
                    SynchronizationBehaviour.HighestTick;

                Components.Add(_client);
                var tweenGame = new TweenGame(this, map);
                tweenGame.Click += (o, e) =>
                {
                    self.TargetPosition = e;
                    _client.SendMouseClick(new Vector2(e.X, e.Y));
                };
                _screenManager.LoadScreen(tweenGame);
            });
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            MyraEnvironment.Game = this;
        }

        protected override void Initialize()
        {
            base.Initialize();
            LoadMenu();
        }

        protected override void Update(GameTime gameTime)
        {
            _server?.Update(gameTime);
            _client?.Update(gameTime);
            _screenManager.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _screenManager.Draw(gameTime);
        }
    }
}