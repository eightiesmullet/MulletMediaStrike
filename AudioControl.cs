using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;

namespace Mullet_Media_Strike_6._9
{
    public class AudioControl
    {
        // method to mute or adjust the volume for application
        public static void AdjustAppVolume(string selectedProcessName, bool mute, float volumePercentage)
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(selectedProcessName));
            if (processes.Length == 0) return;

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
            MMDevice device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                string sessionId = session.GetSessionIdentifier.ToString().ToLower();

                if (sessionId.Contains(selectedProcessName.ToLower()))
                {
                    // Mute or adjust volume based on the mute parameter and volume percentage
                    session.SimpleAudioVolume.Mute = mute;
                    session.SimpleAudioVolume.Volume = volumePercentage / 100.0f; // Convert percentage to float (0-1)
                    break;
                }
            }
        }

        // Method to set volume to 50% for app
        public static void SetVolumeTo50(string selectedProcessName)
        {
            AdjustAppVolume(selectedProcessName, false, 50);
        }

        // Method to set volume to 100% for app
        public static void SetVolumeTo100(string selectedProcessName)
        {
            AdjustAppVolume(selectedProcessName, false, 100);
        }
        public static float GetAppVolume(string selectedProcessName)
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(selectedProcessName));
            if (processes.Length == 0) return -1;

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
            MMDevice device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                string sessionId = session.GetSessionIdentifier.ToString().ToLower();

                if (sessionId.Contains(selectedProcessName.ToLower()))
                {
                    float volume = session.SimpleAudioVolume.Volume * 100.0f;
                    return (float)Math.Round(volume); // return whole number %
                }
            }

            return -1;
        }
        public static void FadeToVolume(string selectedProcessName, float targetVolume, int durationMs = 1000, int steps = 20)
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(selectedProcessName));
            if (processes.Length == 0) return;

            MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
            MMDevice device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                string sessionId = session.GetSessionIdentifier.ToString().ToLower();

                if (sessionId.Contains(selectedProcessName.ToLower()))
                {
                    var simpleVolume = session.SimpleAudioVolume;
                    float currentVolume = simpleVolume.Volume;
                    float target = targetVolume / 100.0f;
                    float stepSize = (target - currentVolume) / steps;
                    int delay = durationMs / steps;

                    for (int s = 0; s < steps; s++)
                    {
                        currentVolume += stepSize;
                        simpleVolume.Volume = Math.Clamp(currentVolume, 0f, 1f);
                        Thread.Sleep(delay);
                    }

                    // set exact volume at the end to avoid rounding error
                    simpleVolume.Volume = target;
                    return;
                }
            }
        }

    }
}