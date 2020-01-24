using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using Myra;

namespace Tweening
{
    public class MainGame : Game
    {
        private readonly ScreenManager _screenManager = new ScreenManager();
        private Server _server;
        private Client _client;

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
                int playerId = map.Players.Last().Key;
                Components.Add(_client);
                var tweenGame = new TweenGame(this, map);
                tweenGame.Click += (o, e) =>
                {
                    map.Players[playerId].TargetPosition = e;
                    _client.SendMouseClick(new Vector2(e.X, e.Y));
                };
                _screenManager.LoadScreen(tweenGame);
            });
        }

        public MainGame()
        {
            _ = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1f / 60f);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            MyraEnvironment.Game = this;
        }

        public Vector2 Linear { get; set; } = new Vector2(200, 50);

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
