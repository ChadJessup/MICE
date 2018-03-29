using System;

namespace MICE.Nintendo
{
    public class FrameCompleteArgs : EventArgs
    {
        public byte[] FrameData { get; set; }
        public long FrameNumber { get; set; }
        public TimeSpan FrameDuration { get; set; }
    }
}