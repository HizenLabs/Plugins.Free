using Cysharp.Threading.Tasks;
using Facepunch;
using Oxide.Plugins;
using System;
using System.Linq;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private static class ChangeMonitor
    {
        private static Timer _timer;
        private static float _interval;

        /// <summary>
        /// Checks if the settings have changed and if the timer needs to be restarted.
        /// </summary>
        private static bool SettingsChanged => Settings.General.BuildMonitorInterval != _interval;

        /// <summary>
        /// Monitors build changes and triggers the appropriate actions.
        /// </summary>
        public static void Start()
        {
            _interval = Settings.General.BuildMonitorInterval;
            _timer = _instance.timer.Every(_interval, () => ProcessIntervalAsync().Forget());
        }

        /// <summary>
        /// Stops monitoring build changes.
        /// </summary>
        public static void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }

        /// <summary>
        /// Restarts the build change monitoring process. Typically used when settings are changed.
        /// </summary>
        public static void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Processes the interval for monitoring build changes. This is where the main logic for handling changes occurs.
        /// </summary>
        public static async UniTaskVoid ProcessIntervalAsync()
        {
            if (SettingsChanged)
            {
                Restart();
                return;
            }

            try
            {
                _isProcessing = true;

                // Copy the recordings to a new dictionary in case the original collection is modified while iterating
                using var recordings = Pool.Get<PooledDictionary<ulong, ChangeManagement.BaseRecording>>();
                recordings.AddRange(ChangeManagement.Recordings);

                foreach (var recording in recordings)
                {
                    await HandlePendingChangesAsync(recording.Key, recording.Value);

                    await UniTask.Yield();
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Handles pending changes for a specific recording.
        /// </summary>
        /// <param name="expectedId">The expected ID of the recording.</param>
        /// <param name="recording">The recording to handle.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async UniTask HandlePendingChangesAsync(ulong expectedId, ChangeManagement.BaseRecording recording)
        {
            // Ensure that the recording hasn't changed since we started processing
            if (!recording.IsValid || recording.Id != expectedId)
            {
                return;
            }

            // Check if the recording has any pending changes
            if (!recording.PendingRecords.Any())
            {
                return;
            }

            // Check if the recording was saved recently
            var timeSinceLastSave = DateTime.UtcNow - recording.LastSuccessfulSaveTime;
            if (timeSinceLastSave.TotalSeconds < Settings.General.DelayBetweenSaves)
            {
                return;
            }

            // Begin attempting to save the recording
            await recording.AttemptSaveAsync();
        }
    }
}
