# WallpaperFetch

A simple application for windows that downloads image from a remote source and set's it as filling wallpaper.

# How to install

- Download latest release from github

## If you want to change your wallpaper daily:

- On the commandline execute  
  `WallpaperFetch.exe cron --install`.  
  `WallpaperFetch` will install itself to `ApplicationData` and set up a cron job.
- Modify the `~/.wallpaper.json` file to your likeing.  
  By default, the awesome wallpapers from [mack](https://github.com/makccr/wallpapers) are used.

## If you want to change your wallpaper only once

_not recommended, you can do that on your own using the GUI_

- On the commandline execute `WallpaperFetch.exe set`. `WallpaperFetch` will install itself to `ApplicationData` but **not** set up a cron job.
- Modify the `~/.wallpaper.json` file to your likeing.
  By default, the awesome wallpapers from [mack](https://github.com/makccr/wallpapers) are used.

# How to uninstall

On the commandline (cmd) execute these commands:

```
%appdata%/WallpaperFetch/WallpaperFetch.exe cron --uninstall
rmdir %appdata%/WallpaperFetch
del %userprofile%/.wallpaper.json
```

# Thanks to [mack](https://github.com/makccr)

Big thank you to mack for sharing his awesome wallpapers with us! ♥️
