using System.Runtime.InteropServices;
using System.Text;
using Gdk;
using System.Numerics;
using System;
namespace DarkSSTV;

class Encoder
{
    AudioDriver ad = new AudioDriver();
    //Cyclic prefix, play last 1/8th
    int pos = 448;    
    bool cyclic = true;
    Complex[] audioData;
    Morse m = new Morse("VK4GDL TESTING");
    int morseKeyLeft = 0;
    bool morseState = false;
    Constellation c = new Constellation();
    Random random = new Random();



    public void Run()
    {
        ad.sink = WriteAudio;
    }

    public void Stop()
    {
        ad.Stop();
    }

    private void WriteAudio(double[] data)
    {
        if (audioData == null)
        {
            GenerateChunk();
        }
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = audioData[pos].Real;
            pos++;
            if (pos == audioData.Length)
            {
                if (!cyclic)
                {
                    GenerateChunk();
                    pos = 448;
                    cyclic = true;
                }
                else
                {
                    pos = 0;
                    cyclic = false;
                }
            }
        }
    }

    private void GenerateChunk()
    {
        byte[] data = new byte[7];
        random.NextBytes(data);
        c.data = data;
        if (morseKeyLeft == 0)
        {
            morseKeyLeft = 5;
            morseState = m.GetKey();
        }
        c.morse = morseState;
        morseKeyLeft--;
        Complex[] fftInput = c.GetData();
        audioData = FFT.CalcIFFT(fftInput);
    }


}