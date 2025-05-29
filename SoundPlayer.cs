using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Newtonsoft.Json.Linq;

namespace Mullet_Media_Strike_6._9
{
    public class SoundPlayer
    {
        private IWavePlayer outputDevice;
        private AudioFileReader audioFile;

        public void PlaySound(string filePath, float volume = 1.0f)
        {
            Stop(); // stop sound that is playing

            outputDevice = new WaveOutEvent();
            audioFile = new AudioFileReader(filePath)
            {
                Volume = volume // set volume 0.0 = mute 1.0 = full
            };
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }

        public void Stop()
        {
            outputDevice?.Stop();
            outputDevice?.Dispose();
            outputDevice = null;

            audioFile?.Dispose();
            audioFile = null;
        }

        // Shared sound player instances (to prevent sound stacking)
        private static readonly SoundPlayer launchSelectPlayer = new SoundPlayer();
        private static readonly SoundPlayer escapePlayer = new SoundPlayer();
        private static readonly SoundPlayer splashScreenPlayer = new SoundPlayer();
        private static readonly SoundPlayer buttonFailPlayer = new SoundPlayer();
        private static readonly SoundPlayer splashScreenDoor = new SoundPlayer();
        //private static readonly SoundPlayer nextSound = new SoundPlayer();
        private static readonly SoundPlayer successSoundEffect = new SoundPlayer();
        private static DateTime lastFailSoundTime = DateTime.MinValue;
        public static void LaunchSelect()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "launch_select2.wav");
            launchSelectPlayer.Stop();
            launchSelectPlayer.PlaySound(soundPath, 0.5f);
        }

        public static void EscapeButton()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "launch_glow1.wav");
            escapePlayer.Stop();
            escapePlayer.PlaySound(soundPath, 0.5f);
        }

        public static void SplashScreen()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "Splash Screen 4.wav");
            splashScreenPlayer.Stop();
            splashScreenPlayer.PlaySound(soundPath, 0.3f);
        }

        public static void ButtonFail()
        {
            if ((DateTime.Now - lastFailSoundTime).TotalMilliseconds < 600)
                return; // Too soon to play again

            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "button2 fail.wav");
            buttonFailPlayer.Stop();
            buttonFailPlayer.PlaySound(soundPath, 0.3f);

            lastFailSoundTime = DateTime.Now;
        }
        public static void SplashScreenDoor()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "doormove2.wav");
            buttonFailPlayer.Stop();
            buttonFailPlayer.PlaySound(soundPath, 0.2f);
        }
        public static void NextSound()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "launch_dnmenu1.wav");
            buttonFailPlayer.Stop();
            buttonFailPlayer.PlaySound(soundPath, 0.3f);
        }
        public static void SuccessSoundEffect()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "bell.wav");
            buttonFailPlayer.Stop();
            buttonFailPlayer.PlaySound(soundPath, 0.2f);
        }
        public static void DoubleKill()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "Unreal Tournament DoubleKill.mp3");
            //buttonFailPlayer.Stop();
            buttonFailPlayer.PlaySound(soundPath, 0.5f);
        }
        public static void MultiKill()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "Unreal Tournament Multi Kill.mp3");
            //buttonFailPlayer.Stop();
            buttonFailPlayer.PlaySound(soundPath, 0.5f);
        }
        public static void UltraKill()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "Unreal Tournament Ultra Kill.mp3");
            //buttonFailPlayer.Stop();
            buttonFailPlayer.PlaySound(soundPath, 0.5f);
        }
        public static void MonsterKill()
        {
            string soundPath = Path.Combine(AppContext.BaseDirectory, "sounds", "Unreal Tournament Monster Kill.mp3");
            //buttonFailPlayer.Stop();
            buttonFailPlayer.PlaySound(soundPath, 0.5f);
        }

    }
}
