using CommunityToolkit.Maui.Core.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MusicPlayer.Models;
using System.Threading.Tasks;

namespace MusicPlayer.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private string _currentSong;
        private bool _isPlaying;
        private Playlist _currentPlaylist;

        // A collection of playlists
        public ObservableCollection<Playlist> Playlists { get; set; }
        public Playlist CurrentPlaylist
        {
            get => _currentPlaylist;
            set
            {
                _currentPlaylist = value;
                OnPropertyChanged();
            }
        }

        public string CurrentSong
        {
            get => _currentSong;
            set
            {
                _currentSong = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
            }
        }

        public Command PlayCommand { get; }
        public Command PauseCommand { get; }
        public Command SkipCommand { get; }
        public Command AddToPlaylistCommand { get; }

        public MainViewModel()
        {
            Playlists = new ObservableCollection<Playlist>();
            PlayCommand = new Command(Play);
            PauseCommand = new Command(Pause);
            SkipCommand = new Command(Skip);
            AddToPlaylistCommand = new Command(AddToPlaylist);

            LoadPlaylists();
        }

        private void LoadPlaylists()
        {
            // Load some example playlists
            var playlist1 = new Playlist("Favorites");
            playlist1.Songs.Add(new Song { Title = "Song 1", Artist = "Artist 1", FilePath = "path/to/song1.mp3" });
            playlist1.Songs.Add(new Song { Title = "Song 2", Artist = "Artist 2", FilePath = "path/to/song2.mp3" });

            var playlist2 = new Playlist("Chill Vibes");
            playlist2.Songs.Add(new Song { Title = "Song 3", Artist = "Artist 3", FilePath = "path/to/song3.mp3" });

            Playlists.Add(playlist1);
            Playlists.Add(playlist2);

            CurrentPlaylist = playlist1; // Default to the first playlist
        }

        private void Play()
        {
            if (!string.IsNullOrEmpty(CurrentSong))
            {
                IsPlaying = true;
                // Logic to trigger MediaElement play (view-side binding will handle it)
            }
        }

        private void Pause()
        {
            IsPlaying = false;
            // Logic to trigger MediaElement pause (view-side binding will handle it)
        }

        private void Skip()
        {
            var nextSong = CurrentPlaylist.Songs.SkipWhile(s => s.FilePath != CurrentSong).Skip(1).FirstOrDefault();
            if (nextSong != null)
            {
                CurrentSong = nextSong.FilePath;
                // Logic to trigger MediaElement play the next song (view-side binding will handle it)
            }
        }

        private async void AddToPlaylist() // Make this method async
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
            { DevicePlatform.Android, new[] { "audio/mpeg" } }, // MIME type
                });

            PickOptions options = new PickOptions()
            {
                PickerTitle = "Please select an audio file",
                FileTypes = customFileType
            };

            // Await the result of the file picker
            var pickedSong = await PickAndShow(options);

            // If a song was selected and it's a valid MP3 file
            if (pickedSong != null)
            {
                Song song = new Song { Title = pickedSong.FileName, FilePath = pickedSong.FullPath };
                CurrentPlaylist.Songs.Add(song);
                OnPropertyChanged(nameof(CurrentPlaylist)); // Notify UI about the change
            }
        }

        public async Task<FileResult> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith("mp3", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("wav", StringComparison.OrdinalIgnoreCase))
                    {
                        // Return the result, no need to create a Song here
                        return result;
                    }
                    else
                    {
                        // Optionally, show a message if the file type isn't valid
                        // e.g., ShowMessage("Invalid file type");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors (e.g., user canceled or there was an issue with the file picker)
                Console.WriteLine($"Error picking file: {ex.Message}");
            }

            return null; // Return null if no valid file was picked
        }
    }
}
