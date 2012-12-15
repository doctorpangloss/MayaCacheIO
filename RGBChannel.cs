using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Media;

namespace MayaCacheIO
{
    public class RGBChannel : OneFilePerFrameCacheChannel
    {
        public RGBChannel(Stream stream) : base(stream)
        {
            ChannelInterpretation = RGBPP;
            ChannelType = FloatVectorArray;
        }

        public void Write(byte R, byte G, byte B)
        {
            Write((float)R * 0.004f, (float)G * 0.004f, (float)B * 0.004f);
        }

        public static RGBChannel MemoryBackedRGB()
        {
            return new RGBChannel(new MemoryStream(24 * 640 * 480));
        }
    }
}
