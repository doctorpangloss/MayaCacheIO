using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using MiscUtil.IO;

namespace MayaCacheIO
{
    [Serializable()]
    public class Cache
    {
        public const string OneFilePerFrameCacheType = "OneFilePerFrame", OneFileCacheType = "OneFile";
        private byte[] CacheVersion = { 0, 0, 0, 0x4, 0x30, 0x2e, 0x31, 0 };

        private string _baseFileName, _directory, _cacheType;
        private int _cacheStartTime, _cacheEndTime, _timePerFrame;

        public string CacheType
        {
            get { return _cacheType; }
            set { _cacheType = value; }
        }

        public string Directory
        {
            get { return _directory; }
            set { _directory = value; }
        }

        public string BaseFileName
        {
            get { return _baseFileName; }
            set { _baseFileName = value; }
        }

        public int TimePerFrame
        {
            get { return _timePerFrame; }
            set { _timePerFrame = value; }
        }

        public int CacheEndTime
        {
            get { return _cacheEndTime; }
            set { _cacheEndTime = value; }
        } 

        public int CacheStartTime
        {
            get { return _cacheStartTime; }
            set { _cacheStartTime = value; }
        }

        public bool Legacy
        { get; set; }

        private char[] Padded(string s)
        {
            return ((string)(_baseFileName + "_" + s)).PadRight((UnpaddedLength(s)+3) & (~3), (char)0).ToCharArray();
        }

        private int UnpaddedLength(string s)
        {
            return _baseFileName.Length + 1 + s.Length;
        }

        public void WriteCacheFile(CacheChannel[] channels, int fileNumber, bool legacy)
        {
            if (legacy)
            {

                int startTime = fileNumber * TimePerFrame;
                int endTime = startTime + TimePerFrame;

                FileStream cacheFile = new FileStream(Path.Combine(Directory, BaseFileName.ToString() + string.Format(".{0}.pdc", startTime)), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                EndianBinaryWriter io = new EndianBinaryWriter(MiscUtil.Conversion.EndianBitConverter.Big, cacheFile, Encoding.ASCII);


                // write header
                io.Write("PDC ".ToCharArray());
                io.Write((int)1);
                io.Write((int)1);
                io.Write((int)0);
                io.Write((int)0);

                // write num particles
                io.Write(channels[0].ArrayLength);
                // write number of channels
                io.Write(channels.Length);

                foreach (CacheChannel channel in channels)
                {
                    // write length of channel name
                    io.Write(channel.ChannelInterpretation.Length);
                    // write channel name
                    io.Write(channel.ChannelInterpretation.ToCharArray());
                    // write type of array
                    Stream s = channel.Data;
                    int type = 0;
                    switch (channel.ChannelType)
                    {
                        case CacheChannel.DoubleArray:
                            type = 3;
                            break;
                        case CacheChannel.DoubleVectorArray:
                        case CacheChannel.FloatVectorArray:
                            type = 5;
                            break;

                        //// requires conversion
                        //type = 5;
                        ////s = channel.ToDoubleArray();
                        //break;
                        default:
                            type = 2;
                            throw new ArgumentException("Unsupported cache channel type. Must be a double array, a double vector array, or a float vector array.");
                    }
                    // write type
                    io.Write(type);
                    // write data
                    s.Seek(0, SeekOrigin.Begin);
                    s.CopyTo(cacheFile);
                    channel.Close();

                    if (s.CanRead)
                    {
                        s.Close();
                    }
                }

                io.Close();
            }
            else
            {
                WriteCacheFile(channels, fileNumber);
            }
        }

        public void WriteCacheFile(CacheChannel[] channels, int fileNumber)
        {
            // Start with a large megabyte buffer
            //MemoryStream cacheMemoryStream = new MemoryStream();
            FileStream cacheFile = new FileStream(Path.Combine(Directory, BaseFileName.ToString() + string.Format("Frame{0}.mc", fileNumber)), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            EndianBinaryWriter writer = new EndianBinaryWriter(MiscUtil.Conversion.EndianBitConverter.Big, cacheFile, Encoding.ASCII);
            int startTime = fileNumber * TimePerFrame;
            int endTime = startTime + TimePerFrame;

            // Write the header
            writer.Write("FOR4".ToCharArray());
            // Write the length of CACHVRSN, STIM and ETIM, plus two ints. Always 40.
            writer.Write((int)40);
            writer.Write("CACHVRSN".ToCharArray());
            writer.Write(CacheVersion);
            writer.Write("STIM".ToCharArray());
            writer.Write((int)4);
            writer.Write(startTime);
            writer.Write("ETIM".ToCharArray());
            writer.Write((int)4);
            writer.Write(endTime);
            writer.Write("FOR4".ToCharArray());

            int blockSize = 0;
            long blockSizeLocation = writer.BaseStream.Position;

            // Revisit block size later
            writer.Write(blockSize);
            // Write channel header
            writer.Write("MYCH".ToCharArray());
            
            foreach (CacheChannel channel in channels)
            {
                writer.Write("CHNM".ToCharArray());

                char[] channelNamePadded = Padded(channel.ChannelName);
                int channelNamePaddedLength = channelNamePadded.Length;

                // Write channel name length and padded string
                writer.Write(channelNamePaddedLength);
                writer.Write(channelNamePadded);
                writer.Write("SIZE".ToCharArray());
                // Write the size of array field (always 4)
                writer.Write((int)4);
                // Write array length (count of points)
                writer.Write(channel.ArrayLength);
                // Write data type tag
                writer.Write(channel.ChannelType.ToCharArray());
                // Write number of bytes in data
                writer.Write((int)channel.Data.Length);
                writer.Flush();
                // Write the data
                channel.Data.Seek(0, SeekOrigin.Begin);
                channel.Data.CopyTo(cacheFile);

                channel.Close();
            }

            // Compute the block size and write it
            blockSize = (int)writer.BaseStream.Position - (int)blockSizeLocation - 4;
            writer.BaseStream.Seek(blockSizeLocation, SeekOrigin.Begin);
            writer.Write(blockSize);

            // Write everything to the file
            writer.Flush();
            writer.Close();
        }

        public void WriteXML()
        {
            throw new NotImplementedException();

            FileStream xmlFile = new FileStream(Path.Combine(Directory, BaseFileName.ToString() + ".xml"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter writer = new StreamWriter(xmlFile, Encoding.ASCII);
            writer.NewLine = "\n";

            writer.WriteLine("<?xml version=\"1.0\"?>\n<Autodesk_Cache_File>");
            writer.WriteLine("<cacheType Type=\"{0}\" Format=\"mcc\"/>", CacheType);
            writer.WriteLine("<time Range=\"{0}-{1}\"/>", CacheStartTime, CacheEndTime);
            writer.WriteLine("<cacheTimePerFrame TimePerFrame=\"{0}\"/>", TimePerFrame);
            writer.WriteLine("<cacheVersion Version=\"2.0\"/>");
        
        }
    }


}
