using System;

namespace MonoSync.Sample.Tweening
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Settings.Default = () =>
            {
                var settings = new Settings();
                settings.Serializers.AddSerializer(new ColorSerializer());
                settings.Serializers.AddSerializer(new Vector2Serializer());
                return settings;
            };

            using var game = new MainGame();
            game.Run();
        }
    }
}