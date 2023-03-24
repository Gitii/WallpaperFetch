using Microsoft.Win32;
using System.Runtime.InteropServices;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using Windows.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using Windows.System.UserProfile;
using ABI.Windows.ApplicationModel.LockScreen;
using SixLabors.ImageSharp.Processing;

namespace WallpaperFetch;

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

        Fill = 10, // The image is resized and cropped to fill the screen while maintaining the aspect ratio. (Windows 7 and later)
        Fit = 6, // The image is resized to fit the screen while maintaining the aspect ratio
    }

    public static async Task SetDesktopWallpaperAsync(byte[] imageData, Style style)
    {
        using var stream = new MemoryStream(imageData);
        using var img = await Image.LoadAsync(stream).ConfigureAwait(false);
        string tempPath = Path.Combine(Path.GetTempPath(), "wallpaperfetch-wallpaper.bmp");
        await img.SaveAsync(tempPath, new BmpEncoder()).ConfigureAwait(false);

        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true)!;
        key.SetValue("WallpaperStyle", ((int)style).ToString());
        key.SetValue("TileWallpaper", (style == Style.Tiled ? 1 : 0).ToString());

        SystemParametersInfo(
            SPI_SETDESKWALLPAPER,
            0,
            tempPath,
            SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE
        );
    }

    public static async Task SetLockScreenWallpaperAsync(byte[] imageData)
    {
        using var stream = new MemoryStream(imageData);
        using var img = await Image.LoadAsync(stream).ConfigureAwait(false);

        var maxSize = await GetMaxMonitorResolutionAsync().ConfigureAwait(false);
        if (!maxSize.IsEmpty)
        {
            var bestSize = GetOptimalSize(img.Width, img.Height, maxSize);
            img.Mutate((x) => x.Resize(bestSize.Width, bestSize.Height));
        }

        string tempPath = Path.Combine(Path.GetTempPath(), "wallpaperfetch-lockscreen.bmp");
        await img.SaveAsync(tempPath, new BmpEncoder()).ConfigureAwait(false);

        var file = await StorageFile.GetFileFromPathAsync(tempPath);

        await LockScreen.SetImageFileAsync(file);
    }

    static Size GetOptimalSize(int width, int height, Size frameSize)
    {
        double widthRatio = (double)frameSize.Width / (double)width;
        double heightRatio = (double)frameSize.Height / (double)height;
        double ratio = Math.Min(widthRatio, heightRatio);
        int optimalWidth = (int)(width * ratio);
        int optimalHeight = (int)(height * ratio);
        return new Size(optimalWidth, optimalHeight);
    }


    private static async Task<Size> GetMaxMonitorResolutionAsync()
    {
        const string DEVICE_INSTANCE_ID = "System.Devices.DeviceInstanceId";
        var displayInformationList = await DeviceInformation.FindAllAsync(DisplayMonitor.GetDeviceSelector(), new[]
        {
            DEVICE_INSTANCE_ID
        });

        var size = Size.Empty;

        foreach (var deviceInformation in displayInformationList.Where((di) =>
                     di.Kind == DeviceInformationKind.DeviceInterface))
        {
            DisplayMonitor monitor = await
                DisplayMonitor.FromIdAsync((string)deviceInformation.Properties[DEVICE_INSTANCE_ID]);

            var monitorSize = monitor.NativeResolutionInRawPixels;
            if (monitorSize.Height > size.Height || monitorSize.Width > size.Width)
            {
                size = new Size(monitorSize.Width, monitorSize.Height);
            }
        }

        return size;
    }
}
