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

namespace MusicPlayer.ViewModels
{
    public class MainViewModel : BindableObject
    {

        private readonly AudioService _audioPlayerService;
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

        // ✅ Parameterless constructor for XAML
        public MainViewModel()
        {
            Playlists = new ObservableCollection<Playlist>();
            PlayCommand = new Command(Play);
            PauseCommand = new Command(Pause);
            SkipCommand = new Command(Skip);
            AddToPlaylistCommand = new Command(async () => await AddToPlaylist());

            _audioPlayerService = new AudioService();

            LoadPlaylists();
        }

        private void LoadPlaylists()
        {
            var playlist1 = new Playlist("Favorites");
            playlist1.Songs.Add(new Song { Title = "Song 1", Artist = "Artist 1", FilePath = "path/to/song1.mp3" });
            playlist1.Songs.Add(new Song { Title = "Song 2", Artist = "Artist 2", FilePath = "path/to/song2.mp3" });

            var playlist2 = new Playlist("Chill Vibes");
            playlist2.Songs.Add(new Song { Title = "Song 3", Artist = "Artist 3", FilePath = "path/to/song3.mp3" });

            Playlists.Add(playlist1);
            Playlists.Add(playlist2);

            CurrentPlaylist = playlist1;
        }

        private void Play()
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

        private void Pause()
        {
            if (IsPlaying)
            {
                _audioPlayerService.Pause();
                IsPlaying = false;
            }
        }

        private void Skip()
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
                OnPropertyChanged(nameof(CurrentPlaylist));
            }
        }
    }
}
