using Android.App;
using Android.Content;
using Android.OS;
using Plugin.Maui.Audio;
using AndroidX.Media;
using AndroidX.Core.App;
using System;
using Android.Content.PM;

namespace MusicPlayer.Services
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaPlayback)]
    public class MusicPlayerService : Service
    {
        private AudioService _audioService = new AudioService();
        private PendingIntent _playPausePendingIntent;
        private NotificationCompat.Builder _notificationBuilder;

        public override IBinder OnBind(Intent intent)
        {
            return null; // No binding needed for background playback
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);

            var action = intent.Action;
            var filePath = intent.GetStringExtra("filePath");

            // Handle Play/Pause action from the notification
            if (action == "com.musicplayer.PLAY_PAUSE")
            {
                if (_audioService.IsPlaying)
                {
                    _audioService.Pause(); // Pause music if it's playing
                }
                else
                {
                    _audioService.Resume(); // Resume music if paused
                }

                // Update notification after play/pause action
                UpdateNotification();
            }
            else if (!string.IsNullOrEmpty(filePath)) // Start playing a new file if filePath is passed
            {
                _audioService.Play(filePath); // Play the new music file
                UpdateNotification(); // Update notification after starting music
            }

            // Create the PendingIntent for the play/pause button
            var playPauseIntent = new Intent(this, typeof(MusicPlayerService));
            playPauseIntent.SetAction("com.musicplayer.PLAY_PAUSE");
            _playPausePendingIntent = PendingIntent.GetService(this, 0, playPauseIntent, PendingIntentFlags.Mutable);

            // Create the notification channel if necessary
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    "music_player_channel",
                    "Music Player",
                    NotificationImportance.Low
                );
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }

            // Initialize notification builder
            _notificationBuilder = new NotificationCompat.Builder(this, "music_player_channel")
                .SetContentTitle("Music Player")
                .SetContentText(_audioService.IsPlaying ? "Playing music..." : "Paused")
                .SetSmallIcon(Resource.Drawable.exo_icon_pause) // Replace with your actual icon
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()
                    .SetShowActionsInCompactView(0) // Show action buttons compactly
                    .SetMediaSession(null)) // Set to null if you don't have a media session
                .AddAction(
                    _audioService.IsPlaying ? Resource.Drawable.exo_icon_pause : Resource.Drawable.exo_icon_play,
                    _audioService.IsPlaying ? "Pause" : "Play",
                    _playPausePendingIntent
                );

            // Start the service in the foreground
            StartForeground(1, _notificationBuilder.Build()); // Keep service running in the foreground

            return StartCommandResult.Sticky; // Ensure service stays running
        }

        private void UpdateNotification()
        {
            _notificationBuilder
                .SetContentText(_audioService.IsPlaying ? "Playing music..." : "Paused")
                .SetSmallIcon(_audioService.IsPlaying ? Resource.Drawable.exo_icon_play : Resource.Drawable.exo_icon_pause) // Update icon
                .ClearActions() // Clear previous actions
                .AddAction(
                    _audioService.IsPlaying ? Resource.Drawable.exo_icon_pause : Resource.Drawable.exo_icon_play,
                    _audioService.IsPlaying ? "Pause" : "Play",
                    _playPausePendingIntent
                );

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(1, _notificationBuilder.Build());
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _audioService?.Stop(); // Stop and release the player when the service is destroyed
        }
    }
}
