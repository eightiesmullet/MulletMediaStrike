using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mullet_Media_Strike_6._9
{
    public class MediaControl
    {
        const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
        const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public static void Play()
        {
            SendPlayPauseKey();
        }


        public static void Pause()
        {
            SendPlayPauseKey();
        }

        
         public static void SendPlayPauseKey()
          {
              keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, UIntPtr.Zero);
              keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
          }

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        const int WM_APPCOMMAND = 0x0319;

        public enum SpotifyAction : long
        {
            PlayPause = 917504,
            Mute = 524288,
            VolumeDown = 589824,
            VolumeUp = 655360,
            Stop = 851968,
            PreviousTrack = 786432,
            NextTrack = 720896,
            Play = 3014656,
            Pause = 3080192
        }
        public static void SendPlayTargettedPauseKey(string selectedProcessName)
        {
            if (selectedProcessName == "spotify.exe")
            {
                Process[] spotify = Process.GetProcessesByName("Spotify");
                if (spotify.Length == 0) return;
                if (spotify[0] != null)
                {
                    SendMessage(spotify[0].MainWindowHandle, WM_APPCOMMAND, 0, new IntPtr((long)SpotifyAction.PlayPause));
                }
            }
            if (selectedProcessName == "vlc.exe")
            {
                Process[] vlc = Process.GetProcessesByName("vlc");
                if (vlc.Length == 0) return;
                if (vlc[0] != null)
                {
                    SendMessage(vlc[0].MainWindowHandle, WM_APPCOMMAND, 0, new IntPtr((long)SpotifyAction.PlayPause));
                }
            }

            if (selectedProcessName == "chrome.exe")
            {
                Process[] chrome = Process.GetProcessesByName("chrome");
                if (chrome.Length == 0) return;
                if (chrome[0] != null)
                {

                    SendMessage(chrome[0].MainWindowHandle, WM_APPCOMMAND, 0, new IntPtr((long)SpotifyAction.PlayPause));
                }
            }


            /*if (selectedProcessName == "winamp.exe")
            {
                Process[] winamp = Process.GetProcessesByName("winamp");
                if (winamp.Length == 0) return;
                if (winamp[0] != null)
                {

                    SendMessage(winamp[0].MainWindowHandle, WM_APPCOMMAND, 0, new IntPtr((long)40046));
                }
            }*/
        }
    }
}
