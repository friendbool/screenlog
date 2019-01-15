using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ScreenLog
{
    class Program
    {
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
                        bool flag = true;
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
                            CaputeScreen(file3);
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

        public static void CaputeScreen(string file)
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            int width = bounds.Width;
            bounds = Screen.PrimaryScreen.Bounds;
            Bitmap bmpScreenshot = new Bitmap(width, bounds.Height, PixelFormat.Format32bppArgb);
            Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            Graphics graphics = gfxScreenshot;
            bounds = Screen.PrimaryScreen.Bounds;
            int x = bounds.X;
            bounds = Screen.PrimaryScreen.Bounds;
            int y = bounds.Y;
            bounds = Screen.PrimaryScreen.Bounds;
            graphics.CopyFromScreen(x, y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            string dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            bmpScreenshot.Save(file, ImageFormat.Png);
            bmpScreenshot.Dispose();
            gfxScreenshot.Dispose();
        }
    }
}
