using Android.App;
using Android.Content;
using Android.OS;
using Plugin.Maui.Audio;
using AndroidX.Media;
using AndroidX.Core.App;
using System;
using Android.Content.PM;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Services
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaPlayback)]
    public class MusicPlayerService : Service
    {
        private PendingIntent _playPausePendingIntent;
        private NotificationCompat.Builder _notificationBuilder;

        public override IBinder OnBind(Intent intent)
        {
            return null; 
        }

        public override void OnCreate()
        {
            base.OnCreate();
            
            ServiceLocator.AudioServiceInstance.PlaybackStateChanged += OnPlaybackStateChanged;
        }

        private void OnPlaybackStateChanged(bool isPlaying)
        {
            UpdateNotification();
        }

        public override void OnDestroy()
        {
            ServiceLocator.AudioServiceInstance.PlaybackStateChanged -= OnPlaybackStateChanged; // Unsubscribe
            ServiceLocator.AudioServiceInstance?.Stop();
            base.OnDestroy();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);

            var action = intent.Action;

            var filePath = intent.GetStringExtra("filePath");

            // Handle Play/Pause action from the notification
            if (action == "com.musicplayer.PLAY_PAUSE")
            {
                

                if (ServiceLocator.AudioServiceInstance.IsPlaying == true)
                {
                    Android.Util.Log.Debug("MusicPlayerService", $"Received action: {action} PAUSE");
                    
                    ServiceLocator.AudioServiceInstance.Pause();
                }
                else
                {
                    Android.Util.Log.Debug("MusicPlayerService", $"Received action: {action} RESUME");
                    ServiceLocator.AudioServiceInstance.Resume();
                }

                // Update notification after play/pause action
                UpdateNotification();
            }
            else if (!string.IsNullOrEmpty(filePath)) // Start playing a new file if filePath is passed
            {
                ServiceLocator.AudioServiceInstance.Play(filePath);
                UpdateNotification(); // Update notification after starting music
            }

            // Create the PendingIntent for the play/pause button
            var playPauseIntent = new Intent(this, typeof(MusicPlayerService));
            playPauseIntent.SetAction("com.musicplayer.PLAY_PAUSE");
            _playPausePendingIntent = PendingIntent.GetService(this, 0, playPauseIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable);

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
                .SetContentIntent(_playPausePendingIntent)
                .SetContentText(ServiceLocator.AudioServiceInstance.IsPlaying ?  "Playing music: " + ServiceLocator.AudioServiceInstance.currentSong : "Paused")
                .SetSmallIcon(Resource.Drawable.exo_icon_pause) 
                .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()
                    .SetShowActionsInCompactView(0)
                    .SetMediaSession(null))
                .AddAction(
                    ServiceLocator.AudioServiceInstance.IsPlaying ? Resource.Drawable.exo_icon_pause : Resource.Drawable.exo_icon_play,
                    ServiceLocator.AudioServiceInstance.IsPlaying ? "Pause" : "Play",
                    _playPausePendingIntent
                );

            // Start the service in the foreground
            StartForeground(1, _notificationBuilder.Build()); // Keep service running in the foreground

            return StartCommandResult.Sticky; // Ensure service stays running
        }

        private void UpdateNotification()
        {
            _notificationBuilder
                .SetContentText(ServiceLocator.AudioServiceInstance.IsPlaying ? "Playing music: " + ServiceLocator.AudioServiceInstance.currentSong : "Paused")
                .SetSmallIcon(ServiceLocator.AudioServiceInstance.IsPlaying ? Resource.Drawable.exo_icon_play : Resource.Drawable.exo_icon_pause) // Update icon
                .ClearActions() // Clear previous actions
                .AddAction(
                    ServiceLocator.AudioServiceInstance.IsPlaying ? Resource.Drawable.exo_icon_pause : Resource.Drawable.exo_icon_play,
                    ServiceLocator.AudioServiceInstance.IsPlaying ? "Pause" : "Play",
                    _playPausePendingIntent
                );

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(1, _notificationBuilder.Build());
        }

        
    }
}
