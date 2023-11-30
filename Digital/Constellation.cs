//At 48ksps, 512samples, 0.3-2.7khz, 93.75hz spacing, 7th carrier first, 57th carrier last.
//3th carrier, 281.25hz is pilot tone at 0 degree phase shift.
//656.25hz is morse carrier, 7th carrier
using System.Numerics;
using Gtk;

namespace DarkSSTV;

class Constellation
{
    public byte[] data;
    public bool morse;

    public Complex[] GetData()
    {
        Complex[] retVal = new Complex[512];
        retVal[3] = 2.0 * Complex.One;
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

    private Complex Qam4Map(bool bit1, bool bit2)
    {
        //00 = 1,1
        //01 = -1,1
        //10 = -1,-1
        //11 = 1,-1
        if (!bit2 && !bit1)
        {
            return new Complex(1, 1);
        }        
        if (!bit2 && bit1)
        {
            return new Complex(-1, 1);
        }
        if (bit2 && !bit1)
        {
            return new Complex(-1, -1);
        }
        if (bit2 && bit1)
        {
            return new Complex(1, -1);
        }
        return Complex.Zero;
    }

}