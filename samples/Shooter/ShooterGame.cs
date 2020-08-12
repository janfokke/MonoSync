using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tweening;

namespace MonoSync.Sample.Tweening
{
    public class ShooterGame : GameScreen
    {
        private readonly Map _map;
        private SpriteBatch _spriteBatch;

        public ShooterGame(Game game, Map map) : base(game)
        {
            _map = map;
        }

        public event EventHandler<Vector2> Click;

        public override void Update(GameTime gameTime)
        {
            MouseStateExtended mouseState = MouseExtended.GetState();
            float elapsedSeconds = gameTime.GetElapsedSeconds();

            foreach (Player player in _map.Players)
            {
                player.Position = Vector2.Lerp(player.Position, player.TargetPosition, 0.05f);
            }

            if (mouseState.WasButtonJustDown(MouseButton.Left))
            {
                Click?.Invoke(this, mouseState.Position.ToVector2());
            }
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            _spriteBatch.Dispose();
            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            foreach (Player player in _map.Players)
            {
                _spriteBatch.FillRectangle(player.Position.X, player.Position.Y, 50, 50, player.Color);
            }

            _spriteBatch.End();
        }
    }
}