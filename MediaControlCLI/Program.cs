using WindowsMediaController;
using static WindowsMediaController.MediaManager;

string command = "";
string player = "";
bool interactive = false;
bool anySession = false;
bool success = false;

if (args.Length > 2)
{
    PrintHelp();
}

MediaManager mediaManager = new MediaManager();

if (args.Length > 0)
{
    if (args[0].ToLower().Contains("help") || args[0].Contains("?") || args[0].ToLower() == "/h" || args[0].ToLower() == "-h")
    {
        PrintHelp();
    }
    string cmd = args[0];
    if (cmd.ToLower() == "utf8")
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        InteractiveMode();
    }

    if (args.Length > 1)
    {
        cmd += " " + args[1];
    }

    ProcessCommand(cmd);
    Environment.Exit(success ? 0 : 1);
}
else
{
    InteractiveMode();
}

async void MediaManager_OnAnySessionOpened(MediaSession mediaSession)
{
    var songInfo = mediaSession.ControlSession.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();
    
    if (player != "")
    {
        if (mediaSession.Id.ToLower() != player.ToLower() && songInfo.Title.ToLower() != player.ToLower())
        {
            return;
        }
    }
    anySession = true;

    var controlsInfo = mediaSession.ControlSession.GetPlaybackInfo().Controls;
    switch (command)
    {
        case "play":
            if (controlsInfo.IsPlayEnabled)
            {
                await mediaSession.ControlSession.TryPlayAsync();
                success = true;
            }
            else
            {
                Console.WriteLine("Play is not enabled for " + mediaSession.Id);
            }
            break;
        case "pause":
            if (controlsInfo.IsPauseEnabled)
            {
                await mediaSession.ControlSession.TryPauseAsync();
                success = true;
            }
            else
            {
                Console.WriteLine("Pause is not enabled for " + mediaSession.Id);
            }
            break;
        case "playpause":
            if (controlsInfo.IsPauseEnabled || controlsInfo.IsPlayEnabled)
            {
                await mediaSession.ControlSession.TryTogglePlayPauseAsync();
                success = true;
            }
            else
            {
                Console.WriteLine("Play/Pause is not enabled for " + mediaSession.Id);
            }
            break;
        case "stop":
            if (controlsInfo.IsStopEnabled)
            {
                await mediaSession.ControlSession.TryStopAsync();
                success = true;
            }
            else
            {
                Console.WriteLine("Stop is not enabled for " + mediaSession.Id);
            }
            break;
        case "prev":
            if (controlsInfo.IsPreviousEnabled)
            {
                await mediaSession.ControlSession.TrySkipPreviousAsync();
                success = true;
            }
            else
            {
                Console.WriteLine("Previous is not enabled for " + mediaSession.Id);
            }
            break;
        case "next":
            if (controlsInfo.IsNextEnabled)
            {
                await mediaSession.ControlSession.TrySkipNextAsync();
                success = true;
            }
            else
            {
                Console.WriteLine("Next is not enabled for " + mediaSession.Id);
            }
            break;
        case "print":
            var playbackInfo = mediaSession.ControlSession.GetPlaybackInfo();
            var positionInfo = mediaSession.ControlSession.GetTimelineProperties();
            
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
            Console.WriteLine("Media Type: " + playbackInfo.PlaybackType);
            Console.WriteLine();
            Console.WriteLine("Playback Status: " + playbackInfo.PlaybackStatus);
            Console.WriteLine("Playback Rate: " + playbackInfo.PlaybackRate);
            Console.WriteLine("Playback Position: " + positionInfo.Position.ToString("hh':'mm':'ss") + " / " + positionInfo.EndTime.ToString("hh':'mm':'ss"));
            Console.WriteLine();
            Console.WriteLine("Shuffle: " + playbackInfo.IsShuffleActive);
            Console.WriteLine("Repeat: " + playbackInfo.AutoRepeatMode);
            Console.WriteLine();
            success = true;
            break;
        default:
            Console.WriteLine("Unknown command: " + command);
            if (interactive)
            {
                Console.WriteLine("Type \"help\" for help");
            }
            else
            {
                Console.WriteLine("Run \"MediaControlCLI help\" for help");
            }
            Console.WriteLine();
            mediaManager.OnAnySessionOpened -= MediaManager_OnAnySessionOpened;
            break;
    }
}

void InteractiveMode()
{
    interactive = true;
    Console.Title = "MediaControlCLI";
    PrintInfo();
    while (true)
    {
        Console.Write("MediaControlCLI> ");
        string? cmd = Console.ReadLine();
        switch (cmd!.ToLower())
        {
            case "help":
                PrintHelp();
                break;
            case "exit":
                Environment.Exit(0);
                break;
            default:
                ProcessCommand(cmd);
                break;
        }
    }
}

void ProcessCommand(string cmd)
{
    string[] cmdSplit = cmd.Split(" ", 2);
    command = cmdSplit[0].ToLower();
    if (cmdSplit.Length > 1)
    {
        player = cmdSplit[1];
    }
    else
    {
        player = "";
    }
    success = false;
    anySession = false;

    mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;

    try
    {
        mediaManager.Start();
    }
    catch (TypeInitializationException)
    {
        Console.WriteLine("Media controls are not supported on this system.");
        Environment.Exit(2);
    }

    if (!anySession)
    {
        if (player != "")
        {
            Console.WriteLine("No media players found with the name " + player);
            Console.WriteLine("Type \"print\" to print info about all available media players.");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("No media players found");
            Console.WriteLine();
        }
    }

    mediaManager.Dispose();
}

void PrintHelp()
{
    if (!interactive)
    {
        PrintInfo();
        Console.WriteLine("Usage: MediaControlCLI [command] ([match])");
    }
    else
    {
        Console.WriteLine("Usage: ([command]) ([match])");
    }
    Console.WriteLine("Commands:");
    Console.WriteLine("  help - Show this help");
    Console.WriteLine("  play - Play media");
    Console.WriteLine("  pause - Pause media");
    Console.WriteLine("  playpause - Play/Pause media");
    Console.WriteLine("  stop - Stop media");
    Console.WriteLine("  prev - Previous media");
    Console.WriteLine("  next - Next media");
    Console.WriteLine("  print - Print media info");
    if (interactive)
    {
        Console.WriteLine("  exit - Exit");
    }
    else
    {
        Console.WriteLine("  utf8 - Enter interactive mode with UTF-8 encoding");
    }
    Console.WriteLine();
    Console.WriteLine("Match:");
    Console.WriteLine("  One of the following:");
    Console.WriteLine("    Process name of the player");
    Console.WriteLine("    Title of the media");
    Console.WriteLine();
    Console.WriteLine("If match is not specified, the command will be executed on all players.");
    if (!interactive)
    {
        Console.WriteLine("If command is not specified, the program will enter interactive mode.");
    }
    Console.WriteLine();
    if (!interactive)
    {
        Environment.Exit(1);
    }
}

void PrintInfo()
{
    Console.WriteLine("MediaControlCLI v1.0");
    Console.WriteLine("Made by Ingan121");
    Console.WriteLine("Licensed under the MIT License");
    Console.WriteLine("https://github.com/Ingan121/MediaControlCLI");
    Console.WriteLine();
}