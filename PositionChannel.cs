using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Media;

namespace MayaCacheIO
{
    public class PositionChannel : OneFilePerFrameCacheChannel
    {
        public PositionChannel(Stream stream) : base(stream)
        {
            ChannelInterpretation = Position;
            ChannelType = FloatVectorArray;
        }

        public static PositionChannel MemoryBackedPosition()
        {
            return new PositionChannel(new MemoryStream(24 * 640 * 480));
        }
    }
}
