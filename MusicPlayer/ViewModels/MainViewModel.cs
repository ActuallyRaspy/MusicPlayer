using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicPlayer.Models;
using MusicPlayer.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Views;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Services;
using Microsoft.Maui.Controls;

namespace MusicPlayer.ViewModels
{
    public class MainViewModel : BindableObject
    {

        private AudioService _audioPlayerService = ServiceLocator.AudioServiceInstance;
        private FileService _fileService = new FileService();

        private Song _currentSong;
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

        public Song CurrentSong
        {
            get => _currentSong;
            set
            {
                if (_currentSong != value)
                {
                    _currentSong = value;
                    OnPropertyChanged();
                }
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
        public Command CreatePlaylistCommand { get; }
        

        // ✅ Parameterless constructor for XAML
        public MainViewModel()
        {
            _audioPlayerService = ServiceLocator.AudioServiceInstance;
            Playlists = new ObservableCollection<Playlist>();
            PlayCommand = new Command(Play);
            PauseCommand = new Command(Pause);
            SkipCommand = new Command(Skip);
            AddToPlaylistCommand = new Command(async () => await AddToPlaylist());
            CreatePlaylistCommand = new Command(async () => await CreatePlaylist());

            LoadPlaylistsAsync();
        }

        private async Task LoadPlaylistsAsync()
        {
            var playlists = await _fileService.GetPlaylists();
            Playlists.Clear(); // Clear the existing collection
            foreach (var playlist in playlists)
            {
                Playlists.Add(playlist); // Add new playlists to the collection
            }
                
            CurrentPlaylist = Playlists[0];
        }

        public void Play()
        {
            if (CurrentSong != null && !string.IsNullOrEmpty(CurrentSong.FilePath))
            {
                if (IsPlaying)
                {
                    _audioPlayerService.Resume();

                }
                else
                {
                    _audioPlayerService.Play(CurrentSong.FilePath);
                }
                IsPlaying = true;
            }
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                _audioPlayerService.Pause();
                IsPlaying = false;
            }
        }

        public void Skip()
        {
            var nextSong = CurrentPlaylist?.Songs
                .SkipWhile(s => s != CurrentSong)
                .Skip(1)
                .FirstOrDefault();

            if (nextSong != null)
            {
                CurrentSong = nextSong;
                Play();
            }
        }

        private async Task AddToPlaylist()
        {
            var customAudioType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/mp3", "audio/wav" } },
                { DevicePlatform.iOS, new[] { "public.audio" } },
                { DevicePlatform.WinUI, new[] { ".mp3", ".wav" } },
                { DevicePlatform.MacCatalyst, new[] { "public.audio" } }
            });

            var options = new PickOptions
            {
                PickerTitle = "Select an audio file",
                FileTypes = customAudioType
            };

            var result = await FilePicker.Default.PickAsync(options);

            if (result != null)
            {
                var song = new Song { Title = result.FileName, FilePath = result.FullPath };
                CurrentPlaylist?.Songs.Add(song);
                _fileService.SavePlaylist(CurrentPlaylist);
                OnPropertyChanged(nameof(CurrentPlaylist));
            }
            await LoadPlaylistsAsync();
        }

        private async Task CreatePlaylist()
        {
            // Display a prompt for the user to input the playlist name
            string playlistName = await Application.Current.MainPage.DisplayPromptAsync(
                "New Playlist",
                "Enter a name for your new playlist:",
                accept: "OK",
                cancel: "Cancel",
                placeholder: "Playlist name");

            // Check if the user provided a name or canceled the dialog
            if (!string.IsNullOrEmpty(playlistName))
            {                
                _fileService.CreatePlaylist(playlistName);
                OnPropertyChanged();
            }
            else
            {
                // Handle the case where the user canceled or didn't input anything
                Console.WriteLine("Playlist creation canceled or no name provided.");
            }
            await LoadPlaylistsAsync();
        }
    }
}
