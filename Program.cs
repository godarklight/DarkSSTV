using System;
using Gtk;

namespace DarkSSTV
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Encoder te = new Encoder();
            te.Run();

            Application.Init();

            var app = new Application("org.DarkSSTV.DarkSSTV", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();

            te.Stop();
        }
    }
}
