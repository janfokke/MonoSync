using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using MonoSync.Attributes;
using Myra;
using Myra.Graphics2D.UI;

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

        public void LoadMenu()
        {
            _screenManager.LoadScreen(new Menu(this));
        }

        public void HostGame()
        {
            _server = new Server();
            _server.StartListening();
            var tweenGame = new ShooterGame(this, _server.Map);
            tweenGame.Click += (o, position) => _server.HandleClick(position);
            _screenManager.LoadScreen(tweenGame);
            InitializeServerSettingsView();
        }

        private void InitializeServerSettingsView()
        {
            Grid grid = new Grid();
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.HorizontalAlignment = HorizontalAlignment.Right;
            grid.VerticalAlignment = VerticalAlignment.Stretch;

            var label = new Label {Text = $"Send rate per sec [{_server.SendRate}]"};
            var horizontalSlider = new HorizontalSlider {GridRow = 0,Width = 100, GridColumn = 1, Minimum = 1, Maximum = 60};
            horizontalSlider.ValueChanged += (o, e) =>
            {
                int newSendRate = (int) e.NewValue;
                label.Text = $"Send rate per sec [{newSendRate}]";
                _server.SendRate = newSendRate;
            };

            grid.Widgets.Add(label);
            grid.Widgets.Add(horizontalSlider);
            Desktop.Widgets.Add(grid);
        }

        public async Task JoinGame()
        {
            _client = new Client();

            Map map = await _client.Join();
          
            Player self = map.Players.Last();

            // Configure Position property synchronization behaviour to highestTick to avoid player snapping back, because server version is older
            self.GetSyncTargetProperty(x => x.Position).SynchronizationBehaviour =
                SynchronizationBehaviour.HighestTick;

            Components.Add(_client);
            var tweenGame = new ShooterGame(this, map);
            tweenGame.Click += (o, e) =>
            {
                self.TargetPosition = e;
                _client.SendMouseClick(new Vector2(e.X, e.Y));
            };
            _screenManager.LoadScreen(tweenGame);
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
            Desktop.Render();
        }
    }
}