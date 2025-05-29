using System.ComponentModel.Design;
using System.Diagnostics;                           
using System.Net;                                      
using System.Runtime.InteropServices;                 //Feel free to do whatever with my code, my project
using System.Security.Principal;                      //just kept growing and i wasn't anticipating it.
using System.Text.RegularExpressions;                 //All I ask is that you leave my signatures
using Microsoft.Win32;
using NAudio.CoreAudioApi;                         
using NAudio.Wave;
using Newtonsoft.Json.Linq;


namespace Mullet_Media_Strike_6._9
{
    class Program
    {
        static HttpListener? listener;        //no i'm not sorry about the variable hell and graveyard, i embrace it!
        static bool isTargetMuted = false;    
        static int keystrokesCounter = 0;
        static int sessionWebhookCount = 0;
        static string counterFilePath = "keystrokes_counter.txt";
        static string portFilePath = "webhook_port.txt";
        static int webhookPort = 1337;
        static string selectedProcessName = "";
        static string statusText = "unmuted";
        static bool exitRequested = false;
        static string playerStatus = "waiting...";
        static string phaseStatus = "waiting...";
        static string activityStatus = "waiting...";
        static string providerSteamidStatus = "";
        static string playerSteamidStatus = "";
        static int counterStatus = 0;
        static bool webhookConnectStatus = false;
        static string gameModeStatus = "waiting...";
        static string competitiveMode = "enabled";
        static bool loop = true;
        static string mainMenuColor = "white";
        static bool validKey = true;
        static bool gray = false;
        static string csFound = "";
        static string csPortSet = "";
        static string autoConfigCSRunning = "";
        static bool portConvert;
        static int foundPortParsed;
        static string FoundSuccess = "";
        static bool isAdmin = IsRunAsAdmin();
        static string freezetimeStatus = "";
        static int defaultMinVolume = 25;
        static int defaultMaxVolume = 75;
        static int connectedRunOnce = 0;
        static int currentVolume = 0;
        static bool hasShownConnectedStatus = false;
        static int lastCheckedWebhookCount = -1;
        static bool hasConnectedOnce = false;
        static string lastStatusMessage = "";
        static bool firstWebhookReceived = false;
        static DateTime lastWebhookTime = DateTime.MinValue;
        static bool isConnected = false;
        static string configFilePathForAutoConfig = "";
        static bool foundStatus = false;
        static string? foundPort = null;
        static int newPort = 0;
        static bool gamestateFileFound = false;
        static string configFilePathPull = "";
        static bool cfgRecentlyUpdated = false;
        static bool fadeInAndOut = true;
        static bool WebHookConnected = false;
        static int hiddenCounter = 0;
        static bool mainScreen = false;
        static string killDeathAssistRatio = "";
        static string fadeStatus = "enabled";
        static int defaultFade = 1000;
        static bool mediaKeysEnabled;
        static string mediaKeys => mediaKeysEnabled ? "enabled" : "disabled";
        static bool csWasFoundByAutoConfiguration = false;

        static int lastTargetVolume;
        static bool lastMuteState = false;

        // Session cumulative stats
        static int sessionKills = 0;
        static int sessionAssists = 0;
        static int sessionDeaths = 0;

        // Last known match stats
        static int lastKills = 0;
        static int lastAssists = 0;
        static int lastDeaths = 0;
        static string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");

        static string mainMenu;

        static bool cfgIsOpen = false;
        static bool freezetimeactive;
        static int targetVolume;
        static string activityFailSafe = "";
        static string phaseFailSafe = "";
        static bool mapPhaseActive = false;
        static float prevVolume;
        static string lastRound = "";
        static bool playerDiedThisRound;
        static bool playerAliveThisRound;
        static bool playerIsAlive;
        static bool menuOverride;
        static bool shouldMute = false;
        static bool resetRequested = false;
        static bool firstRun = true;
        static float currentVolume2;
        static string volumeVisualized = "";
        static string howmanybars = "";
        static int startUpVolume;
        static bool exiting = false;
        static string killStreakTextForUpdateLoop = "";
        static bool playedDoubleKill = false;
        static bool playedMultiKill = false;
        static bool playedUltraKill = false;
        static bool playedMonsterKill = false;
        static string killStreakText = "";
        static bool runOnce = false;
        static int newDeaths = 0;
        static string soundAlerts = "disabled(DM)";

        enum ProgramState
        {
            Menu,
            Running,
            Exit
        }

        static ProgramState currentState = ProgramState.Menu;

        static void Main(string[] args)
        {
            if (listener == null || !listener.IsListening)
            {
                StartWebhook();
            }
            LoadSettings();
            Console.CursorVisible = false;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "=== Mullet Media-Strike 6.9 ===";
            //AppDomain.CurrentDomain.ProcessExit += (sender, e) => SaveSettings();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            LoadCounter();
            LoadPort();
            IsRunAsAdmin();
            SplashScreen();
            while (currentState != ProgramState.Exit)
            {
                switch (currentState)
                {
                    case ProgramState.Menu:
                        EnsureWebhookIsRunning();
                        DisplayMenu();
                        break;

                    case ProgramState.Running:
                        EnsureWebhookIsRunning();
                        Console.Clear();
                        PrintHeader();
                        UpdateDisplayLoop();
                        break;
                }
            }
        }
        static void OnProcessExit(object sender, EventArgs e)
        {

            exiting = true;
            targetVolume = startUpVolume;
            AudioControl.FadeToVolume(selectedProcessName, targetVolume, defaultFade, 10);
            Thread.Sleep(defaultFade);
        }

        static void SaveStartUpVolume()
        {
            float startUpVolumeFloat = AudioControl.GetAppVolume($"{selectedProcessName}");
            startUpVolume = (int)Math.Round(startUpVolumeFloat);
        }

        static void DisplayMenuGray(string mainMenu)
        {
            Console.CursorVisible = false;
            //EnsureWebhookIsRunning();
            Console.Clear();
            PrintHeader();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(mainMenu);
            Thread.Sleep(100);
            Console.Clear();
            PrintHeader();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(mainMenu);
            Thread.Sleep(100);
            Console.ResetColor();
        }

        static void DisplayMenu()
        {
            while (true)
            {
                Thread.Sleep(50);
                ClearKeyBuffer();
                //EnsureWebhookIsRunning();
                Console.CursorVisible = true;
                mainScreen = false;
                Console.Clear();
                PrintHeader();
                GetPortFromConfig();

                /*string mainMenu =
@$"Select audio app to mute when alive:
[1] Spotify
[2] Chrome
[3] VLC
[4] Custom (e.g winamp.exe, foobar2000.exe)

Automatic configuration:
[5] Automatic Port Configuration

Additional options:
[6] Volume (min, max, fade)
[7] Media Keys: {mediaKeys}
[8] Webhook Port Change (current: {webhookPort})
[9] Readme & Instructions
[0] Restore Defaults";*/

                string mainMenu =
@$"Select audio app to mute when alive:
[1] Spotify
[2] Chrome
[3] VLC
[4] Custom (e.g winamp.exe, foobar2000.exe)

Port configuration:
[5] Automatic Port Configuration
[6] Webhook Port Change (current: {webhookPort})

Settings & Features:
[7] Sound Settings
[8] Help & Restore Defaults";
                if (validKey == true)
                {
                    TypeWithBlockTrail(mainMenu, 1);
                }
                else if (validKey == false && gray == false)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(mainMenu);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(mainMenu);
                    Console.ResetColor();
                }

                portConvert = int.TryParse(foundPort, out foundPortParsed);
                if (foundPortParsed == webhookPort && cfgRecentlyUpdated == false)
                {
                    Console.WriteLine("\nPort Auto-Detection Helper:");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"[✓] Ports matched: [{webhookPort}] = [{foundPort}]");
                    Console.WriteLine($"[✓] You're all set! - Just select a media app to begin.");
                    Console.ResetColor();
                }
                else if (foundPortParsed == webhookPort && cfgRecentlyUpdated == true && webhookConnectStatus == true && (Process.GetProcessesByName("cs2").Any() || Process.GetProcessesByName("csgo").Any()))
                {
                    Console.WriteLine("\nPort Auto-Detection Helper:");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[!] You have recently changed ports with Counter-Strike running!");
                    Console.WriteLine($"[!] Please make sure you restart Counter-Strike whenever you change ports.");
                    cfgRecentlyUpdated = false; // Reset flag after the warning
                    Console.ResetColor();
                }
                else if (foundPortParsed != webhookPort && foundPort != null)
                {
                    Console.WriteLine("\nPort Auto-Detection Helper:");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    //Console.WriteLine($"[✘] Mullet Media-Strike port {webhookPort} =/= gamestate_integration_media.cfg port: {foundPort}");
                    Console.WriteLine($@"[port: {webhookPort}] <- Mullet Media-Strike 6.9
[port: {foundPort}] <- gamestate_integration_media.cfg");
                    Console.WriteLine($"[✘] Port mismatch detected, ports need to match!");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"\nSuggested solutions:");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(
$@"[5] Automatic Port Configuration (recommended)
[6] Webhook Port Change (recommended)
[8] Help -> Readme & Instructions (for manual change)");
                    Console.ResetColor();
                }
                else if (foundPortParsed != webhookPort && foundPort == null && firstRun == false)
                {
                    Console.WriteLine("\nPort Auto-Detection Helper:");
                    Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine($"[✘] Mullet Media-Strike port {webhookPort} =/= gamestate_integration_media.cfg port: {foundPort}");
                    Console.WriteLine($@"[✘] Your gamestate_integration_media.cfg seems to be missing.");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"\nSuggested solutions:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(
$@"[5] Automatic Port Configuration (recommended)
[6] Webhook Port Change (recommended)
[8] Help -> Readme & Instructions (for manual change))");
                    Console.ResetColor();
                }
                else if (foundPortParsed != webhookPort && foundPort == null && firstRun == true)
                {
                   
                        Console.WriteLine("\nPort Auto-Detection Helper:");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        //Console.WriteLine($"[✘] Mullet Media-Strike port {webhookPort} =/= gamestate_integration_media.cfg port: {foundPort}");
                        Console.WriteLine($@"[✘] Your gamestate_integration_media.cfg seems to be missing.");
                        Console.ResetColor();
                        for (int i = 0; i < 20; i++)
                        {
                        Console.SetCursorPosition(0, 20);
                        if (Console.KeyAvailable) break;
                        Console.ForegroundColor = (i % 2 == 0) ? ConsoleColor.DarkGray : ConsoleColor.White;
                        Console.WriteLine($"\nSuggested solutions:");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("No worries :) - don't be alarmed if this is your first boot-up.");
                        Console.WriteLine("Just press [5] to generate one automatically and get started fast!");
                        Thread.Sleep(400);
                        firstRun = false;
                        }
                }
                //Console.ForegroundColor = ConsoleColor.DarkGray;
                //Console.WriteLine($"\nCurrent Settings:");
                //Console.WriteLine($"Volume: min {defaultMinVolume}, max {defaultMaxVolume}, fade {defaultFade} | Media Keys: {mediaKeys} | Announcer: Enabled");
                //Console.WriteLine($"Media Keys: {mediaKeys}");
                //Console.WriteLine($"Announcer: Enabled");
                Console.ResetColor();
                bool validInput = false;
                while (!validInput)
                {
                    Console.CursorVisible = false;
                    //EnsureWebhookIsRunning();
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.NoName || key == ConsoleKey.VolumeUp || key == ConsoleKey.VolumeDown || key == ConsoleKey.VolumeMute)
                    {
                        // skip over volume scroll or invalid inputs
                        continue;
                    }
                    switch (key)
                    {
                        case ConsoleKey.D1:
                            mainScreen = true;
                            validInput = true;
                            if (listener == null || !listener.IsListening) // only start if not already running
                            {
                                StartWebhook();
                            }
                            SoundPlayer.LaunchSelect();
                            selectedProcessName = "spotify.exe";
                            SaveStartUpVolume();
                            validKey = true;
                            currentState = ProgramState.Running;
                            SaveSettings();
                            return;
                        case ConsoleKey.D2:
                            mainScreen = true;
                            validInput = true;
                            SoundPlayer.LaunchSelect();
                            selectedProcessName = "chrome.exe";
                            validKey = true;
                            currentState = ProgramState.Running;
                            SaveSettings();
                            return;
                        /*case ConsoleKey.D3:
                            selectedProcessName = "foobar2000.exe";
                            validKey = true;
                            currentState = ProgramState.Running;
                            return;*/
                        case ConsoleKey.D3:
                            mainScreen = true;
                            validInput = true;
                            SoundPlayer.LaunchSelect();
                            selectedProcessName = "vlc.exe";
                            validKey = true;
                            currentState = ProgramState.Running;
                            SaveSettings();
                            return;
                        case ConsoleKey.D4:
                            mainScreen = true;
                            validInput = true;
                            SoundPlayer.LaunchSelect();
                            while (true)
                            {
                                Console.CursorVisible = true;
                                DisplayMenuGray(mainMenu);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("\nEnter name of currently running program (e.g. chrome.exe): ");
                                selectedProcessName = Console.ReadLine()?.Trim() ?? "";
                                Console.ResetColor();
                                string message3 =
    @$"
========================================================================================
**Application selected to mute: {selectedProcessName}!**

Make sure it maches the desired .exe that you want to mute.

- You can always hit escape to go back later if you'd like to change it.
========================================================================================";
                                TypeWithBlockTrail(message3, 10);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Press any key to launch");
                                SoundPlayer.SuccessSoundEffect();
                                Console.ResetColor();
                                Console.ReadKey();
                                SoundPlayer.NextSound();
                                gray = false;

                                if (string.IsNullOrWhiteSpace(selectedProcessName))
                                {
                                    Console.WriteLine("Invalid input. Please try again...");
                                    SoundPlayer.ButtonFail();
                                    break;
                                }

                                if (!selectedProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                {
                                    selectedProcessName += ".exe";
                                }
                                validKey = false;
                                SaveSettings();
                                break;
                            }

                            currentState = ProgramState.Running;
                            return; //Choose app
                        case ConsoleKey.D5:
                            validInput = true;
                            SoundPlayer.LaunchSelect();
                            {
                                var player = new SoundPlayer();
                                string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "targetting_system.wav");
                                player.PlaySound(soundPath, 0.4f);
                                gray = true;
                                validKey = false;
                                DisplayMenuGray(mainMenu);
                                string autoConfigWarning =
    $@"
========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

This automatic configuration will attempt to look for your Counter-Strike cfg
folder and either read or generate the appropriate gamestate_integration_media.cfg.

[!] Highly recommend running this as administrator, it might not work otherwise.
========================================================================================";
                                TypeWithBlockTrail(autoConfigWarning, 10);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Press any key to continue...");
                                Console.ResetColor();
                                Console.ReadKey();
                                SoundPlayer.NextSound();
                                gray = true;
                                validKey = false;
                                DisplayMenuGray(mainMenu);
                                string autoConfigWarning2 =
    $@"
========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

If it fails you can also set it up manually following the instructions in the readme.
It's really easy!

- Shoutout to FisheyePG for helping make this autoconfiguration wizard. Absolute Chad!
========================================================================================";
                                TypeWithBlockTrail(autoConfigWarning2, 10);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Press any key to continue...");
                                Console.ResetColor();
                                Console.ReadKey();
                                SoundPlayer.NextSound();

                                gray = true;
                                validKey = false;
                                DisplayMenuGray(mainMenu);
                                string? path = FindCSGOInstallPath();
                                if (path != null)
                                {
                                    csFound = "[✓] Counter-Strike cfg folder found.";

                                    string? port = SetupGameStateIntegration(path);
                                    if (port != null)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        csPortSet = "[✓] Ready to communicate on port: ";
                                        bool parsing = int.TryParse(port, out int portInt);
                                        webhookPort = portInt;
                                        SavePort();
                                        if (Process.GetProcessesByName("cs2").Any() || Process.GetProcessesByName("csgo").Any())
                                        {
                                            autoConfigCSRunning = "- It looks like Counter-Strike is running, please restart the game for changes to take effect.";
                                            cfgRecentlyUpdated = true;
                                        }
                                        else
                                        {
                                            autoConfigCSRunning = "- Just start Counter-Strike and select your media software and you're ready to go!";
                                        }
                                    }
                                    else
                                    {
                                        SoundPlayer.ButtonFail();
                                        csPortSet = "[X] failed to set port";
                                    }
                                }
                                else
                                {
                                    SoundPlayer.ButtonFail();
                                    csFound = "[X] Counter-Strike cfg folder could not be found.";
                                }
                                string autoConfigMessage =
    $@"
========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

{FoundSuccess}

{csPortSet}{webhookPort}

{autoConfigCSRunning}
========================================================================================";
                                TypeWithBlockTrail(autoConfigMessage, 10);
                                Console.ForegroundColor = ConsoleColor.Green;
                                if (csWasFoundByAutoConfiguration)
                                {
                                    Console.WriteLine("**Mullet Media-Strike 6.9 automatic port configuration** was successful!");
                                    SoundPlayer.SuccessSoundEffect();
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("**Mullet Media-Strike 6.9 automatic port configuration** failed!");
                                }
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Press any key to save and continue.");
                                Console.ResetColor();
                                gray = false;
                                autoConfigCSRunning = "";
                                csFound = "";
                                csPortSet = "";
                                Console.ReadKey();
                                SoundPlayer.NextSound();
                                ResetWebhookToDefault();
                                SaveSettings();
                                break;
                            } //auto config
                        case ConsoleKey.D6:
                            Console.CursorVisible = true;
                            validInput = true;
                            gray = true;
                            SoundPlayer.LaunchSelect();
                            DisplayMenuGray(mainMenu);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("\nEnter desired port number (1024–65535): ");
                            Console.ResetColor();
                            if (int.TryParse(Console.ReadLine(), out int newPort) && newPort >= 1024 && newPort <= 65535)
                            {
                                webhookPort = newPort;
                                SavePort();

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"[✓] Success! New port set to http://localhost:{newPort}.");
                                SoundPlayer.SuccessSoundEffect();
                                Console.WriteLine("Press any key to save and continue.");
                                Console.ReadKey();
                                RestartWebhook(newPort);
                                DisplayMenuGray(mainMenu);
                                SoundPlayer.NextSound();
                                Console.ResetColor();

                                bool wasGenerated;
                                if (CfgFoundOrNot("gamestate_integration_media.cfg", out wasGenerated))
                                {
                                    DisplayMenuGray(mainMenu);
                                    if (Process.GetProcessesByName("cs2").Any() || Process.GetProcessesByName("csgo").Any())
                                    {
                                        cfgRecentlyUpdated = true;
                                    }
                                    string truncatedPath = configFilePathPull.Length > 50 ? configFilePathPull.Substring(0, 50) + "..." : configFilePathPull;
                                    string msg =
                        @$"
========================================================================================
**Webhook Port Change**

You {(wasGenerated ? "just created" : "seem to have")} a gamestate_integration_media.cfg located in:
{truncatedPath}

- Would you like to save the new port to it as well so that they match? (recommended)
========================================================================================";
                                    TypeWithBlockTrail(msg, 1);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("[1] Yes [2] No");
                                    Console.CursorVisible = false;
                                    Console.ResetColor();


                                    bool validInput2 = false;
                                    while (!validInput2)
                                    {
                                        ConsoleKey key2 = Console.ReadKey(true).Key;
                                        switch (key2)
                                        {
                                            case ConsoleKey.D1:
                                            case ConsoleKey.NumPad1:
                                                DisplayMenuGray(mainMenu);
                                                SoundPlayer.NextSound();
                                                validInput2 = true;
                                                string newPortString = newPort.ToString();
                                                bool success = UpdateGamestateIntegrationURI(newPortString);

                                                if (success)
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine("gamestate_integration.cfg port updated successfully!");
                                                    if (Process.GetProcessesByName("cs2").Any() || Process.GetProcessesByName("csgo").Any())
                                                    {
                                                        cfgRecentlyUpdated = true;
                                                    }
                                                    SoundPlayer.SuccessSoundEffect();
                                                }
                                                else
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("Failed to update the port.");
                                                    DisplayMenuGray(mainMenu);
                                                    ShowUriWarningBlock();
                                                    SoundPlayer.ButtonFail();
                                                }
                                                Console.ResetColor();

                                                break;

                                            case ConsoleKey.D2:
                                            case ConsoleKey.NumPad2:
                                                SoundPlayer.NextSound();
                                                DisplayMenuGray(mainMenu);
                                                validInput2 = true;
                                                ShowUriWarningBlock();
                                                Console.ForegroundColor = ConsoleColor.Yellow;
                                                Console.WriteLine("Skipped updating URI.");
                                                break;

                                            default:
                                                SoundPlayer.ButtonFail();
                                                break;
                                        }
                                    }

                                }
                                else
                                {
                                    DisplayMenuGray(mainMenu);
                                    ShowUriWarningBlock();
                                }

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Press any key to continue...");
                                Console.CursorVisible = false;
                                Console.ResetColor();
                                validKey = false;
                                Console.ReadKey();
                                SoundPlayer.NextSound();
                            }
                            else
                            {
                                SoundPlayer.ButtonFail();
                                Console.WriteLine("Invalid port. Press any key to try again...");
                                Console.WriteLine("Press any key to continue...");
                                Console.CursorVisible = false;
                                validKey = false;
                                Console.ReadKey();
                                SoundPlayer.NextSound();
                            }
                            Console.ResetColor();
                            SaveSettings();
                            gray = false;
                            break; //change ports
                        case ConsoleKey.D7:
                            {
                                bool subInputValid = false;
                                while (!subInputValid)
                                {
                                    SoundPlayer.LaunchSelect();
                                    Console.CursorVisible = false;
                                    gray = true;
                                    validKey = false;
                                    DisplayMenuGray(mainMenu);
                                    string soundSettingSelection =
                        @$"
========================================================================================
**Sound Settings**

[1] Volume Settings: min {defaultMinVolume} | max {defaultMaxVolume} | fade {defaultFade}
[2] Media Keys: {mediaKeys}
[3] Sound Alerts: {soundAlerts}

[ESC] Exit
========================================================================================";
                                    TypeWithBlockTrail(soundSettingSelection, 1);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Please make your selection!");
                                    Console.ResetColor();

                                    var subKey1 = Console.ReadKey(true).Key;

                                    if (subKey1 == ConsoleKey.NoName || subKey1 == ConsoleKey.VolumeUp || subKey1 == ConsoleKey.VolumeDown || subKey1 == ConsoleKey.VolumeMute)
                                    {
                                        // Skip over volume scroll or invalid inputs
                                        continue;  // This is valid because we are inside a while loop
                                    }

                                    switch (subKey1)
                                    {
                                        case ConsoleKey.D1:
                                            subInputValid = true;
                                            SoundPlayer.LaunchSelect();
                                            {
                                                Console.CursorVisible = false;
                                                gray = true;
                                                validKey = false;
                                                DisplayMenuGray(mainMenu);
                                                string message2 =
                    @$"
========================================================================================
**Volume Settings**

Set your MINIMUM volume for freezetimes, timeouts and intermissions.
Current minimum volume: {defaultMinVolume}%

- I'd suggest setting it to 25% or 50% so you can hear calls at the start of rounds.
========================================================================================";
                                                TypeWithBlockTrail(message2, 10);
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.Write("Please set your NEW minimum volume (0-100): ");
                                                Console.CursorVisible = true;
                                                Console.ForegroundColor = ConsoleColor.White;
                                                string input = Console.ReadLine();
                                                Console.ResetColor();
                                                if (string.IsNullOrEmpty(input))
                                                {
                                                    SoundPlayer.ButtonFail();
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("[✘] Input cannot be empty. - loading defaults");
                                                    Console.ResetColor();
                                                    defaultMinVolume = 25;
                                                    defaultMaxVolume = 75;
                                                    defaultFade = 1000;
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"Press any key to go back to main menu...");
                                                    Console.CursorVisible = false;
                                                    Console.ResetColor();
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                    return;
                                                }
                                                else
                                                {
                                                    // Check if input contains '%'
                                                    if (input.Contains("%"))
                                                    {
                                                        // Remove the '%' character and try to parse the remaining number
                                                        input = input.Replace("%", "").Trim();
                                                    }
                                                }

                                                if (int.TryParse(input, out defaultMinVolume) && defaultMinVolume >= 0 && defaultMinVolume <= 100) //volume
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"[✓] New minimum volume: {defaultMinVolume}%.");
                                                    Console.WriteLine("Press any key to save and continue.");
                                                    Console.CursorVisible = false;
                                                    SoundPlayer.SuccessSoundEffect();
                                                    Console.ResetColor();
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                }
                                                else
                                                {
                                                    SoundPlayer.ButtonFail();
                                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                                    Console.WriteLine("[✘] Invalid input, choose a value between 0 and 100 - loading defaults.");
                                                    defaultMinVolume = 25;
                                                    defaultMaxVolume = 75;
                                                    defaultFade = 1000;
                                                    Console.ResetColor();
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"Press any key to go back to main menu...");
                                                    Console.CursorVisible = false;
                                                    Console.ResetColor();
                                                    validKey = false;
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                    return;
                                                }

                                                gray = true;
                                                validKey = false;
                                                DisplayMenuGray(mainMenu);
                                                string message5 =
                    @$"
========================================================================================
**Volume Settings**

Set your MAXIMUM volume for when you're DEAD in game.
Current maximum volume: {defaultMaxVolume}%

- I'd suggest setting it to 75% or 100%.
========================================================================================";
                                                TypeWithBlockTrail(message5, 10);
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.Write("Please set your NEW maximum volume (0-100): ");
                                                Console.ForegroundColor = ConsoleColor.White;
                                                Console.ResetColor();
                                                string input2 = Console.ReadLine();
                                                if (string.IsNullOrEmpty(input2))
                                                {
                                                    SoundPlayer.ButtonFail();
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("[✘] Input cannot be empty - loading defaults.");
                                                    Console.ResetColor();
                                                    defaultFade = 1000;
                                                    defaultMaxVolume = 75;
                                                    defaultMinVolume = 25;
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"Press any key to go back to main menu...");
                                                    Console.ResetColor();
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                    return;
                                                }
                                                else
                                                {
                                                    // Check if input contains '%'
                                                    if (input2.Contains("%"))
                                                    {
                                                        // Remove the '%' character and try to parse the remaining number
                                                        input2 = input2.Replace("%", "").Trim();
                                                    }
                                                }

                                                if (int.TryParse(input2, out defaultMaxVolume) && defaultMaxVolume >= 0 && defaultMaxVolume <= 100) //volume
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"[✓] New maximum volume: {defaultMaxVolume}%.");
                                                    SoundPlayer.SuccessSoundEffect();
                                                    Console.WriteLine("Press any key to save and continue.");
                                                    Console.ResetColor();
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                }
                                                else
                                                {
                                                    SoundPlayer.ButtonFail();
                                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                                    Console.WriteLine("[✘] Invalid input, choose a value between 0 and 100 - loading defaults.");
                                                    defaultMaxVolume = 75;
                                                    defaultMinVolume = 25;
                                                    defaultFade = 1000;
                                                    Console.ResetColor();
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"Press any key to go back to main menu...");
                                                    Console.ResetColor();
                                                    validKey = false;
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                    return;
                                                }

                                                gray = true;
                                                validKey = false;
                                                DisplayMenuGray(mainMenu);
                                                string message6 =
                    @$"
========================================================================================
**Volume Settings**

Set your fade level between mutes and unmutes for smooth sound transitions.
Current fade: {defaultFade}ms

- I'd suggest setting it between 500ms (0,5 seconds) and 1500ms (1,5 seconds)
========================================================================================";
                                                TypeWithBlockTrail(message6, 10);
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.Write("Please set your NEW fade volume (100-5000): ");
                                                Console.ForegroundColor = ConsoleColor.White;
                                                Console.ResetColor();
                                                string input3 = Console.ReadLine();
                                                if (string.IsNullOrEmpty(input3))
                                                {
                                                    SoundPlayer.ButtonFail();
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("[✘] Input cannot be empty - loading defaults.");
                                                    Console.ResetColor();
                                                    defaultMaxVolume = 75;
                                                    defaultMinVolume = 25;
                                                    defaultFade = 1000;
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"Press any key to go back to main menu...");
                                                    Console.ResetColor();
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                    return;
                                                }

                                                if (int.TryParse(input3, out defaultFade) && defaultFade >= 100 && defaultFade <= 5000) //volume
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"[✓] New fade volume: {defaultFade}.");
                                                    SoundPlayer.SuccessSoundEffect();
                                                    Console.WriteLine("Press any key to save and continue go back to main menu.");
                                                    Console.ResetColor();
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                }
                                                else
                                                {
                                                    SoundPlayer.ButtonFail();
                                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                                    Console.WriteLine("[✘] Invalid input, choose a value between 100 and 5000 - loading defaults.");
                                                    defaultMaxVolume = 75;
                                                    defaultMinVolume = 25;
                                                    defaultFade = 1000;
                                                    Console.ResetColor();
                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine($"Press any key to go back to main menu...");
                                                    Console.ResetColor();
                                                    validKey = false;
                                                    Console.ReadKey();
                                                    SoundPlayer.NextSound();
                                                    gray = false;
                                                    return;
                                                }
                                                Console.ResetColor();
                                                gray = false;
                                            }
                                            SaveSettings();
                                            return; //min & max volume

                                        case ConsoleKey.D2:
                                            subInputValid = true;
                                            validKey = false;
                                            gray = true;
                                            SoundPlayer.LaunchSelect();
                                            DisplayMenuGray(mainMenu); // draw menu once
                                            HandleMediaKeys();         // display extra info below menu
                                            gray = false;
                                            return; //Media Keys

                                        case ConsoleKey.D3:
                                            subInputValid = true;
                                            bool subInputValid3 = false;
                                            while (!subInputValid3)
                                            {
                                                SoundPlayer.LaunchSelect();
                                                Console.CursorVisible = false;
                                                gray = true;
                                                validKey = false;
                                                DisplayMenuGray(mainMenu);
                                                string soundSettingChoices =
                                    @$"
========================================================================================
**Sound Alert Settings**
This will enable or disable sound alerts when you get kill streaks, double kill, multi 
kill etc. Current setting: {soundAlerts}

[1] Enable (all game modes)
[2] Enable (only in deathmatch)
[3] Disable (all game modes)

[ESC] Exit
========================================================================================";
                                                TypeWithBlockTrail(soundSettingChoices, 1);
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("Please make your selection!");
                                                Console.ResetColor();

                                                var subKey5 = Console.ReadKey(true).Key;

                                                if (subKey5 == ConsoleKey.NoName || subKey5 == ConsoleKey.VolumeUp || subKey5 == ConsoleKey.VolumeDown || subKey5 == ConsoleKey.VolumeMute)
                                                {
                                                    // Skip over volume scroll or invalid inputs
                                                    continue;  // This is valid because we are inside a while loop
                                                }

                                                switch (subKey5)
                                                {
                                                    case ConsoleKey.D1:
                                                        SoundPlayer.SuccessSoundEffect();
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        soundAlerts = "enabled";
                                                        Console.WriteLine("Sound notifications enabled!");
                                                        Console.WriteLine("Press any key to return to main menu.");
                                                        Console.ReadKey();
                                                        Console.ResetColor();
                                                        SaveSettings();
                                                        return;
                                                    case ConsoleKey.D2:
                                                        SoundPlayer.SuccessSoundEffect();
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        soundAlerts = "enabled(DM)";
                                                        Console.WriteLine("Sound notifications enabled only for deathmatch!");
                                                        Console.WriteLine("Press any key to return to main menu.");
                                                        Console.ReadKey();
                                                        Console.ResetColor();
                                                        SaveSettings();
                                                        return;
                                                    case ConsoleKey.D3:
                                                        SoundPlayer.SuccessSoundEffect();
                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        Console.WriteLine("Sound notifications disabled!");
                                                        Console.WriteLine("Press any key to return to main menu.");
                                                        soundAlerts = "disabled";
                                                        Console.ReadKey();
                                                        Console.ResetColor();
                                                        SaveSettings();
                                                        return;


                                                    case ConsoleKey.Escape:
                                                    mainScreen = false;
                                                    subInputValid3 = true;
                                                    SoundPlayer.EscapeButton();
                                                    currentState = ProgramState.Menu;
                                                    SaveSettings();
                                                    return;  // exits entire method
                                                default:
                                                    SoundPlayer.ButtonFail();
                                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                                    Console.WriteLine("[✘] Invalid input.");
                                                    Console.ReadKey();
                                                    break;
                                                }
                                            } // end while subInputValid
                                            break;

                                        case ConsoleKey.Escape:
                                            mainScreen = false;
                                            subInputValid = true;
                                            SoundPlayer.EscapeButton();
                                            currentState = ProgramState.Menu;
                                            SaveSettings();
                                            return;  // exits entire method
                                        default:
                                            SoundPlayer.ButtonFail();
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("[✘] Invalid input.");
                                            Console.ReadKey();
                                            break;
                                    }
                                } // end while subInputValid
                                break;
                            }
                        case ConsoleKey.D8:
                            {
                                bool subInputValid2 = false;
                                while (!subInputValid2)
                                {
                                    SoundPlayer.LaunchSelect();
                                    Console.CursorVisible = false;
                                    gray = true;
                                    validKey = false;
                                    DisplayMenuGray(mainMenu);

                                    string helpMenu =
                            @$"
========================================================================================
**Help & Restore Defaults**  

[1] ReadMe & Instructions
[2] Restore Defaults

[ESC] Exit
========================================================================================";
                                    TypeWithBlockTrail(helpMenu, 1);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Please make your selection!");
                                    Console.ResetColor();

                                    var subKey2 = Console.ReadKey(true).Key;

                                    // skip volume up/down/mute scroll events
                                    if (subKey2 == ConsoleKey.NoName
                                     || subKey2 == ConsoleKey.VolumeUp
                                     || subKey2 == ConsoleKey.VolumeDown
                                     || subKey2 == ConsoleKey.VolumeMute)
                                    {
                                        continue;
                                    }

                                    switch (subKey2)
                                    {
                                        case ConsoleKey.D1:
                                        case ConsoleKey.NumPad1:
                                            subInputValid2 = true;
                                            validInput = true;
                                            SoundPlayer.LaunchSelect();
                                            HandleReadMe();
                                            SaveSettings();
                                            break;

                                        case ConsoleKey.D2:
                                        case ConsoleKey.NumPad2:
                                            subInputValid2 = true;
                                            validInput = true;
                                            SoundPlayer.LaunchSelect();
                                            DisplayMenuGray(mainMenu);

                                            string restoreDefaults =
                            @$"
========================================================================================
**Restore Defaults**

This will restore your user settings back to default values.

- Your program will require a restart after completion. Would you like to continue?
========================================================================================";
                                            TypeWithBlockTrail(restoreDefaults, 1);

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("[1] Yes   [2] No");
                                            Console.ResetColor();

                                            var choice = Console.ReadKey(true).Key;
                                            Console.WriteLine();

                                            if (choice == ConsoleKey.D1 || choice == ConsoleKey.NumPad1)
                                            {
                                                try
                                                {
                                                    var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                                                    var filesToClear = new[]
                                                    {
                            Path.Combine(exeDirectory, "settings.txt"),
                            Path.Combine(exeDirectory, "webhook_log.txt"),
                            Path.Combine(exeDirectory, "webhook_port.txt")
                        };

                                                    foreach (var file in filesToClear)
                                                        File.WriteAllText(file, string.Empty);

                                                    Console.ForegroundColor = ConsoleColor.Green;
                                                    Console.WriteLine("Reverted back to defaults!");
                                                    Console.WriteLine("Please restart the program!");
                                                    resetRequested = true;
                                                    Console.ResetColor();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("An error occurred while resetting files:");
                                                    Console.WriteLine(ex.Message);
                                                    resetRequested = true;
                                                    Console.ResetColor();
                                                }

                                                SoundPlayer.SuccessSoundEffect();
                                                Environment.Exit(0);
                                                Console.ReadLine(); // never reached but safe

                                            }
                                            else if (choice == ConsoleKey.D2 || choice == ConsoleKey.NumPad2)
                                            {
                                                SoundPlayer.LaunchSelect();
                                                gray = true;
                                                validKey = false;
                                                // subInputValid2 already true, will exit to main menu
                                            }
                                            break;

                                        case ConsoleKey.Escape:
                                            mainScreen = false;
                                            subInputValid2 = true;
                                            SoundPlayer.EscapeButton();
                                            currentState = ProgramState.Menu;
                                            SaveSettings();
                                            return;  // exits entire method, back to caller

                                        default:
                                            SoundPlayer.ButtonFail();
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("[✘] Invalid input.");
                                            Console.ReadKey();
                                            continue;

                                    }
                                } // end while (!subInputValid2)

                                break;
                            } // end case D8

                        }
                     }

                 void ShowUriWarningBlock()
                {
                    DisplayMenuGray(mainMenu);
                    string portSaved =
                @$"
========================================================================================
Make sure your port matches the uri in your gamestate_integration.cfg

Where? [...]SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg\

- Note that every time you change the .cfg Counter-Strike requires a restart.
========================================================================================";
                    TypeWithBlockTrail(portSaved, 1);
                }

            }
        }
        static void HandleReadMe()
        {
            Console.Clear();
            string guideText =
@"  ========================================================================================
                 Mullet Media-Strike 6.9 - Game State Integration
  ========================================================================================

  Follow these quick steps to get everything working smoothly. (Manual Setup)

  ----------------------------------------------------------------------------------------
  1. PLACE THE CONFIG FILE
  ----------------------------------------------------------------------------------------
     - Turn off Counter-Strike if its running.

     - Copy the provided **game_stateintegration_media.cfg** file
       (Its in the same folder as this readme)

     - Paste it into the following folder:
       [...]\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg\

  ----------------------------------------------------------------------------------------
  2. YOU ARE DONE!
  ----------------------------------------------------------------------------------------
     - Start Counter-Strike again.

     - Start or restart Mullet Media-Strike 6.9.
  ----------------------------------------------------------------------------------------";


            string guideText2 =
@"  ----------------------------------------------------------------------------------------
  3. (OPTIONAL) REPLACE ANY EXISTING CONFIG - FOOLPROOFING!
  ----------------------------------------------------------------------------------------
      - If a file named **gamestate_integration.cfg** already exists in that folder:
       > Delete it.
       > Replace it with **game_stateintegration_media.cfg**.

  ----------------------------------------------------------------------------------------
  4. (OPTIONAL) CONFIGURE LOCALHOST - skip if you want to use defaults.
  ----------------------------------------------------------------------------------------
     - Skip this if you just want to use the default port: **1337**

     - If you want to use a different port:
       > Open **game_stateintegration.cfg**
       > Make sure the ""uri"" line uses the correct port your software listens to.
         Example:
         
       ""uri"" ""http://localhost:1337/""

       > Set whatever you port you chose in Mullet Media-Strike 6.9 to the same number.
         You can change it in menu option [6] ""Change Webhook Port""

  ----------------------------------------------------------------------------------------
  IMPORTANT NOTES
  ----------------------------------------------------------------------------------------
   - You may need to run the software as administrator on some systems.

   - This setup is **VAC-safe**. It uses the official GSI webhook that Valve
     provides for tournament overlays.

   - That said, I take **zero responsibility** if you get banned (even though that 
     would make absolutely no sense).

   - If something isn’t working, double-check:
       > File paths
       > Port numbers, they NEED to match for it to work.
         Example: 
         The gamestate_integration_media.cfg file has ""uri"" set to ""http://localhost:1337/""
         this means that Mullet Media-Strike 6.9 needs to listen for port 1337 too.

    - Every time you make changes to the .cfg file Counter-Strike requires a restart.

    - I am currently considering looking into Spotify's API.

    - If someone wants to help out with this project, let me know!

  ========================================================================================
                 Click some heads and enjoy your jams. //80smullet
  ========================================================================================
  ";
            TypeWithBlockTrail(guideText, 10);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n[!] It should work as intended with these steps, anything beyond this is entirely optional.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nPress any key to continue reading...");
            Console.ResetColor();
            Console.ReadKey();
            SoundPlayer.NextSound();
            TypeWithBlockTrail(guideText2, 10);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press any key to go back to main menu.");
            Console.ResetColor();
            Console.ReadKey();
            SoundPlayer.NextSound();
            Console.Clear();
        }

        static void HandleMediaKeys()
        {
            string message2;

            if (!mediaKeysEnabled)
            {
                message2 =
        @$"
========================================================================================
**Media Keys: {mediaKeys}!** (experimental)

Enabling this means that your keyboard will attempt to pause your media software using
media keyboard keys (play and pause).
========================================================================================";
                TypeWithBlockTrail(message2, 10);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Media keys are now ENABLED!");
                mediaKeysEnabled = true;
            }
            else
            {
                message2 =
        @$"
========================================================================================
**Media Keys: {mediaKeys}!** (experimental)

Disabling this means that your keyboard will NOT attempt to pause your media software
using media keyboard keys.
========================================================================================";
                TypeWithBlockTrail(message2, 10);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Media keys are now DISABLED.");
                mediaKeysEnabled = false;
            }

            SaveSettings();
            SoundPlayer.SuccessSoundEffect();
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
            SoundPlayer.NextSound();
        }

        static void TypeWithBlockTrail(string text, int delay = 10) //the cool modified text reveal method
        {
            string[] lines = text.Split('\n');

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                // Handle divider lines (= and -)
                if (trimmedLine.StartsWith("===") || trimmedLine.StartsWith("---"))
                {
                    // Show instantly in white
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(line);

                    // After 100ms, change to dark gray
                    Thread.Sleep(150);
                    if (Console.CursorTop > 0)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(line);
                    }
                    continue;
                }

                // Set initial colors based on content
                if (trimmedLine.Contains("Mullet Media-Strike 6.9 - Game State Integration") ||
                    trimmedLine.Contains("Click some heads and enjoy your jams. //80smullet"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (trimmedLine.StartsWith("1.") || trimmedLine.StartsWith("2.") || trimmedLine.StartsWith("3.") || trimmedLine.StartsWith("4.") ||
                         trimmedLine.Contains("PLACE THE CONFIG FILE") ||
                         trimmedLine.Contains("YOU ARE DONE!") ||
                         trimmedLine.Contains("REPLACE ANY EXISTING CONFIG") ||
                         trimmedLine.Contains("CONFIGURE LOCALHOST") ||
                         trimmedLine.Contains("IMPORTANT NOTES"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                // Type line with blinking trail
                for (int i = 0; i < line.Length; i++)
                {
                    char currentChar = line[i];

                    // Blink only if not at end of line
                    bool nextIsNewlineOrEnd = (i + 1 >= line.Length) || (line[i + 1] == '\n');

                    if (!nextIsNewlineOrEnd)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("█");
                        Thread.Sleep(delay);

                        if (Console.CursorLeft > 0)
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }


                    if (trimmedLine.Contains("Mullet Media-Strike 6.9 - Game State Integration") ||
                        trimmedLine.Contains("Click some heads and enjoy your jams. //80smullet"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else if (trimmedLine.StartsWith("1.") || trimmedLine.StartsWith("2.") || trimmedLine.StartsWith("3.") || trimmedLine.StartsWith("4.") ||
                             trimmedLine.Contains("PLACE THE CONFIG FILE") ||
                             trimmedLine.Contains("YOU ARE DONE!") ||
                             trimmedLine.Contains("REPLACE ANY EXISTING CONFIG") ||
                             trimmedLine.Contains("CONFIGURE LOCALHOST") ||
                             trimmedLine.Contains("IMPORTANT NOTES"))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    if (trimmedLine == ("[ twitch.tv/80smullet | twitter.com/80smullet | youtube.com/@Cenote | discord.gg/WMcAHPNFef ]"))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }

                    Console.Write(currentChar);
                }

                Console.WriteLine(); 
            }

            Console.ResetColor(); 
        }

        static void SaveSettings()
        {
            AudioControl.GetAppVolume($"{selectedProcessName}");


            try
            {
                using (StreamWriter writer = new StreamWriter(settingsFilePath, false))  // Overwrites the save file each time
                {
                    writer.WriteLine($"defaultMinVolume={defaultMinVolume}");
                    writer.WriteLine($"defaultMaxVolume={defaultMaxVolume}");
                    writer.WriteLine($"defaultFade={defaultFade}");
                    writer.WriteLine($"mediaKeysEnabled={mediaKeysEnabled}");
                    writer.WriteLine($"keystrokesCounter={keystrokesCounter}");
                    writer.WriteLine($"soundAlerts={soundAlerts}");
                }
                //Console.WriteLine("[+] Settings saved successfully.");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[X] Error saving settings: {ex.Message}");
            }
        }
        static void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {

                var lines = File.ReadAllLines(settingsFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        switch (parts[0])
                        {
                            case "defaultMinVolume":
                                defaultMinVolume = int.Parse(parts[1]);
                                break;
                            case "defaultMaxVolume":
                                defaultMaxVolume = int.Parse(parts[1]);
                                break;
                            case "defaultFade":
                                defaultFade = int.Parse(parts[1]);
                                break;
                            case "mediaKeysEnabled":
                                mediaKeysEnabled = bool.Parse(parts[1]);
                                break;
                            case "keystrokesCounter":
                                keystrokesCounter = int.Parse(parts[1]);
                                break;
                            case "soundAlerts":
                                soundAlerts = parts[1];
                                break;
                        }
                    }
                }


                //catch (Exception ex) <---dont care, let the thing die if it has to
                {
                    //Console.WriteLine($"[X] Error loading settings: {ex.Message}");
                }
            }
            //else
            {
                //Console.WriteLine("[X] Settings file not found. Using default values.");
            }
        }

        static void ClearKeyBuffer()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true); // clearing keys, prevent stacked inputs
            }
        }

        static void StopWebhook()
        {
            if (listener != null && listener.IsListening)
            {
                listener.Stop();
                listener.Close();
                listener = null;
            }
        }

        static void RestartWebhook(int newPort)
        {
            StopWebhook();
            webhookPort = newPort;
            StartWebhook();
        }
        const int DefaultWebhookPort = 1337;

        static void ResetWebhookToDefault()
        {
            // Persist the default port
            webhookPort = DefaultWebhookPort;
            SavePort();                // so it’s stored for next run

            // Tear down any existing listener and bring up a new one
            RestartWebhook(webhookPort);

            Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine($"[!] Webhook port reset to default http://localhost:{webhookPort}/");
            Console.ResetColor();
        }

        static void StartWebhook() //Webhook listener
        {
            if (listener != null && listener.IsListening)
                return;

            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{webhookPort}/");

            try
            {
                listener.Start();
            }
            catch (HttpListenerException ex)
            {
                //SoundPlayer.ButtonFail();
                //Console.WriteLine($"[X] Failed to start listener on port {webhookPort}: {ex.Message}");
                return;
            }

            Task.Run(() =>
            {
                while (listener.IsListening)
                {
                    try
                    {
                        var context = listener.GetContext();
                        var request = context.Request;
                        if (request.HttpMethod == "POST")
                        {
                            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                            {
                                var body = reader.ReadToEnd();
                                HandleIncomingGameState(body);
                                try
                                {
                                    Interlocked.Increment(ref sessionWebhookCount);
                                    string path = "webhook_log.txt";
                                    File.WriteAllText(path, $"[{DateTime.Now}] {body}{Environment.NewLine}");
                                    lastWebhookTime = DateTime.Now;
                                }
                                catch (Exception fileEx)
                                {
                                    SoundPlayer.ButtonFail();
                                    //Console.WriteLine($"[X] Failed to write to file: {fileEx.Message}");
                                }
                            }

                            context.Response.StatusCode = 200;
                            using (var writer = new StreamWriter(context.Response.OutputStream))
                            {
                                writer.Write("OK");
                                webhookConnectStatus = true;
                            }

                            //webhooks received
                            Interlocked.Increment(ref hiddenCounter);
                        }
                        else
                        {
                            context.Response.StatusCode = 405;
                            context.Response.Close();
                            webhookConnectStatus = false;
                        }
                    }
                    catch (HttpListenerException ex) when (!listener.IsListening)
                    {
                        // intentional stop of listener
                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
        }

        static void HandleIncomingGameState(string json)
        {
            shouldMute = false;
            try
            {
                var jsonObj = JObject.Parse(json);
                string providerSteamid = jsonObj["provider"]?["steamid"]?.ToString() ?? "";
                string playerSteamid = jsonObj["player"]?["steamid"]?.ToString() ?? "";
                string activity = jsonObj["player"]?["activity"]?.ToString() ?? "";
                string phase = jsonObj["round"]?["phase"]?.ToString() ?? "";
                string gameMode = jsonObj["map"]?["mode"]?.ToString() ?? "";
                double phaseEndsIn = jsonObj["map"]?["phase_ends_in"]?.Value<double>() ?? 0;
                int playerHealth = jsonObj["state"]?["health"]?.Value<int>() ?? 0;
                string mapPhase = jsonObj["map"]?["phase"]?.ToString() ?? "";

                string currentRound = jsonObj["map"]?["round"]?.ToString() ?? "";

                int roundKills = jsonObj["player"]?["state"]?["round_kills"]?.Value<int>() ?? 0;

                var matchStats = jsonObj["player"]?["match_stats"];
                if (matchStats == null)

                    // Status tracking
                phaseStatus = phase;
                activityStatus = activity;
                providerSteamidStatus = providerSteamid;
                playerSteamidStatus = playerSteamid;
                gameModeStatus = gameMode;
                string soundAlertsNext = "";
                if (soundAlerts == "disabled")
                {
                    soundAlertsNext = "disabled";
                }
                else if (soundAlerts == "enabled")
                {
                    soundAlertsNext = "enabled";
                }
                else if (soundAlerts == "enabled(DM)" && gameMode == "deathmatch")
                {
                    soundAlertsNext = "enabled";
                }
                else if (soundAlerts == "enabled(DM)" && gameMode != "deathmatch")
                {
                    soundAlertsNext = "disabled";
                }


                bool isFreezeTime = (phase == "freezetime");
                bool playerDied = (providerSteamid == playerSteamid) && (newDeaths > 0);

                if (isFreezeTime || playerDied)
                {
                    roundKills = 0;
                    killStreakText = "";
                    playedDoubleKill = false;
                    playedMultiKill = false;
                    playedUltraKill = false;
                    playedMonsterKill = false;
                }

                if (providerSteamid == playerSteamid && !isFreezeTime)
                {
                    if (roundKills == 0)
                    {
                        killStreakText = "";
                    }
                    if (roundKills >= 2 && !playedDoubleKill)
                    {
                        if (soundAlertsNext == "enabled")
                        {
                            Task.Run(() => SoundPlayer.DoubleKill());
                        }
                        killStreakText = "★★ Double Kill! ★★";
                        playedDoubleKill = true;
                    }
                    if (roundKills >= 3 && !playedMultiKill)
                    {
                        if (soundAlertsNext == "enabled")
                        {
                            Task.Run(() => SoundPlayer.MultiKill());
                        }
                        killStreakText = "★★★ Multi Kill! ★★★";
                        playedMultiKill = true;
                    }
                    if (roundKills >= 4 && !playedUltraKill)
                    {
                        if (soundAlertsNext == "enabled")
                        {
                            Task.Run(() => SoundPlayer.UltraKill());
                        }
                        killStreakText = "★★★★ ULTRA KILL!! ★★★★";
                        playedUltraKill = true;
                    }
                    if (roundKills >= 5 && !playedMonsterKill)
                    {
                        if (soundAlertsNext == "enabled")
                        {
                        Task.Run(() => SoundPlayer.MonsterKill());
                        }
                        killStreakText = "★★★★★ M O N S T E R K I L L !!! ★★★★★";
                        playedMonsterKill = true;

                        playedDoubleKill = true;
                        playedMultiKill = true;
                        playedUltraKill = true;
                    }
                }

                if (providerSteamid == playerSteamid && activity == "playing" && phase == "live")
                {
                    int kills = matchStats?["kills"]?.Value<int>() ?? 0;
                    int assists = matchStats?["assists"]?.Value<int>() ?? 0;
                    int deaths = matchStats?["deaths"]?.Value<int>() ?? 0;

                    int newKills = Math.Max(0, kills - lastKills);
                    int newAssists = Math.Max(0, assists - lastAssists);
                    newDeaths = Math.Max(0, deaths - lastDeaths);

                    sessionKills += newKills;
                    sessionAssists += newAssists;
                    sessionDeaths += newDeaths;

                    lastKills = kills;
                    lastAssists = assists;
                    lastDeaths = deaths;

                    double kdaRatio = sessionDeaths == 0
                        ? sessionKills + sessionAssists
                        : (double)(sessionKills + sessionAssists) / sessionDeaths;

                    killDeathAssistRatio = $"Session KDA: {kdaRatio:F2}";
                }

                if (phase == "live")
                {
                    freezetimeactive = false;
                }
                
                if
                (
                providerSteamid == playerSteamid
                && gameMode != "deathmatch"
                //&& phase == "live"
                //&& activity == "playing"  <-shitty way to determine if you're alive, just compare steamid's
                //&& playerHealth <= 0
                //&& mapPhase == "live" 
                //&& playerAliveThisRound == true
                )

                {
                    shouldMute = true;
                }
                else
                {
                    shouldMute = false;
                }

                if (mainScreen && activity == "menu" || activity == "waiting...")
                {
                    targetVolume = defaultMaxVolume;
                    TargetAppVolume();
                    playerStatus = "inactive / dead";
                    statusText = "unmuted";
                    menuOverride = true;
                }
                else
                {
                    menuOverride = false;
                }

                if //alive
                (shouldMute == true && mainScreen && menuOverride == false)
                {
                    if (providerSteamid == playerSteamid) //alive check (alive)
                    {
                        if (phase == "live" || phase == "over")
                        {
                            targetVolume = 0;
                        }
                        else
                        {
                            targetVolume = defaultMinVolume;
                        }
                        TargetAppVolume();
                        //isTargetMuted = true;
                        playerStatus = "active / alive";
                        statusText = "muted";
                    }
                }
                else if
                (shouldMute != true && mainScreen && menuOverride == false)
                {
                    if (providerSteamid != playerSteamid) //alive check (dead)
                    {
                        if (phase == "live")
                        {
                            targetVolume = defaultMaxVolume;

                        }
                        else if (phase == "over")
                        {
                            targetVolume = defaultMinVolume;
                        }
                    }
                    TargetAppVolume();
                    //isTargetMuted = false;
                    playerStatus = "inactive / dead";
                    statusText = "unmuted";
                }
                if (targetVolume != lastTargetVolume)
                {
                    IncrementCounter();
                    lastTargetVolume = targetVolume;
                }

            }


            catch { }

        }
        public static bool IsCFGFileOpenInEditor()
        {
            string targetFileName = "gamestate_integration_media.cfg";


            string[] editors = { "notepad", "notepad++", "code", "sublime_text" };

            foreach (var editor in editors)
            {
                var processes = Process.GetProcessesByName(editor);
                foreach (var proc in processes)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle) &&
                            proc.MainWindowTitle.Contains(targetFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            cfgIsOpen = true;
                            return true;
                        }
                    }
                    catch { /* Ignore access-denied processes */ }
                }
            }

            return false;
        }


        static bool isPlaying = false;

        static void TargetAppVolume(/*…*/)
        {
            if (exiting == false)
            {
                // decide “should be playing” based on targetVolume:
                bool shouldBePlaying = targetVolume > 0;

                    // paused to playing
                    if (shouldBePlaying && !isPlaying && mediaKeysEnabled == true)
                {
                    if (selectedProcessName == "spotify.exe" || 
                    selectedProcessName == "vlc.exe" || 
                    selectedProcessName == "chrome.exe")
                    {
                    MediaControl.SendPlayTargettedPauseKey(selectedProcessName);
                    }
                    else
                    {
                    MediaControl.Play();
                    }
                isPlaying = true;
                }

                // volume fading
                AudioControl.FadeToVolume(selectedProcessName, targetVolume, defaultFade, 10);
                Thread.Sleep(defaultFade);

                // transition
                if (!shouldBePlaying && isPlaying && mediaKeysEnabled == true)
                {
                    if (selectedProcessName == "spotify.exe" ||
                        selectedProcessName == "vlc.exe" ||
                        selectedProcessName == "chrome.exe")

                    {
                        MediaControl.SendPlayTargettedPauseKey(selectedProcessName);
                    }
                    else
                    {
                        MediaControl.Pause();
                    }
                    isPlaying = false;
                }
            }
        }

        static void IncrementCounter()
        {
            keystrokesCounter++;
            SaveCounter();
        }

        static void SaveCounter()
        {
            try { File.WriteAllText(counterFilePath, keystrokesCounter.ToString()); }
            catch (Exception ex)
            {
                SoundPlayer.ButtonFail();
                Console.WriteLine($"[X] Error saving counter: {ex.Message}");
            }
        }

        static void LoadCounter()
        {
            if (File.Exists(counterFilePath))
            {
                try { keystrokesCounter = int.Parse(File.ReadAllText(counterFilePath)); }
                catch { keystrokesCounter = 0; }
            }
        }

        static void SavePort() //saving ports mtheods
        {
            try { File.WriteAllText(portFilePath, webhookPort.ToString()); }
            catch (Exception ex)
            {
                SoundPlayer.ButtonFail();
                Console.WriteLine($"[X] Error saving port: {ex.Message}");
            }
        }

        static void LoadPort() //Loading ports methods
        {
            if (File.Exists(portFilePath))
            {
                try
                {
                    int savedPort = int.Parse(File.ReadAllText(portFilePath));
                    if (savedPort >= 1024 && savedPort <= 65535)
                        webhookPort = savedPort;
                }
                catch { webhookPort = 1337; }
            }
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"=== Mullet Media-Strike 6.9 ===\n");
            Console.ResetColor();
        }

        static int blinkColorIndex = 0;
        static readonly ConsoleColor[] blinkColors = new[] { ConsoleColor.Yellow, ConsoleColor.Gray };

        static void UpdateDisplayLoop() // activity feed
        {
            bool firstRun = true;
            bool shownConnected = false;
            bool shownCsNotRunning = false;
            bool shownWebhookMissing = false;
            bool shownBothMissing = false;
            string currentStatusMessage2 = "";
            string counterstrikemessage = null;
            bool shouldPlaySound = false;
            DateTime lastWebhookCheck = DateTime.Now;
            int lastWidth = Console.WindowWidth;
            int lastHeight = Console.WindowHeight;
            DateTime lastConnectionTime = DateTime.MinValue;

            while (!exitRequested)
            {
                // 1) Detect resize and clear
                if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
                {
                    Console.Clear();
                    lastWidth = Console.WindowWidth;
                    lastHeight = Console.WindowHeight;
                }

                // 2) current width minus 1 for padding
               var w = Console.WindowWidth - 1;

                Console.CursorVisible = false;
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        SoundPlayer.EscapeButton();
                        //RestartWebhookRegular();
                        //EnsureWebhookIsRunning();
                        currentState = ProgramState.Menu;
                        return; 
                    }
                }

                bool csIsRunning = Process.GetProcessesByName("cs2").Any() || Process.GetProcessesByName("csgo").Any();
                bool webhookRecentlyActive = (DateTime.Now - lastWebhookTime).TotalSeconds < 5;

                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"=== Mullet Media-Strike 6.9 ===".PadRight(w));
                Console.ResetColor();
                Console.WriteLine($"".PadRight(w));
                Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.SetCursorPosition(0, 2);
                Console.WriteLine($"Webhook started on http://localhost:{webhookPort}/".PadRight(w));

                Console.SetCursorPosition(0, 3);
                Console.WriteLine($"Webhooks received: {sessionWebhookCount}".PadRight(w));

                Console.SetCursorPosition(0, 5);
                Console.WriteLine($"Counter-Strike: {counterstrikemessage}".PadRight(w));

                Console.SetCursorPosition(0, 6);
                Console.WriteLine($"{killDeathAssistRatio}".PadRight(w));

                Console.SetCursorPosition(0, 7);
                Console.WriteLine($"Activity Status: {activityStatus}".PadRight(w));

                Console.SetCursorPosition(0, 8);
                Console.WriteLine($"Round Phase Status: {phaseStatus} {freezetimeStatus}".PadRight(w));

                Console.SetCursorPosition(0, 9);
                Console.WriteLine($"Game Mode Status: {gameModeStatus}".PadRight(w));

                Console.SetCursorPosition(0, 10);
                Console.WriteLine($"Player Status: {playerStatus}".PadRight(w));

                Console.SetCursorPosition(0, 12);
                currentVolume2 = AudioControl.GetAppVolume($"{selectedProcessName}");
                Console.WriteLine($"{selectedProcessName} - {statusText}".PadRight(w));

                Console.SetCursorPosition(0, 13);
                Console.WriteLine($"Volume: min {defaultMinVolume} | max {defaultMaxVolume} | fade: {defaultFade}".PadRight(w));

                string howmanybars = plotVolume(currentVolume2);
                Console.SetCursorPosition(0, 14);
                Console.WriteLine($"Current Volume: {currentVolume2} - [{howmanybars}]");


                Console.SetCursorPosition(0, 15);
                Console.WriteLine($"Media Keys: {mediaKeys}".PadRight(w));

                Console.SetCursorPosition(0, 16);
                Console.WriteLine($"Sound Alerts: {soundAlerts}".PadRight(w));

                Console.SetCursorPosition(0, 19);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("=================================================".PadRight(w));

                Console.SetCursorPosition(0, 20);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[✓] Total keystrokes saved: {keystrokesCounter}".PadRight(w));
                Console.ResetColor();

                Console.SetCursorPosition(0, 22);
                if (keystrokesCounter > 0)
                {
                    Console.ForegroundColor = blinkColors[blinkColorIndex % blinkColors.Length];
                    Console.WriteLine("Have you even said thanks...? twitch.tv/80smullet".PadRight(w));
                    blinkColorIndex++;
                }

                Console.SetCursorPosition(0, 23);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("=================================================".PadRight(w));

                Console.SetCursorPosition(0, 25);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{killStreakText}".PadRight(w));

                //Console.SetCursorPosition(0, 27);
                //Console.ForegroundColor = ConsoleColor.DarkGray;
                //Console.WriteLine($"[ESC] - Exit | [1] - Show Additional Options".PadRight(w));


                /*Console.SetCursorPosition(0, 24);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{roundKills}");*/


                if (csIsRunning)
                {
                    counterstrikemessage = "running...".PadRight(w);
                }
                else
                {
                    counterstrikemessage = "waiting...".PadRight(w);
                    activityStatus = "waiting...".PadRight(w);
                    phaseStatus = "waiting...".PadRight(w);
                    playerStatus = "waiting...".PadRight(w);
                    gameModeStatus = "waiting...".PadRight(w);
                }

                if (csIsRunning && webhookRecentlyActive) //...if it works it works
                {
                    if (!isConnected)
                    {
                        isConnected = true;
                        shouldPlaySound = true;
                    }
                    lastConnectionTime = DateTime.Now;
                    
                    if (!shownConnected)
                    {
                        currentStatusMessage2 = "[CONNECTED] - Leave this window open and play!".PadRight(w);
                        shownConnected = true;
                        shownCsNotRunning = false;
                        shownWebhookMissing = false;
                        shownBothMissing = false;
                    }
                }
                else
                {
                    if ((DateTime.Now - lastConnectionTime).TotalSeconds > 10)
                    {
                        isConnected = false;
                    }
                    shouldPlaySound = false;

                    // can only show one
                    if (!csIsRunning && !webhookRecentlyActive)
                    {
                        if (!shownBothMissing)
                        {
                            currentStatusMessage2 = "Please start Counter-Strike, awaiting webhooks.".PadRight(w);
                            shownBothMissing = true;
                            shownCsNotRunning = false;
                            shownWebhookMissing = false;
                            shownConnected = false;
                        }
                    }
                    else if (!csIsRunning)
                    {
                        if (!shownCsNotRunning && !shownBothMissing)
                        {
                            currentStatusMessage2 = "Please start Counter-Strike, leave window open.".PadRight(w);
                            shownCsNotRunning = true;
                            shownWebhookMissing = false;
                            shownConnected = false;
                        }
                    }
                    else if (!webhookRecentlyActive)
                    {
                        if (!shownWebhookMissing && !shownBothMissing)
                        {
                            currentStatusMessage2 = "Counter-Strike is running, waiting for webhooks.".PadRight(w);
                            shownWebhookMissing = true;
                            shownCsNotRunning = false;
                            shownConnected = false;
                        }
                    }
                }

                // always show a message on first run or when status changes
                if (!string.IsNullOrEmpty(currentStatusMessage2) && (currentStatusMessage2 != lastStatusMessage || firstRun))
                {
                    Console.SetCursorPosition(0, 18);
                    Console.ForegroundColor = isConnected ? ConsoleColor.DarkGreen : ConsoleColor.DarkYellow;
                    Console.WriteLine(currentStatusMessage2.PadRight(w)); 
                    Console.ResetColor();

                    lastStatusMessage = currentStatusMessage2;
                    firstRun = false;
                }

                // only play the sound once when connected
                if (shouldPlaySound)
                {
                    var player = new SoundPlayer();
                    string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "communications_on.wav");
                    player.PlaySound(soundPath, 0.4f);
                    shouldPlaySound = false;
                }
                Thread.Sleep(300);
            }
        }

        //  rest with blanks, so, that our string is ALWAYS 10 long.
        static string plotVolume(float currentVolume2)
        {
            float checkedvolume = currentVolume2;

            if (checkedvolume > 100) checkedvolume = 100; // just rangechecking here
            if (checkedvolume < 0) checkedvolume = 0;     // and here

            int INTVolume = (int)Math.Round(checkedvolume / 10); // Round first, then cast it to Integer

            string howmanybars = "";

            for (int i = 0; i < INTVolume; i++)
            {
                howmanybars += "●";
            }

            for (int i = 0; i < 10 - INTVolume; i++)
            {
                howmanybars += " ";
            }

            return howmanybars;
        }


        static string? FindCSGOInstallPath()
        {
            string gameName = "Counter-Strike Global Offensive";


            string? steamPath = null;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        steamPath = key.GetValue("SteamPath") as string;
                    }
                }
            }
            catch (Exception ex)
            {
                SoundPlayer.ButtonFail();
                Console.WriteLine($"[X] Error reading the registry.: {ex.Message}");
                return null;
            }

            if (string.IsNullOrEmpty(steamPath))
            {
                SoundPlayer.ButtonFail();
                Console.WriteLine("[X] Steam path not found in the registry.");
                return null;
            }

            string libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersPath))
            {
                SoundPlayer.ButtonFail();
                Console.WriteLine($"[X] libraryfolders.vdf is missing: {libraryFoldersPath}");
                return null;
            }

            try
            {
                string[] lines = File.ReadAllLines(libraryFoldersPath);
                foreach (string line in lines)
                {
                    Match match = Regex.Match(line, @"""path""\s+""(.*?)""");
                    if (match.Success)
                    {
                        string libraryPath = match.Groups[1].Value.Replace("\\\\", "\\");
                        string installPath = Path.Combine(libraryPath, "steamapps", "common", gameName, "game", "csgo", "cfg");

                        if (Directory.Exists(installPath))
                        {
                            return installPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SoundPlayer.ButtonFail();
                Console.WriteLine($"[X] Error interpreting libraryfolders.vdf: {ex.Message}");
                return null;
            }
            SoundPlayer.ButtonFail();
            Console.WriteLine("[X] The game could not be found in the libraries.");
            return null;
        }
        static string? SetupGameStateIntegration(string csgoPath)
        {
            string? cfgPath = FindCSGOInstallPath();
            if (cfgPath == null)
                return null;

            string configFileName = "gamestate_integration_media.cfg";
            string configFilePath = Path.Combine(cfgPath, configFileName);
            string port = "1337"; // Default port

            try
            {
                // New URI to set in the config
                string newUri = $"http://localhost:{port}/";

                
                string configContent = $@"""Console Sample v.1""
{{
 ""uri"" ""{newUri}""
 ""timeout"" ""1.0""
 ""buffer""  ""1""
 ""throttle"" ""0.5""
 ""heartbeat"" ""1.0""
 ""auth""
 {{
   ""token"" ""mywhatevertoken""
 }}
 ""output""
 {{
   ""precision_time"" ""3""
   ""precision_position"" ""1""
   ""precision_vector"" ""3""
 }}
 ""data""
 {{
   ""provider""            ""1""
   ""player_id""           ""1""
   ""player_state""        ""1""
   ""player_weapons""      ""0""
   ""map""                 ""1""
   ""round""               ""1""
   ""player_match_stats""  ""1""
 }}
}}";

                // overwrrite
                File.WriteAllText(configFilePath, configContent);

                // successful
                Console.ForegroundColor = ConsoleColor.Green;
                csWasFoundByAutoConfiguration = true;
                FoundSuccess = "[✓] gamestate_integration_media.cfg file replaced with new URI.";
                if (Process.GetProcessesByName("cs2").Any() || Process.GetProcessesByName("csgo").Any())
                {
                    cfgRecentlyUpdated = true;
                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                SoundPlayer.ButtonFail();
                Console.WriteLine($"[X] Error writing config file: {ex.Message}");
                return null;
            }

            return port;


        }
        static bool UpdateGamestateIntegrationURI(string newPort)
        {
            string? cfgPath = FindCSGOInstallPath();
            if (cfgPath == null)
                return false;

            string configFileName = "gamestate_integration_media.cfg";
            string configFilePath = Path.Combine(cfgPath, configFileName);

            if (File.Exists(configFilePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(configFilePath);
                    bool uriUpdated = false;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        // look for the line containing the URI
                        Match match = Regex.Match(lines[i], @"""uri""\s*""http:\/\/localhost:(\d+)\/""");
                        if (match.Success)
                        {
                            // port number replaced
                            lines[i] = lines[i].Replace(match.Groups[1].Value, newPort);
                            uriUpdated = true;
                            break;
                        }
                    }

                    if (uriUpdated)
                    {
                        // write the updated content back to the file
                        File.WriteAllLines(configFilePath, lines);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n[✓] URI updated successfully.");
                        Console.ResetColor();
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[X] URI not found in the config file.");
                        Console.ResetColor();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[X] Error reading or writing the config file: {ex.Message}");
                    Console.ResetColor();
                    return false;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[X] gamestate_integration_media.cfg file not found.");
                Console.ResetColor();
                return false;
            }
        }

        static string defaultContent = "";
        static bool CfgFoundOrNot(string configFileName, out bool wasGenerated)
        {
            wasGenerated = false;

            // Find the CSGO installation path
            string? cfgPath = FindCSGOInstallPath();
            if (cfgPath == null)
                return false;

            string configFilePath = Path.Combine(cfgPath, configFileName);

            if (File.Exists(configFilePath))
            {
                configFilePathPull = configFilePath;
                return true;
            }

            if (Directory.Exists(cfgPath))
            {
                string missingMsg =
        @$"
========================================================================================
It looks like you don't have a gamestate_integration_media.cfg present in your CS folder.

{cfgPath}

- Would you like to attempt to generate one now with default settings? (recommended)
========================================================================================";

                TypeWithBlockTrail(missingMsg, 1);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[1] Yes [2] No");
                Console.ResetColor();

                ConsoleKey input = Console.ReadKey(true).Key;
                Console.WriteLine();

                if (input == ConsoleKey.D1 || input == ConsoleKey.NumPad1)
                {
                    string defaultCfg =
 @"""Console Sample v.1""
{
 ""uri"" ""http://localhost:1337/""
 ""timeout"" ""1.0""
 ""buffer""  ""1""
 ""throttle"" ""0.5""
 ""heartbeat"" ""1.0""
 ""auth""
 {
   ""token"" ""mywhatevertoken""
 }
 ""output""
 {
   ""precision_time"" ""3""
   ""precision_position"" ""1""
   ""precision_vector"" ""3""
 }
 ""data""
 {
   ""provider""            ""1""
   ""player_id""           ""1""
   ""player_state""        ""1""
   ""player_weapons""      ""0""
   ""map""                 ""1""
   ""round""               ""1""
   ""player_match_stats""  ""1""
 }
}";

                    File.WriteAllText(configFilePath, defaultCfg);
                    configFilePathPull = configFilePath;
                    wasGenerated = true;
                    return true;
                }
            }
                return false;
        }
        public static void EnsureWebhookIsRunning()
        {
            if (listener == null)
            {
                StartWebhook();
            }
            else if (!listener.IsListening)
            {
                try
                {
                    listener.Start(); 
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[Error] Could not start listener: {ex.Message}");
                }
            }
        }

        /*static void CheckFileIntegrity() //probably useful at a later date...
        {
            string settingsSettings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
            string keyStrokesSettings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keystrokes_counter.txt");
            string webhookLogSettings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webhook_log.txt");
            string webhookPortSettings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webhook_port.txt");

            if (File.Exists(settingsSettings) &&
                File.Exists(keyStrokesSettings) &&
                File.Exists(webhookLogSettings) && 
                File.Exists(webhookPortSettings))
            {
                Console.WriteLine("File integrity intact");
            }
            else 
            {
                Console.WriteLine("File integrity corrupt?");
            }

        }*/

        static string? GetPortFromConfig()
        {
            string? cfgPath = FindCSGOInstallPath();
            if (cfgPath == null)
            {
                return null;
            }

            string configFileName = "gamestate_integration_media.cfg";
            string configFilePath = Path.Combine(cfgPath, configFileName);

            if (File.Exists(configFilePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(configFilePath);
                    foreach (string line in lines)
                    {
                        Match match = Regex.Match(line, @"""uri""\s*""http:\/\/localhost:(\d+)\/""");
                        if (match.Success)
                        {
                            foundPort = match.Groups[1].Value;
                            return foundPort;
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    SoundPlayer.ButtonFail();
                    Console.WriteLine("[X] No port found in the config file.");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    SoundPlayer.ButtonFail();
                    Console.WriteLine($"[X] Error reading the config file: {ex.Message}");
                    Console.ResetColor();
                }
            }
            else
            {
            }

            return null;
        }
        static void SplashScreen() //if it works it works
        {

            Console.CursorVisible = false;
            SoundPlayer.SplashScreen();
            if (isAdmin == false) 
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(
        @"  __  __       _ _      _     __  __          _ _              _____ _        _ _            __  ___  
 |  \/  |     | | |    | |   |  \/  |        | (_)            / ____| |      (_) |          / / / _ \ 
 | \  / |_   _| | | ___| |_  | \  / | ___  __| |_  __ _ _____| (___ | |_ _ __ _| | _____   / /_| (_) |
 | |\/| | | | | | |/ _ \ __| | |\/| |/ _ \/ _` | |/ _` |______\___ \| __| '__| | |/ / _ \ | '_ \\__, |
 | |  | | |_| | | |  __/ |_  | |  | |  __/ (_| | | (_| |      ____) | |_| |  | |   <  __/ | (_) | / / 
 |_|  |_|\__,_|_|_|\___|\__| |_|  |_|\___|\__,_|_|\__,_|     |_____/ \__|_|  |_|_|\_\___|  \___(_)_/.03");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("                                            ⠀⠀⠀⠀⠀⠀");

                // Line 2: art + menu
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀⠀        ⣼⣿⣿⣟⣻⣿⡏⠻⣿⣿⣦⡀⠀⠀⠀⠀⠀⠀       ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("It looks like you're running this without admin privileges.");

                // Line 3
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀        ⣼⣿w⣿⢿⣿⣿⠗⠀⢹⣿⣿⣟⡄⠀⠀⠀⠀⠀⠀      ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Functionality that may be affected:");

                // Line 4
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀        ⢰⣿⣿⣇⣻⡮⢟⠛⠛⠛⣻⣿⣿⣿⠘⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[X]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" Auto Configuration.");

                // Line 5
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀        ⢸⣿⣿⡿⣷⡰⠀⠀⣀⢉⣸⣿e⣿⡶⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[X]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" Media Keys");

                // Line 6: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀        ⢸⣿⣿⣽⣶⣦⡀⢾⣏⠐⣿⣿⣿⠿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 7
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀        ⠈⢟⡿b⠟⠿⣙⡄⠈⠓⡄⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("You'll be able to use the software regardless, but it is");

                // Line 8
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀        ⢰⠃⠀⠀⠀⠈⠉⡖⠀⡟⠢⡄⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("recommended to run as administrator! :)");

                // Line 9: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀⠀        h⠀⠀⠀⢢⠀⢀⡆⠀⡇⠀⠈⠢⡈⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 10
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀        ⠘⡆⠀⠀⠈o⢸⠀⠀⡇⠢⡀⠀⠈⠦⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[1]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" Go straight to main menu");

                // Line 11: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀⠀⠀        ⠹⡆⠀⠀⢨⠃⠀⠀⣇⠀⠈⠢⠀⠀⠈⢢⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 12
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀⠀        ⠀⠑o⠀⠇⠀⠀⠀⡇⠳⡀⠀⢱⠀⠀⠀⢳⡀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[2]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" Try running auto-configuration without admin privileges.");

                // Line 13
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀⠀⠀⠀        ⠱⣾⠀⠀⠀⠀⡇⠀⠑⡀⡸⠄⠀⠀⣜⢘⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" (highly recommended)");

                // Line 14: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀⠀⠀⠀⠀        ⠀⠙⣄⠀⠀k⢀⣤⡤⠼⣧⣤⣤⡾⠟⠁⠀⠀⠀⠀⠀⠀⠀    ");

                // Line 15: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀⠀⠀        ⢀⡠⠔⠉⠈⠣⠀⠀⠀⠀⠀⠀⣰⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    ");

                // Line 16
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀        ⢠e⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠆⠀⡇⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("By using this app, you agree to the ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Terms of Service");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("]");

                // Line 17: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀        ⣴⠛⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⡜⠀⢀⠃⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 18: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀        ⡼⠃⣸⡉⠢⠀⠀⢆⠀⠀⠀⠀⠀⠁⠀⠼r⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 19: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("        ⢰⠁⠀⠏⡇⠀⠈⠐⢬⣄⠀⠀⠀⠀⠀⢀⣼⢿⠅⠀⠀⠀⠀        ");

                // Line 20: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("        ⢸⠀⢠⢀⠁⠀⠀⠀⠀⢇⠈⠁⢒⠖⠊⠉⠀⠘⡆⠀⠀⠀⠀");

                // Line 21: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("        ⢸⠀⡜⡘⠀⠀⠀⠀⠀⠘⣦⠔⠁⠀⠀⠀⠀⠀⠸⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀\n");

                Console.ResetColor();


                /*Console.ForegroundColor = ConsoleColor.White;
                ConsoleHelper.ColoredWriteLine(
@"                                            ⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀        ⣼⣿⣿⣟⣻⣿⡏⠻⣿⣿⣦⡀⠀⠀⠀⠀⠀⠀       It looks like you're running this without admin privileges.  
⠀⠀⠀⠀        ⣼⣿w⣿⢿⣿⣿⠗⠀⢹⣿⣿⣟⡄⠀⠀⠀⠀⠀⠀      Functionality that may be affected:
⠀⠀⠀        ⢰⣿⣿⣇⣻⡮⢟⠛⠛⠛⣻⣿⣿⣿⠘⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀[X] Auto Configuration.⠀⠀⠀⠀
⠀⠀⠀        ⢸⣿⣿⡿⣷⡰⠀⠀⣀⢉⣸⣿e⣿⡶⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀[X] Media Keys
⠀⠀⠀        ⢸⣿⣿⣽⣶⣦⡀⢾⣏⠐⣿⣿⣿⠿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀        ⠈⢟⡿b⠟⠿⣙⡄⠈⠓⡄⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀You'll be able to use the software regardless, but it is⠀
⠀⠀⠀⠀        ⢰⠃⠀⠀⠀⠈⠉⡖⠀h⠢⡄⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀recommended to run as administrator! :)
⠀⠀⠀⠀        ⢸⠀⠀⠀⢢⠀⢀⡆⠀⡇⠀⠈⠢⡈⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀        ⠘⡆⠀⠀⠈o⢸⠀⠀⡇⠢⡀⠀⠈⠦⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀[1] Go straight to main menu
⠀⠀⠀⠀⠀        ⠹⡆⠀⠀⢨⠃⠀⠀⣇⠀⠈⠢⠀⠀⠈⢢⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀        ⠀o⡀⠀⠇⠀⠀⠀⡇⠳⡀⠀⢱⠀⠀ ⢳⡀⠀⠀⠀⠀⠀⠀⠀[2] Try running auto-configuration without admin privileges.⠀
⠀⠀⠀⠀⠀⠀⠀        ⠱⣾⠀⠀⠀⠀⡇⠀⠑⡀⡸⠄⠀⠀⣜⢘⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀ (highly recommended)
⠀⠀⠀⠀⠀⠀⠀        ⠀⠙⣄⠀⠀k⢀⣤⡤⠼⣧⣤⣤⡾⠟⠁⠀⠀⠀⠀⠀⠀⠀    
⠀⠀⠀⠀⠀        ⢀⡠⠔⠉⠈⠣⠀⠀⠀⠀⠀⠀⣰⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    
⠀⠀⠀        ⢠e⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠆⠀⡇⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    By using this app, you agree to the [Terms of Service]
⠀⠀        ⣴⠛⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⡜⠀⢀⠃⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀        ⡼⠃⣸⡉⠢⠀⠀⢆⠀⠀⠀⠀⠀⠁⠀r⣦⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢰⠁⠀⠏⡇⠀⠈⠐⢬⣄⠀⠀⠀⠀⠀⢀⣼⢿⠅⠀⠀⠀⠀        
        ⢸⠀⢠⢀⠁⠀⠀⠀⠀⢇⠈⠁⢒⠖⠊⠉⠀⠘⡆⠀⠀⠀⠀
        ⢸⠀⡜⡘⠀⠀⠀⠀⠀⠘⣦⠔⠁⠀⠀⠀⠀⠀⠸⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀");*/
                Console.ForegroundColor = ConsoleColor.DarkGray;
                string mySocials = "       [ twitch.tv/80smullet | twitter.com/80smullet | youtube.com/@Cenote | discord.gg/WMcAHPNFef ]";
                TypeWithBlockTrail(mySocials, 10);
                Console.ResetColor();

                bool validInput = false;
                while (!validInput)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.D2:
                            validInput = true;
                            SoundPlayer.SplashScreenDoor();
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(
                @"  __  __       _ _      _     __  __          _ _              _____ _        _ _            __  ___  
 |  \/  |     | | |    | |   |  \/  |        | (_)            / ____| |      (_) |          / / / _ \ 
 | \  / |_   _| | | ___| |_  | \  / | ___  __| |_  __ _ _____| (___ | |_ _ __ _| | _____   / /_| (_) |
 | |\/| | | | | | |/ _ \ __| | |\/| |/ _ \/ _` | |/ _` |______\___ \| __| '__| | |/ / _ \ | '_ \\__, |
 | |  | | |_| | | |  __/ |_  | |  | |  __/ (_| | | (_| |      ____) | |_| |  | |   <  __/ | (_) | / / 
 |_|  |_|\__,_|_|_|\___|\__| |_|  |_|\___|\__,_|_|\__,_|     |_____/ \__|_|  |_|_|\_\___|  \___(_)_/.03");
                            var player = new SoundPlayer();
                            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "targetting_system.wav");
                            player.PlaySound(soundPath, 0.4f);
                            gray = true;
                            validKey = false;
                            string autoConfigWarning =

        @"========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

This automatic configuration will attempt to look for your Counter-Strike cfg
folder and either read or generate the appropriate gamestate_integration_media.cfg.

[!] Please turn off Counter-Strike if you have it running until the auto-configuration
has completed!

[!] You aren't running as administrator, but fingers crossed!
========================================================================================";
                            TypeWithBlockTrail(autoConfigWarning, 10);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Press any key to continue...");
                            Console.ResetColor();
                            Console.ReadKey();
                            SoundPlayer.NextSound();
                            gray = true;
                            validKey = false;
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(
                @"  __  __       _ _      _     __  __          _ _              _____ _        _ _            __  ___  
 |  \/  |     | | |    | |   |  \/  |        | (_)            / ____| |      (_) |          / / / _ \ 
 | \  / |_   _| | | ___| |_  | \  / | ___  __| |_  __ _ _____| (___ | |_ _ __ _| | _____   / /_| (_) |
 | |\/| | | | | | |/ _ \ __| | |\/| |/ _ \/ _` | |/ _` |______\___ \| __| '__| | |/ / _ \ | '_ \\__, |
 | |  | | |_| | | |  __/ |_  | |  | |  __/ (_| | | (_| |      ____) | |_| |  | |   <  __/ | (_) | / / 
 |_|  |_|\__,_|_|_|\___|\__| |_|  |_|\___|\__,_|_|\__,_|     |_____/ \__|_|  |_|_|\_\___|  \___(_)_/");
                            string autoConfigWarning2 =

        @"========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

If it fails you can also set it up manually following the instructions in the readme.
It's really easy!

- Shoutout to FisheyePG for helping make this auto-configuration wizard. Absolute Chad!
========================================================================================";
                            TypeWithBlockTrail(autoConfigWarning2, 10);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Press any key to continue...");
                            Console.ResetColor();
                            Console.ReadKey();
                            SoundPlayer.NextSound();

                            gray = true;
                            validKey = false;
                            string? path = FindCSGOInstallPath();
                            if (path != null)
                            {
                                csFound = "[✓] Counter-Strike cfg folder found.";

                                string? port = SetupGameStateIntegration(path);
                                if (port != null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    csPortSet = "[✓] Ready to communicate on port: ";
                                    bool parsing = int.TryParse(port, out int portInt);
                                    webhookPort = portInt;
                                    ResetWebhookToDefault();
                                    SavePort();
                                    if (Process.GetProcessesByName("cs2").Any() || Process.GetProcessesByName("csgo").Any())
                                    {
                                        autoConfigCSRunning = "- It looks like Counter-Strike is running, please restart the game.";
                                        cfgRecentlyUpdated = true;
                                    }
                                    else
                                    {
                                        autoConfigCSRunning = "- Just start Counter-Strike and select your media software and you're ready to go!";
                                    }
                                }
                                else
                                {
                                    SoundPlayer.ButtonFail();
                                    csPortSet = "[X] failed to set port";
                                }
                            }
                            else
                            {
                                //SoundPlayer.ButtonFail();
                                csFound = "[X] Counter-Strike cfg folder could not be found.";
                            }
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(
                @"  __  __       _ _      _     __  __          _ _              _____ _        _ _            __  ___  
 |  \/  |     | | |    | |   |  \/  |        | (_)            / ____| |      (_) |          / / / _ \ 
 | \  / |_   _| | | ___| |_  | \  / | ___  __| |_  __ _ _____| (___ | |_ _ __ _| | _____   / /_| (_) |
 | |\/| | | | | | |/ _ \ __| | |\/| |/ _ \/ _` | |/ _` |______\___ \| __| '__| | |/ / _ \ | '_ \\__, |
 | |  | | |_| | | |  __/ |_  | |  | |  __/ (_| | | (_| |      ____) | |_| |  | |   <  __/ | (_) | / / 
 |_|  |_|\__,_|_|_|\___|\__| |_|  |_|\___|\__,_|_|\__,_|     |_____/ \__|_|  |_|_|\_\___|  \___(_)_/");
                            string autoConfigMessage =
        @$"========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

{FoundSuccess}
{configFilePathForAutoConfig}

{csPortSet}{webhookPort}

{autoConfigCSRunning}
========================================================================================";
                            TypeWithBlockTrail(autoConfigMessage, 10);
                            Console.ForegroundColor = ConsoleColor.Green;
                            SoundPlayer.SuccessSoundEffect();
                            Console.WriteLine("Press any key to save and continue.");
                            ResetWebhookToDefault();
                            Console.ResetColor();
                            gray = false;
                            autoConfigCSRunning = "";
                            csFound = "";
                            csPortSet = "";
                            Console.ReadKey();
                            SoundPlayer.NextSound();
                            break;

                        case ConsoleKey.D1:
                            validInput = true;
                            SoundPlayer.NextSound();
                            break;

                        default:
                            SoundPlayer.ButtonFail();
                            break;
                    }
                }
            }

            else if (isAdmin == true)
            {
                Console.CursorVisible = false;
                SoundPlayer.SplashScreen();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(
        @"  __  __       _ _      _     __  __          _ _              _____ _        _ _            __  ___  
 |  \/  |     | | |    | |   |  \/  |        | (_)            / ____| |      (_) |          / / / _ \ 
 | \  / |_   _| | | ___| |_  | \  / | ___  __| |_  __ _ _____| (___ | |_ _ __ _| | _____   / /_| (_) |
 | |\/| | | | | | |/ _ \ __| | |\/| |/ _ \/ _` | |/ _` |______\___ \| __| '__| | |/ / _ \ | '_ \\__, |
 | |  | | |_| | | |  __/ |_  | |  | |  __/ (_| | | (_| |      ____) | |_| |  | |   <  __/ | (_) | / / 
 |_|  |_|\__,_|_|_|\___|\__| |_|  |_|\___|\__,_|_|\__,_|     |_____/ \__|_|  |_|_|\_\___|  \___(_)_/0.3");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("                                            ⠀⠀⠀⠀⠀⠀");

                // Line 2: art + menu
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀⠀        ⣼⣿⣿⣟⣻⣿⡏⠻⣿⣿⣦⡀⠀⠀⠀⠀⠀⠀       ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");

                // Line 3
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀        ⣼⣿w⣿⢿⣿⣿⠗⠀⢹⣿⣿⣟⡄⠀⠀⠀⠀⠀⠀      ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");

                // Line 4
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀        ⢰⣿⣿⣇⣻⡮⢟⠛⠛⠛⣻⣿⣿⣿⠘⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");

                // Line 5
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀        ⢸⣿⣿⡿⣷⡰⠀⠀⣀⢉⣸⣿e⣿⡶⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");

                // Line 6: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀        ⢸⣿⣿⣽⣶⣦⡀⢾⣏⠐⣿⣿⣿⠿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 7
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀        ⠈⢟⡿b⠟⠿⣙⡄⠈⠓⡄⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");

                // Line 8
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀        ⢰⠃⠀⠀⠀⠈⠉⡖⠀⡟⠢⡄⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");

                // Line 9: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀⠀        h⠀⠀⠀⢢⠀⢀⡆⠀⡇⠀⠈⠢⡈⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 10
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀        ⠘⡆⠀⠀⠈o⢸⠀⠀⡇⠢⡀⠀⠈⠦⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[1]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" Go straight to main menu.");

                // Line 11: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀⠀⠀        ⠹⡆⠀⠀⢨⠃⠀⠀⣇⠀⠈⠢⠀⠀⠈⢢⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 12
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀⠀        ⠀⠑o⠀⠇⠀⠀⠀⡇⠳⡀⠀⢱⠀⠀⠀⢳⡀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[2]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" Run auto-configuration with admin privileges.");

                // Line 13
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀⠀⠀⠀⠀        ⠱⣾⠀⠀⠀⠀⡇⠀⠑⡀⡸⠄⠀⠀⣜⢘⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" (highly recommended)");

                // Line 14: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀⠀⠀⠀⠀        ⠀⠙⣄⠀⠀k⢀⣤⡤⠼⣧⣤⣤⡾⠟⠁⠀⠀⠀⠀⠀⠀⠀    ");

                // Line 15: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀⠀⠀⠀        ⢀⡠⠔⠉⠈⠣⠀⠀⠀⠀⠀⠀⣰⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    ");

                // Line 16
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("⠀⠀⠀        ⢠e⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠆⠀⡇⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("By using this app, you agree to the ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Terms of Service");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("]");

                // Line 17: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀⠀        ⣴⠛⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⡜⠀⢀⠃⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 18: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("⠀        ⡼⠃⣸⡉⠢⠀⠀⢆⠀⠀⠀⠀⠀⠁⠀⠼r⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀");

                // Line 19: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("        ⢰⠁⠀⠏⡇⠀⠈⠐⢬⣄⠀⠀⠀⠀⠀⢀⣼⢿⠅⠀⠀⠀⠀        ");

                // Line 20: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("        ⢸⠀⢠⢀⠁⠀⠀⠀⠀⢇⠈⠁⢒⠖⠊⠉⠀⠘⡆⠀⠀⠀⠀");

                // Line 21: art only
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("        ⢸⠀⡜⡘⠀⠀⠀⠀⠀⠘⣦⠔⠁⠀⠀⠀⠀⠀⠸⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀\n");

                // 3) Reset to defaults when done
                Console.ResetColor();





                /*Console.ForegroundColor = ConsoleColor.White;
                    ConsoleHelper.ColoredWriteLine(
            @"                                            ⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀        ⣼⣿⣿⣟⣻⣿⡏⠻⣿⣿⣦⡀⠀⠀⠀⠀⠀⠀        
⠀⠀⠀⠀        ⣼⣿⣿⣿⢿⣿⣿⠗⠀⢹⣿⣿⣟⡄⠀⠀⠀⠀⠀⠀      
⠀⠀⠀        ⢰⣿⣿⣇⣻⡮⢟⠛⠛⠛⣻⣿⣿⣿⠘⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀        ⢸⣿⣿⡿⣷⡰⠀⠀⣀⢉⣸⣿⣿⣿⡶⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀        ⢸⣿⣿⣽⣶⣦⡀⢾⣏⠐⣿⣿⣿⠿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀        ⠈⢟⡿⠿⠟⠿⣙⡄⠈⠓⡄⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀        ⢰⠃⠀⠀⠀⠈⠉⡖⠀⡟⠢⡄⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀        ⢸⠀⠀⠀⢢⠀⢀⡆⠀⡇⠀⠈⠢⡈⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀        ⠘⡆⠀⠀⠈⣧⢸⠀⠀⡇⠢⡀⠀⠈⠦⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀[1] Go straight to main menu
⠀⠀⠀⠀⠀        ⠹⡆⠀⠀⢨⠃⠀⠀⣇⠀⠈⠢⠀⠀⠈⢢⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀        ⠀⠑⡀⠀⠇⠀⠀⠀⡇⠳⡀⠀⢱⠀⠀⠀⢳⡀⠀⠀⠀⠀⠀⠀⠀[2] Run auto-configuration before starting
⠀⠀⠀⠀⠀⠀⠀        ⠱⣾⠀⠀⠀⠀⡇⠀⠑⡀⡸⠄⠀⠀⣜⢘⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀ (highly recommended)
⠀⠀⠀⠀⠀⠀⠀        ⠀⠙⣄⠀⠀⣰⢀⣤⡤⠼⣧⣤⣤⡾⠟⠁⠀⠀⠀⠀⠀⠀⠀    
⠀⠀⠀⠀⠀        ⢀⡠⠔⠉⠈⠣⠀⠀⠀⠀⠀⠀⣰⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    
⠀⠀⠀        ⢠⡖⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠆⠀⡇⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    By using this app, you agree to the [Terms of Service]
⠀⠀        ⣴⠛⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⡜⠀⢀⠃⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀        ⡼⠃⣸⡉⠢⠀⠀⢆⠀⠀⠀⠀⠀⠁⠀⠼⣦⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢰⠁⠀⠏⡇⠀⠈⠐⢬⣄⠀⠀⠀⠀⠀⢀⣼⢿⠅⠀⠀⠀⠀        
        ⢸⠀⢠⢀⠁⠀⠀⠀⠀⢇⠈⠁⢒⠖⠊⠉⠀⠘⡆⠀⠀⠀⠀
        ⢸⠀⡜⡘⠀⠀⠀⠀⠀⠘⣦⠔⠁⠀⠀⠀⠀⠀⠸⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀");*/
                Console.ForegroundColor = ConsoleColor.DarkGray;
                string mySocials = "       [ twitch.tv/80smullet | twitter.com/80smullet | youtube.com/@Cenote | discord.gg/WMcAHPNFef ]";
                TypeWithBlockTrail(mySocials, 10);
                Console.ResetColor();

                bool validInput = false;
                while (!validInput)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.D2:
                            validInput = true;
                            SoundPlayer.SplashScreenDoor();
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(
                @"  __  __       _ _      _     __  __          _ _              _____ _        _ _            __  ___  
 |  \/  |     | | |    | |   |  \/  |        | (_)            / ____| |      (_) |          / / / _ \ 
 | \  / |_   _| | | ___| |_  | \  / | ___  __| |_  __ _ _____| (___ | |_ _ __ _| | _____   / /_| (_) |
 | |\/| | | | | | |/ _ \ __| | |\/| |/ _ \/ _` | |/ _` |______\___ \| __| '__| | |/ / _ \ | '_ \\__, |
 | |  | | |_| | | |  __/ |_  | |  | |  __/ (_| | | (_| |      ____) | |_| |  | |   <  __/ | (_) | / / 
 |_|  |_|\__,_|_|_|\___|\__| |_|  |_|\___|\__,_|_|\__,_|     |_____/ \__|_|  |_|_|\_\___|  \___(_)_/0.3");
                            var player = new SoundPlayer();
                            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "targetting_system.wav");
                            player.PlaySound(soundPath, 0.4f);
                            gray = true;
                            validKey = false;
                            string autoConfigWarning =

        @"========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

This automatic configuration will attempt to look for your Counter-Strike cfg
folder and either read or generate the appropriate gamestate_integration_media.cfg.

[!] Please turn off Counter-Strike if you have it running until the auto-configuration
has completed!

========================================================================================";
                            TypeWithBlockTrail(autoConfigWarning, 10);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Press any key to continue...");
                            Console.ResetColor();
                            Console.ReadKey();
                            SoundPlayer.NextSound();
                            gray = true;
                            validKey = false;
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(
                @"  __  __       _ _      _     __  __          _ _              _____ _        _ _            __  ___  
 |  \/  |     | | |    | |   |  \/  |        | (_)            / ____| |      (_) |          / / / _ \ 
 | \  / |_   _| | | ___| |_  | \  / | ___  __| |_  __ _ _____| (___ | |_ _ __ _| | _____   / /_| (_) |
 | |\/| | | | | | |/ _ \ __| | |\/| |/ _ \/ _` | |/ _` |______\___ \| __| '__| | |/ / _ \ | '_ \\__, |
 | |  | | |_| | | |  __/ |_  | |  | |  __/ (_| | | (_| |      ____) | |_| |  | |   <  __/ | (_) | / / 
 |_|  |_|\__,_|_|_|\___|\__| |_|  |_|\___|\__,_|_|\__,_|     |_____/ \__|_|  |_|_|\_\___|  \___(_)_/0.3");
                            string autoConfigWarning2 =

        @"========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

If it fails you can also set it up manually following the instructions in the readme.
It's really easy!

- Shoutout to FisheyePG for helping make this auto-configuration wizard. Absolute Chad!
========================================================================================";
                            TypeWithBlockTrail(autoConfigWarning2, 10);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Press any key to continue...");
                            Console.ResetColor();
                            Console.ReadKey();
                            SoundPlayer.NextSound();

                            gray = true;
                            validKey = false;
                            string? path = FindCSGOInstallPath();
                            if (path != null)
                            {
                                csFound = "[✓] Counter-Strike cfg folder found.";

                                string? port = SetupGameStateIntegration(path);
                                if (port != null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    csPortSet = "[✓] Ready to communicate on port: ";
                                    bool parsing = int.TryParse(port, out int portInt);
                                    webhookPort = portInt;
                                    ResetWebhookToDefault();
                                    SavePort();
                                    if (Process.GetProcessesByName("cs2").Any() || Process.GetProcessesByName("csgo").Any())
                                    {
                                        autoConfigCSRunning = "- It looks like Counter-Strike is running, please restart the game.";
                                        cfgRecentlyUpdated = true;
                                    }
                                    else
                                    {
                                        autoConfigCSRunning = "- Just start Counter-Strike and select your media software and you're ready to go!";
                                    }
                                }
                                else
                                {
                                    SoundPlayer.ButtonFail();
                                    csPortSet = "[X] failed to set port";
                                }
                            }
                            else
                            {
                                SoundPlayer.ButtonFail();
                                csFound = "[X] Counter-Strike cfg folder could not be found.";
                            }
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(
                @"  __  __       _ _      _     __  __          _ _              _____ _        _ _            __  ___  
 |  \/  |     | | |    | |   |  \/  |        | (_)            / ____| |      (_) |          / / / _ \ 
 | \  / |_   _| | | ___| |_  | \  / | ___  __| |_  __ _ _____| (___ | |_ _ __ _| | _____   / /_| (_) |
 | |\/| | | | | | |/ _ \ __| | |\/| |/ _ \/ _` | |/ _` |______\___ \| __| '__| | |/ / _ \ | '_ \\__, |
 | |  | | |_| | | |  __/ |_  | |  | |  __/ (_| | | (_| |      ____) | |_| |  | |   <  __/ | (_) | / / 
 |_|  |_|\__,_|_|_|\___|\__| |_|  |_|\___|\__,_|_|\__,_|     |_____/ \__|_|  |_|_|\_\___|  \___(_)_/0.3");
                            string autoConfigMessage =
        @$"========================================================================================
**Mullet Media-Strike 6.9 automatic port configuration**:

{FoundSuccess}
{configFilePathForAutoConfig}

{csPortSet}{webhookPort}

{autoConfigCSRunning}
========================================================================================";
                            TypeWithBlockTrail(autoConfigMessage, 10);
                            Console.ForegroundColor = ConsoleColor.Green;
                            SoundPlayer.SuccessSoundEffect();
                            Console.WriteLine("Press any key to save and continue.");
                            ResetWebhookToDefault();
                            Console.ResetColor();
                            gray = false;
                            autoConfigCSRunning = "";
                            csFound = "";
                            csPortSet = "";
                            Console.ReadKey();
                            SoundPlayer.NextSound();
                            break;

                        case ConsoleKey.D1:
                            validInput = true;
                            SoundPlayer.NextSound();
                            break;

                        default:
                            SoundPlayer.ButtonFail();
                            break;
                    }
                }
            }
        }




        public static bool IsRunAsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}





