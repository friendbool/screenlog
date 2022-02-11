using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace ScreenLog
{
    class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            string mutexId = $"Global\\{{{appGuid}}}";
            using (Mutex mutex = new Mutex(false, mutexId))
            {
                if (mutex.WaitOne(TimeSpan.Zero))
                {
                    while (true)
                    {
                        try
                        {
                            DateTime now = DateTime.Now;
                            string file3 = ConfigurationManager.AppSettings["Folder"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ScreenLog");
                            file3 = Path.Combine(file3, now.ToString("yyyyMMdd"));
                            file3 = Path.Combine(file3, now.ToString("HHmmss") + ".png");
                            int cont = 1;
                            while (File.Exists(file3))
                            {
                                file3 = Path.GetFileNameWithoutExtension(file3) + "_" + cont + Path.GetExtension(file3);
                                cont++;
                            }
                            CapureScreen(file3);
                        }
                        catch (Exception ex)
                        {
                            EventLog.WriteEntry("Application", "ScreenLog\r\n\r\n" + ex.ToString(), EventLogEntryType.Error, 99);
                        }
                        Thread.Sleep(300000);
                    }
                }
            }
        }

        public static void CapureScreen(string file)
        {
            System.Windows.Media.Matrix toDevice;
            using (var source = new HwndSource(new HwndSourceParameters()))
            {
                toDevice = source.CompositionTarget.TransformToDevice;
            }
            var screenWidth = (int)Math.Round(SystemParameters.PrimaryScreenWidth * toDevice.M11);
            var screenHeight = (int)Math.Round(SystemParameters.PrimaryScreenHeight * toDevice.M22);

            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            int width = bounds.Width;
            bounds = Screen.PrimaryScreen.Bounds;
            using (var bmpScreenshot = new Bitmap(width, bounds.Height, PixelFormat.Format32bppArgb))
            using (var graphics = Graphics.FromImage(bmpScreenshot))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));
                string dir = Path.GetDirectoryName(file);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                bmpScreenshot.Save(file, ImageFormat.Png);
            }                
        }
    }
}
