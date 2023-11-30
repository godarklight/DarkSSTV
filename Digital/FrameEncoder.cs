using System;
using System.Collections.Generic;
using System.Text;
using GLib;

namespace DarkSSTV;
class FrameEncoder
{
    private List<byte[]> frames = new List<byte[]>();

    public int Length
    {
        get
        {
            return frames.Count;
        }
    }

    public int Encode(byte[] data, string callsign, string filename, string comment)
    {
        int dataLeft = data.Length;
        int totalFrames = (int)Math.Ceiling(data.Length / 496.0);
        byte[] callsignbytes = Encoding.UTF8.GetBytes(callsign);
        byte[] filenamebytes = Encoding.UTF8.GetBytes(filename);
        byte[] commentbytes = Encoding.UTF8.GetBytes(comment);
        byte[] header = new byte[500];
        header[2] = (byte)((totalFrames >> 8) & 0xFF);
        header[3] = (byte)(totalFrames & 0xFF);
        header[4] = 1; //1 file 2 sstv
        header[5] = (byte)((data.Length >> 24) & 0xFF);
        header[6] = (byte)((data.Length >> 16) & 0xFF);
        header[7] = (byte)((data.Length >> 8) & 0xFF);
        header[8] = (byte)((data.Length) & 0xFF);
        header[9] = (byte)callsignbytes.Length;
        Array.Copy(callsignbytes, 0, header, 10, callsignbytes.Length);
        int offset = (byte)callsignbytes.Length;
        header[10 + offset] = (byte)filenamebytes.Length;
        Array.Copy(filenamebytes, 0, header, 11 + offset, filenamebytes.Length);
        offset += filenamebytes.Length;
        header[11 + offset] = (byte)commentbytes.Length;
        Array.Copy(commentbytes, 0, header, 12 + offset, commentbytes.Length);
        frames.Add(header);


        int frameID = 1;
        while (dataLeft > 0)
        {
            byte[] b248 = new byte[248];
            if (dataLeft >= 248)
            {
                Array.Copy(data, data.Length - dataLeft, b248, 0, 248);
            }
            else
            {
                Array.Copy(data, data.Length - dataLeft, b248, 0, dataLeft);
            }

            byte[] b500 = new byte[500];
            b500[0] = (byte)((frameID >> 8) & 0xFF);
            b500[1] = (byte)(frameID & 0xFF);
            b500[2] = (byte)((totalFrames >> 8) & 0xFF);
            b500[3] = (byte)(totalFrames & 0xFF);
            byte[] viterbiData = Viterbi.Encode(b248);
            Array.Copy(viterbiData, 0, b500, 0, 496);
            frames.Add(b500);
            dataLeft -= 248;
            frameID++;
        }

        return totalFrames;
    }

    public byte[] GetFrame(int id)
    {
        return frames[id];
    }
}