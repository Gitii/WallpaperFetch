using Microsoft.Win32;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;

namespace WallpaperFetch
{
    public static class Wallpaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled = 0, // The wallpaper picture should be tiled
            Centered = 0, // The image is centered if TileWallpaper=0 or tiled if TileWallpaper=1
            Stretched = 2, // The image is stretched to fill the screen

            Fill =
                10, // The image is resized and cropped to fill the screen while maintaining the aspect ratio. (Windows 7 and later)
            Fit = 6, // The image is resized to fit the screen while maintaining the aspect ratio
        }

        public static async Task SetDesktopWallpaper(byte[] imageData, Style style)
        {
            using var stream = new MemoryStream(imageData);
            using var img = await Image.LoadAsync(stream);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaperfetch-wallpaper.bmp");
            await img.SaveAsync(tempPath, new BmpEncoder());

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true)!;
            key.SetValue("WallpaperStyle", ((int)style).ToString());
            key.SetValue("TileWallpaper", (style == Style.Tiled ? 1 : 0).ToString());

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}