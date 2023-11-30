using System;
using System.Diagnostics;

namespace DarkSSTV;

interface IAudio
{
    void SetSource(Action<double[]> source);
    void SetSink(Action<double[]> sink);
    void Stop();
}