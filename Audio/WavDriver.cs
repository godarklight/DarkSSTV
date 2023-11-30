using System;
using System.IO;
using System.Text;

namespace DarkSSTV;

class WavDriver : IAudio
{
    MemoryStream sinkData = new MemoryStream();
    Action<double[]> sink;
    Action<double[]> source;
    FileStream sourceData;
    int sourceLeft = 0;

    public void SetSink(Action<double[]> sink)
    {
        this.sink = sink;
    }

    public void SetSource(Action<double[]> source)
    {
        this.source = source;
    }

    public void Run()
    {
        double[] sinkData = new double[32];
        while (sink != null)
        {
            sink(sinkData);
            for (int i = 0; i < sinkData.Length; i++)
            {
                short s16 = (short)(sinkData[i] * short.MaxValue);
                //LSB
                this.sinkData.WriteByte((byte)(s16 & 0xFF));
                this.sinkData.WriteByte((byte)((s16 >> 8) & 0xFF));
            }
        }
        if (File.Exists("input.wav"))
        {
            sourceData = new FileStream("input.wav", FileMode.Open);
            byte[] b4 = new byte[4];
            sourceData.Seek(8, SeekOrigin.Begin);
            sourceData.Read(b4, 0, 4);
            string wavHeader = Encoding.ASCII.GetString(b4, 0, 4);
            if (wavHeader != "WAVE")
            {
                throw new IOException($"Unsupported wav file {wavHeader}");
            }
            bool dataFound = false;

            while (!dataFound)
            {
                sourceData.Read(b4, 0, 4);
                string chunkHeader = Encoding.ASCII.GetString(b4, 0, 4);
                sourceData.Read(b4, 0, 4);
                int chunkSize = BitConverter.ToInt32(b4);
                if (chunkHeader == "data")
                {
                    sourceLeft = chunkSize;
                    dataFound = true;
                }
                else
                {
                    sourceData.Seek(chunkSize, SeekOrigin.Current);
                }
            }
        }

        double[] data = new double[32];
        while (source != null)
        {
            long sourceLeft = sourceData.Length - sourceData.Position;
            if (sourceLeft < 64)
            {
                break;
            }
            for (int i = 0; i < 32; i++)
            {
                short s = (short)sourceData.ReadByte();
                s |= (short)(sourceData.ReadByte() << 8);
                data[i] = s / (double)short.MaxValue;
            }
            source(data);
        }
    }

    public void Stop()
    {
        if (sinkData.Position > 0)
        {
            string filename = "output.wav";
            byte[] b4 = new byte[4];
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                //RIFF header
                Encoding.ASCII.GetBytes("RIFF", 0, 4, b4, 0);
                fs.Write(b4, 0, 4);
                int dataSize = (int)sinkData.Length;
                int fileSize = dataSize + 36;
                b4 = BitConverter.GetBytes(fileSize);
                fs.Write(b4, 0, 4);
                //WAVE header
                Encoding.ASCII.GetBytes("WAVE", 0, 4, b4, 0);
                fs.Write(b4, 0, 4);
                //fmt chunk
                Encoding.ASCII.GetBytes("fmt ", 0, 4, b4, 0);
                fs.Write(b4, 0, 4);
                b4 = BitConverter.GetBytes(16);
                fs.Write(b4, 0, 4);
                //Type 1 PCM
                fs.WriteByte(1);
                fs.WriteByte(0);
                //Number of channels: Mono
                fs.WriteByte(1);
                fs.WriteByte(0);
                //Sample Rate
                b4 = BitConverter.GetBytes(48000);
                fs.Write(b4, 0, 4);
                //Sample rate * Bytes per sample * Channels
                b4 = BitConverter.GetBytes(48000 * 2 * 1);
                fs.Write(b4, 0, 4);
                //Bytes per sample * Channels
                fs.WriteByte(2);
                fs.WriteByte(0);
                //Bits per sample
                fs.WriteByte(16);
                fs.WriteByte(0);
                //data chunk
                Encoding.ASCII.GetBytes("data", 0, 4, b4, 0);
                fs.Write(b4, 0, 4);
                b4 = BitConverter.GetBytes(dataSize);
                fs.Write(b4, 0, 4);
                fs.Flush();
                sinkData.Seek(0, SeekOrigin.Begin);
                sinkData.CopyTo(fs);
                sinkData = new MemoryStream();
            }
        }
    }
}