using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TaskbarLauncher.Models
{
    public class AppConfig
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";

        public BitmapSource? Icon
        {
            get
            {
                if (!File.Exists(Path))
                    return null;

                try
                {
                    var icon = System.Drawing.Icon.ExtractAssociatedIcon(Path);
                    if (icon == null) return null;

                    return Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}