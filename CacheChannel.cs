using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace MayaCacheIO
{
    [Serializable()]
    public class CacheChannel : MiscUtil.IO.EndianBinaryWriter
    {
        string _channelType, _channelInterpretation, _sampleType;

        public virtual string SampleType
        {
            get { return _sampleType; }
            set { _sampleType = value; }
        }

        public virtual string ChannelInterpretation
        {
            get { return _channelInterpretation; }
            set { _channelInterpretation = value; }
        }

        public virtual string ChannelType
        {
            get { return _channelType; }
            set { _channelType = value; }
        }

        public virtual string ChannelName
        {
            get { return _channelInterpretation; }
        }
        
        double _sampleRate, _startTime, _endTime;

        public virtual double EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        public virtual double StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public virtual double SampleRate
        {
            get { return _sampleRate; }
            set { _sampleRate = value; }
        }

        public int ArrayLength
        {
            get;
            set;
        }

        public virtual Stream Data
        {
            get { return BaseStream; }
        }

        public CacheChannel(Stream baseStream) : base(EndianBitConverter.Big,baseStream,Encoding.ASCII)
        {
            _sampleType = RegularSamplingType;
            _channelType = DoubleVectorArray;
            _channelInterpretation = Position;
            _startTime = 200;
            _endTime = 400;
            _sampleRate = 200;
        }

        public void Write(double x, double y, double z)
        {
            base.Write(x);
            base.Write(y);
            base.Write(z);
            ArrayLength += 1;
        }

        public void Write(float x, float y, float z)
        {
            base.Write(x);
            base.Write(y);
            base.Write(z);
            ArrayLength += 1;
        }

        public Stream ToDoubleArray()
        {
            MemoryStream s = new MemoryStream((int)BaseStream.Length/2);
            EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, BaseStream);
            BaseStream.Seek(0, SeekOrigin.Begin);
            EndianBinaryWriter writer = new EndianBinaryWriter(EndianBitConverter.Big, s);

            for (int i = 0; i < ArrayLength*3; i++)
            {
                writer.Write((double)reader.ReadSingle());
            }

            return s;
        }

        public const string ID = "id", Count = "count", Position = "position", Velocity = "velocity", Acceleration = "Acceleration", WorldPosition = "worldPosition",
            WorldVelocity = "worldVelocity", Mass = "mass", BirthTime = "birthTime", Age = "age", FinalLifespanPP = "finalLifespanPP", LifespanPP = "lifespanPP", RGBPP = "rgbPP",
            DoubleArray = "DBLA", FloatVectorArray = "FVCA", DoubleVectorArray = "DVCA", RegularSamplingType="Regular", IrregularSamplingType="Irregular";

    }
}
