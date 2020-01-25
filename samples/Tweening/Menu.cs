using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using Myra.Graphics2D.UI;

namespace MonoSync.Sample.Tweening
{
    public class Menu : GameScreen
    {
        private readonly VerticalStackPanel _verticalStackPanel;

        public Menu(MainGame game) : base(game)
        {
            _verticalStackPanel = new VerticalStackPanel
                {HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center};
            var hostButton = new TextButton {Text = "Host", Width = 100, Height = 50};
            hostButton.Click += (o, e) => game.HostGame();
            _verticalStackPanel.Widgets.Add(hostButton);

            var joinButton = new TextButton {Text = "Join", Width = 100, Height = 50};
            joinButton.Click += async (o, e) =>
            {
                hostButton.Enabled = false;
                joinButton.Enabled = false;
                try
                {
                    await game.JoinGame();
                }
                catch
                {
                    hostButton.Enabled = true;
                    joinButton.Enabled = true;
                }

            };
            _verticalStackPanel.Widgets.Add(joinButton);
        }

        public override void Dispose()
        {
            Desktop.Widgets.Remove(_verticalStackPanel);
            base.Dispose();
        }

        public override void Initialize()
        {
            Desktop.Widgets.Add(_verticalStackPanel);
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            
        }
    }
}