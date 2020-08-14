using System;

namespace MonoSync
{
    public class Clock
    {
        private DateTime _ownTickSetDateTime = DateTime.Now;
        private TimeSpan _ownTick;
        public TimeSpan OwnTick
        {
            get => _ownTick;
            set
            {
                _ownTickSetDateTime = DateTime.Now;
                _ownTick = value;
            }
        }

        private DateTime _otherTickSetDateTime = DateTime.Now;
        private TimeSpan _otherTick;
        public TimeSpan OtherTick
        {
            get => _otherTick;
            set
            {
                _otherTickSetDateTime = DateTime.Now;
                _otherTick = value;
            }
        }

        public void Update()
        {
            _ownTick = DateTime.Now - _ownTickSetDateTime + _ownTick;
            _otherTick = DateTime.Now - _otherTickSetDateTime + _otherTick;
        }

        public TimeSpan Difference
        {
            get
            {
                if(OtherTick > OwnTick)
                    return TimeSpan.Zero;
                return OwnTick - OtherTick;
            }
        }

        public override string ToString()
        {
            return $"{nameof(OwnTick)}:{OwnTick} {nameof(OtherTick)}:{OtherTick} {nameof(Difference)}:{Difference}";
        }
    }
}