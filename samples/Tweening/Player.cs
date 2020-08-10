﻿using Microsoft.Xna.Framework;
using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Sample.Tweening
{
    [AddINotifyPropertyChangedInterface]
    public class Player
    {
        [SyncConstructor]
        public Player()
        {
        }

        public Player(Color color)
        {
            Color = color;
        }

        [Synchronize (SynchronizationBehaviour.Interpolated)]
        public Vector2 Position { get; set; }

        [Synchronize] 
        public Vector2 TargetPosition { get; set; }

        [Synchronize] 
        public Color Color { get; set; }
    }
}