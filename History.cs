using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace WallpaperFetch
{
    class History
    {
        private readonly string _historyFolder;

        public History(string historyFolder)
        {
            _historyFolder = historyFolder;
        }

        private string GetFileName(DateOnly day)
        {
            return Path.Join(_historyFolder, day.ToShortDateString() + ".png");
        }

        public bool HasEntryForDay(DateOnly day)
        {
            return File.Exists(GetFileName(day));
        }

        public bool HasEntryForToday()
        {
            return HasEntryForDay(DateOnly.FromDateTime(DateTime.Today));
        }

        public async Task SaveAsync(byte[] imageData)
        {
            using var stream = new MemoryStream(imageData);
            using var img = await Image.LoadAsync(stream);
            string path = GetFileName(DateOnly.FromDateTime(DateTime.Today));
            await img.SaveAsync(path,
                new PngEncoder()
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    ColorType = PngColorType.Rgb
                });
        }
    }
}