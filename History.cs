using System.Collections.Immutable;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace WallpaperFetch;

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
        using var img = await Image.LoadAsync(stream).ConfigureAwait(false);
        string path = GetFileName(DateOnly.FromDateTime(DateTime.Today));
        await img.SaveAsync(
                path,
                new PngEncoder()
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    ColorType = PngColorType.Rgb
                }
            )
            .ConfigureAwait(false);
    }

    public void Cleanup(int maxHistoryLength)
    {
        var files = Directory
            .GetFiles(_historyFolder)
            .OrderByDescending(ToSortValue)
            .Skip(maxHistoryLength)
            .ToImmutableList();

        foreach (var file in files)
        {
            File.Delete(file);
        }
    }

    private int ToSortValue(string path)
    {
        var dateString = Path.GetFileNameWithoutExtension(path);

        if (dateString == null)
        {
            throw new Exception($"invalid file path: {path}");
        }

        if (DateOnly.TryParse(dateString, out var date))
        {
            return date.Year * 1000 + date.DayOfYear;
        }

        throw new Exception($"invalid file name: not a valid DateOnly: {dateString}");
    }
}
