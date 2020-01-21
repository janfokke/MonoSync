using System;

namespace MonoSync
{
    public class Clock
    {
        public int OwnTick { get; set; }
        public int OtherTick { get; set; }

        public int Difference => Math.Max(0, OwnTick - OtherTick);

        public void Update()
        {
            OwnTick++;
            OtherTick++;
        }

        public override string ToString()
        {
            return $"{nameof(OwnTick)}:{OwnTick} {nameof(OtherTick)}:{OtherTick} {nameof(Difference)}:{Difference}";
        }
    }
}