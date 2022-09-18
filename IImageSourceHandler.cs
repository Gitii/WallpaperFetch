namespace WallpaperFetch;

interface IImageSourceHandler
{
    public Task<byte[]> GetImageAsync(ImageSource source, string category, string? imageName);
}
