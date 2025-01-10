using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using MusicPlayer.Services;
using AndroidX.Core.Content;

namespace MusicPlayer
{
    [Activity(Label = "MusicPlayer", Theme = "@style/Maui.SplashTheme", ResizeableActivity = true, MainLauncher = true, LaunchMode = LaunchMode.SingleTask, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Intent intent = new Intent(this, typeof(MusicPlayerService));
            intent.PutExtra("filePath", ServiceLocator.AudioServiceInstance.CurrentFilePath); // Set your audio file path here
            ContextCompat.StartForegroundService(this, intent);
        }
    }
}
