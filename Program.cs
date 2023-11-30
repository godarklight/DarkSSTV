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
            byte[] data = File.ReadAllBytes("input.jpg");
            //AudioDriver audio = new AudioDriver();
            WavDriver audio = new WavDriver();
            Morse morse = new Morse("VK4GDL TESTING DARKSSTV");
            FrameEncoder frame = new FrameEncoder();
            frame.Encode(data, "VK4GDL", "meme.png", "haha funny");
            Encoder encoder = new Encoder(morse, frame, audio);
            encoder.Run();
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
