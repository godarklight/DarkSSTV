using System;
using System.IO;
using System.Text;
using Gtk;

namespace DarkSSTV
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            //AudioDriver audio = new AudioDriver();
            WavDriver audio = new WavDriver();
            //Encode path
            /*
            byte[] data = File.ReadAllBytes("input.jpg");
            Morse morse = new Morse("VK4GDL TESTING DARKSSTV");
            FrameEncoder frame = new FrameEncoder();
            frame.Encode(data, "VK4GDL", "meme.png", "haha funny");
            Encoder encoder = new Encoder(morse, frame, audio);
            encoder.Run();
            */

            //Decode path
            FrameDecoder decoder = new FrameDecoder();
            FrameSyncroniser frameSync = new FrameSyncroniser(decoder, audio);

            audio.Run();

            Application.Init();

            var app = new Application("org.DarkSSTV.DarkSSTV", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();

            audio.Stop();
        }
    }
}
