using System;

namespace MonoSync
{
    public class Clock
    {
        private DateTime _ownTickSetDateTime = DateTime.Now;
        private TimeSpan _ownTickOffset;
        private TimeSpan _ownTick;
        public TimeSpan OwnTick
        {
            get => _ownTick;
            set
            {
                _ownTickSetDateTime = DateTime.Now;
                _ownTick = _ownTickOffset = value;
            }
        }

        private DateTime _otherTickSetDateTime = DateTime.Now;
        private TimeSpan _otherTickOffset;
        private TimeSpan _otherTick;
        public TimeSpan OtherTick
        {
            get => _otherTick;
            set
            {
                _otherTickSetDateTime = DateTime.Now;
                _otherTick = _otherTickOffset = value;
            }
        }

        public void Update()
        {
            _ownTick = DateTime.Now - _ownTickSetDateTime + _ownTickOffset;
            _otherTick = DateTime.Now - _otherTickSetDateTime + _otherTickOffset;
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