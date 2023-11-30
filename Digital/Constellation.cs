//At 48ksps, 512samples, 0.3-2.7khz, 93.75hz spacing, 7th carrier first, 57th carrier last.
//3th carrier, 281.25hz is pilot tone at 0 degree phase shift.
//656.25hz is morse carrier, 7th carrier
using System;
using System.Numerics;
using Gtk;

namespace DarkSSTV;

static class Constellation
{
    public static Complex[] Encode(bool morse, bool startFrame, byte[] data)
    {
        Complex[] retVal = new Complex[512];
        if (!startFrame)
        {
            retVal[3] = 2.0 * Complex.One;
        }
        else
        {
            retVal[3] = -2.0 * Complex.One;
        }
        if (morse)
        {
            //Make  morse carrier the strongest tone.
            retVal[7] = 4.0 * Complex.One;
        }

        //Write 5 bytes
        int bytePos = 0;
        int bitLeft = 0;
        int currentByte = 0;
        for (int i = 10; i < 30; i++)
        {
            if (bitLeft == 0)
            {
                bitLeft = 8;
                currentByte = data[bytePos++];
            }
            bool bit1 = (currentByte & 1) == 1;
            currentByte = currentByte >> 1;
            bool bit2 = (currentByte & 1) == 1;
            currentByte = currentByte >> 1;
            retVal[i] = Qam4Map(bit1, bit2);
            bitLeft -= 2;
        }

        return retVal;
    }

    const double angle14 = 1.0 / 4.0 * Math.Tau;
    const double angle34 = -1.0 / 4.0 * Math.Tau;
    public static void Decode(Complex[] fft, out bool morse, out bool startFrame, out byte[] data)
    {
        byte[] b5 = new byte[5];
        morse = false;
        startFrame = false;
        if (fft[3].Phase > angle14 || fft[3].Phase < angle34)
        {
            startFrame = true;
        }
        if (fft[7].Magnitude > 1)
        {
            morse = true;
        }
        for (int i = 0; i < 5; i++)
        {
            double error = 0;
            int currentByte = 0;
            for (int j = 0; j < 4; j++)
            {
                currentByte >>= 2;
                currentByte |= Qam4Map(fft[10 + (i * 4) + j], out double thisError) << 6;
                error += thisError;
            }
            b5[i] = (byte)currentByte;
        }
        data = b5;
    }

    private static Complex Qam4Map(bool bit1, bool bit2)
    {
        //00 = 1,1
        //01 = -1,1
        //10 = -1,-1
        //11 = 1,-1
        if (!bit2 && !bit1)
        {
            return new Complex(0.707, 0.707);
        }
        if (!bit2 && bit1)
        {
            return new Complex(-0.707, 0.707);
        }
        if (bit2 && !bit1)
        {
            return new Complex(-0.707, -0.707);
        }
        if (bit2 && bit1)
        {
            return new Complex(0.707, -0.707);
        }
        return Complex.Zero;
    }

    private static int Qam4Map(Complex num, out double error)
    {
        if (num.Real >= 0 && num.Imaginary >= 0)
        {
            error = (new Complex(0.707, 0.707) - num).Magnitude;
            return 0;
        }
        if (num.Real < 0 && num.Imaginary >= 0)
        {
            error = (new Complex(-0.707, 0.707) - num).Magnitude;
            return 1;
        }
        if (num.Real < 0 && num.Imaginary < 0)
        {
            error = (new Complex(-0.707, -0.707) - num).Magnitude;
            return 2;
        }
        if (num.Real >= 0 && num.Imaginary < 0)
        {
            error = (new Complex(0.707, -0.707) - num).Magnitude;
            return 3;
        }
        error = 0;
        return 0;
    }
}