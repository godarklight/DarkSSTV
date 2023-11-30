using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Gtk;

namespace DarkSSTV;
class FrameDecoder
{
    byte[] fileData;
    byte[] b500 = new byte[500];
    int writePos = 0;
    bool receiving = false;
    string callsign = "";
    string filename = "";
    string comment = "";

    public void FrameEvent(Complex[] samples)
    {
        Complex[] fft = FFT.CalcFFT(samples);
        Constellation.Decode(fft, out bool morse, out bool startFrame, out byte[] data);
        if (startFrame)
        {
            writePos = 0;
            receiving = true;
        }
        if (!receiving)
        {
            return;
        }
        Array.Copy(data, 0, b500, writePos, 5);
        writePos += 5;
        if (writePos == 500)
        {
            byte[] b250 = Viterbi.Decode(b500);
            int frameID = b250[0] << 8 | b250[1];
            int frameTotal = b250[2] << 8 | b250[3];
            if (frameID == 0)
            {
                int length = b250[5] << 24 | b250[6] << 16 | b250[7] << 8 | b250[8];
                if (fileData == null || fileData.Length != length)
                {
                    Console.WriteLine($"File transfer of {length} bytes");
                    fileData = new byte[length];
                }
                int offset = 9;
                int callsignLength = b250[offset++];
                if (callsignLength > 0)
                {
                    callsign = Encoding.UTF8.GetString(b250, offset, callsignLength);
                    offset += callsignLength;
                    Console.WriteLine($"callsign: {callsign}");
                }
                int fileNameLength = b250[offset++];
                if (fileNameLength > 0)
                {
                    filename = Encoding.UTF8.GetString(b250, offset, fileNameLength);
                    offset += fileNameLength;
                }
                int commentLength = b250[offset++];
                if (commentLength > 0)
                {
                    comment = Encoding.UTF8.GetString(b250, offset, commentLength);
                    offset += commentLength;
                }
            }
            else
            {
                if (fileData != null)
                {
                    int writePos = 246 * (frameID - 1);
                    if (frameID < frameTotal)
                    {
                        Array.Copy(b250, 4, fileData, writePos, 246);
                    }
                    else
                    {
                        Array.Copy(b250, 4, fileData, writePos, fileData.Length % 246);
                        File.WriteAllBytes(filename, fileData);
                    }
                }
            }
            Console.WriteLine($"RX frame {frameID}");
            receiving = false;
        }
    }
}