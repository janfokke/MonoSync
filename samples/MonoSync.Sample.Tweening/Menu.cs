using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using Myra.Graphics2D.UI;

namespace MonoSync.Sample.Tweening
{
    public class Menu : GameScreen
    {
        public Menu(MainGame game) : base(game)
        {
            var verticalStackPanel = new VerticalStackPanel
                {HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center};
            var hostButton = new TextButton {Text = "Host", Width = 100, Height = 50};
            hostButton.Click += (o, e) => game.HostGame();
            verticalStackPanel.Widgets.Add(hostButton);

            var joinButton = new TextButton {Text = "Join", Width = 100, Height = 50};
            joinButton.Click += (o, e) => game.JoinGame();
            verticalStackPanel.Widgets.Add(joinButton);
            Desktop.Widgets.Add(verticalStackPanel);
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            Desktop.Render();
        }
    }
}