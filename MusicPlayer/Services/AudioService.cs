using Plugin.Maui.Audio;

namespace MusicPlayer.Services
{
    public class AudioService
    {
        private IAudioPlayer _audioPlayer;
        private double _pausePosition;
        private string currentFilePath = "";
        public bool IsPlaying => _audioPlayer?.IsPlaying ?? false;
        public double CurrentPosition => _audioPlayer?.CurrentPosition ?? 0;

        public void Play(string filePath)
        {
            if (_audioPlayer == null || currentFilePath != filePath)
            {
                _audioPlayer = AudioManager.Current.CreatePlayer(filePath);
                currentFilePath = filePath;
            }

            if (_pausePosition > 0)
            {
                _audioPlayer.Seek(_pausePosition); // Resume from last position
            }

            _audioPlayer.Play();
        }

        public void Pause()
        {
            if (_audioPlayer?.IsPlaying == true)
            {
                _pausePosition = _audioPlayer.CurrentPosition;
                _audioPlayer.Pause();
            }
        }

        public void Stop()
        {
            _audioPlayer?.Stop();
            _audioPlayer?.Dispose();
            _audioPlayer = null;
        }

        public void Resume()
        {
            if (_audioPlayer != null && _pausePosition > 0)
            {
                _audioPlayer.Seek(_pausePosition);
                _audioPlayer.Play();
            }
        }
    }
}
