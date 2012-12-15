using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MayaCacheIO
{
    public class OneFilePerFrameCacheChannel : CacheChannel
    {
        int _frameNumber;

        public int FrameNumber
        {
            get { return _frameNumber; }
            set { _frameNumber = value; }
        }

        public override double StartTime
        {
            get
            {
                return FrameNumber * SampleRate;
            }
            set
            {
                throw new NotSupportedException();
                // FrameNumber = (int)(value / SampleRate);
            }
        }

        public override double EndTime
        {
            get
            {
                return StartTime + SampleRate;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public OneFilePerFrameCacheChannel(Stream stream)
            : base(stream)
        { }
    }
}
