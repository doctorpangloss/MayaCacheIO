using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MayaCacheIO
{
    public class nCacheFile
    {
        PositionChannel _position;

        public PositionChannel Position
        {
            get { return _position; }
            set { _position = value; }
        }
        RGBChannel _rgb;

        public RGBChannel Rgb
        {
            get { return _rgb; }
            set { _rgb = value; }
        }
        int _frameNumber;

        public int FrameNumber
        {
            get { return _frameNumber; }
            set { _frameNumber = value; }
        }

        public CacheChannel[] Channels
        {
            get
            {
                CacheChannel[] v = { this._position, this._rgb };
                return v;
            }
        }

        public nCacheFile()
        {
            Position = PositionChannel.MemoryBackedPosition();
            Rgb = RGBChannel.MemoryBackedRGB();
        }
    }
}
