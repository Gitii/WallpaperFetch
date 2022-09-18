namespace WallpaperFetch;

interface IImageSourceHandler
{
    public Task<byte[]> GetImage(ImageSource source, string category, string? imageName);
}