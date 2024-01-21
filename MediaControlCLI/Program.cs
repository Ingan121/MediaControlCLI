using WindowsMediaController;
using static WindowsMediaController.MediaManager;

string[] arg = Environment.GetCommandLineArgs();
if (arg.Length != 2 && arg.Length != 3)
{
    PrintHelp();
}

if (arg[1].ToLower() == "help")
{
    PrintHelp();
}

string command = arg[1].ToLower();
bool anySession = false;

MediaManager mediaManager = new MediaManager();
mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
mediaManager.Start();

if (!anySession)
{
    if (arg.Length == 3)
    {
        Console.WriteLine("No media players found with the name " + arg[2]);
    }
    else
    {
        Console.WriteLine("No media players found");
    }
}

void MediaManager_OnAnySessionOpened(MediaSession mediaSession)
{
    var songInfo = mediaSession.ControlSession.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();
    if (arg.Length == 3)
    {
        if (mediaSession.Id.ToLower() != arg[2].ToLower() && songInfo.Title.ToLower() != arg[2].ToLower())
        {
            return;
        }
    }

    var controlsInfo = mediaSession.ControlSession.GetPlaybackInfo().Controls;
    switch (command)
    {
        case "play":
            if (controlsInfo.IsPlayEnabled)
            {
                _ = mediaSession.ControlSession.TryPlayAsync();
            }
            else
            {
                Console.WriteLine("Play is not enabled for " + mediaSession.Id);
            }
            break;
        case "pause":
            if (controlsInfo.IsPauseEnabled)
            {
                _ = mediaSession.ControlSession.TryPauseAsync();
            }
            else
            {
                Console.WriteLine("Pause is not enabled for " + mediaSession.Id);
            }
            break;
        case "playpause":
            if (controlsInfo.IsPauseEnabled || controlsInfo.IsPlayEnabled)
            {
                _ = mediaSession.ControlSession.TryTogglePlayPauseAsync();
            }
            else
            {
                Console.WriteLine("Play/Pause is not enabled for " + mediaSession.Id);
            }
            break;
        case "stop":
            if (controlsInfo.IsStopEnabled)
            {
                _ = mediaSession.ControlSession.TryStopAsync();
            }
            else
            {
                Console.WriteLine("Stop is not enabled for " + mediaSession.Id);
            }
            break;
        case "prev":
            if (controlsInfo.IsPreviousEnabled)
            {
                _ = mediaSession.ControlSession.TrySkipPreviousAsync();
            }
            else
            {
                Console.WriteLine("Previous is not enabled for " + mediaSession.Id);
            }
            break;
        case "next":
            if (controlsInfo.IsNextEnabled)
            {
                _ = mediaSession.ControlSession.TrySkipNextAsync();
            }
            else
            {
                Console.WriteLine("Next is not enabled for " + mediaSession.Id);
            }
            break;
        case "print":
            Console.WriteLine("Media from " + mediaSession.Id);
            Console.WriteLine();
            Console.WriteLine("Title: " + songInfo.Title);
            Console.WriteLine("Subtitle: " + songInfo.Subtitle);
            Console.WriteLine("Artist: " + songInfo.Artist);
            Console.WriteLine("Album: " + songInfo.AlbumTitle);
            Console.WriteLine("Album Artist: " + songInfo.AlbumArtist);
            Console.WriteLine("Track Number: " + songInfo.TrackNumber);
            Console.WriteLine("Track Count: " + songInfo.AlbumTrackCount);
            Console.WriteLine("Genres: " + string.Join(", ", songInfo.Genres));
            Console.WriteLine();
            break;
        default:
            PrintHelp();
            break;
    }
    anySession = true;
}

void PrintHelp()
{
    Console.WriteLine("MediaControlCLI v1.0");
    Console.WriteLine("Made by Ingan121");
    Console.WriteLine("Licensed under the MIT License");
    Console.WriteLine("https://github.com/Ingan121/MediaControlCLI");
    Console.WriteLine();
    Console.WriteLine("Usage: " + Path.GetFileName(arg[0]) + " [command] ([match])");
    Console.WriteLine("Commands:");
    Console.WriteLine("  help - Show this help");
    Console.WriteLine("  play - Play media");
    Console.WriteLine("  pause - Pause media");
    Console.WriteLine("  playpause - Play/Pause media");
    Console.WriteLine("  stop - Stop media");
    Console.WriteLine("  prev - Previous media");
    Console.WriteLine("  next - Next media");
    Console.WriteLine("  print - Print media info");
    Console.WriteLine();
    Console.WriteLine("Match:");
    Console.WriteLine("  One of the following:");
    Console.WriteLine("    Process name of the player");
    Console.WriteLine("    Title of the media");
    Console.WriteLine();
    Console.WriteLine("If match is not specified, the command will be executed on all players.");
    Console.WriteLine();
    Environment.Exit(1);
}