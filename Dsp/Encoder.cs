using System.Runtime.InteropServices;
using System.Text;
using Gdk;
using System.Numerics;
using System;
using Gtk;
namespace DarkSSTV;

class Encoder
{
    IAudio audio;
    //Cyclic prefix, play last 1/8th
    int pos = 448;
    bool cyclic = true;
    Complex[] audioData;
    Morse morse;
    int morseKeyLeft = 0;
    bool morseState = false;
    FrameEncoder frame;
    byte[] frameData;
    int frameDataLeft = 0;
    int currentFrame = 0;
    int nextHeader = 0;
    bool running = true;

    public Encoder(Morse morse, FrameEncoder frame, IAudio audio)
    {
        this.morse = morse;
        this.frame = frame;
        this.audio = audio;
    }

    public void Run()
    {
        audio.SetSink(WriteAudio);
    }

    public void Stop()
    {
        running = false;
        audio.SetSink(null);
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
                    if (!running)
                    {
                        Array.Clear(data);
                    }
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

    byte[] b5 = new byte[5];
    private void GenerateChunk()
    {
        if (morseKeyLeft == 0)
        {
            morseKeyLeft = 5;
            morseState = morse.GetKey();
        }
        if (frameDataLeft == 0)
        {
            frameDataLeft = 500;
            if (nextHeader == 0)
            {
                Console.WriteLine($"Sending 0/{frame.Length - 1}");
                nextHeader = 20;
                frameData = frame.GetFrame(0);
            }
            else
            {
                if (currentFrame == frame.Length)
                {
                    Stop();
                    return;
                }
                Console.WriteLine($"Sending {currentFrame}/{frame.Length - 1}");
                frameData = frame.GetFrame(currentFrame);
                nextHeader--;
                currentFrame++;
            }

        }
        morseKeyLeft--;
        Array.Copy(frameData, frameData.Length - frameDataLeft, b5, 0, 5);
        Complex[] fftInput = Constellation.GetData(morseState, frameDataLeft == 500, b5);
        frameDataLeft -= 5;
        audioData = FFT.CalcIFFT(fftInput);
    }


}