using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace DarkSSTV;
class FrameSyncroniser
{
    FrameDecoder decoder;
    IAudio audio;
    bool cyclic = true;
    int audioPos = 448;
    Complex[] samples = new Complex[512];

    public FrameSyncroniser(FrameDecoder decoder, IAudio audio)
    {
        this.decoder = decoder;
        this.audio = audio;
        audio.SetSource(AudioEvent);
    }

    //TODO: Write a syncroniser. This works from the wav file where everything is aligned and coherent.
    private void AudioEvent(double[] data)
    {
        if (cyclic)
        {
            audioPos += data.Length;
            if (audioPos == samples.Length)
            {
                audioPos = 0;
                cyclic = false;
            }
            return;
        }
        for (int i = 0; i < data.Length; i++)
        {
            samples[audioPos++] = data[i];
        }
        if (audioPos == samples.Length)
        {
            decoder.FrameEvent(samples);
            audioPos = 448;
            cyclic = true;
        }
    }
}