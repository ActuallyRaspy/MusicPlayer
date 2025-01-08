using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Services
{
    public class FileService
    {
     
        string backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "playlist.txt");
            

        public async void SavePlaylist(Playlist playlist)
        {
            
            using (var writer = File.CreateText(backingFile))
            {
                for (int i = 0; i < playlist.Songs.Count(); i++)
                {
                    playlist.Songs[i].Title = playlist.Songs[i].Title.Replace(";", string.Empty);
                    playlist.Songs[i].Artist = playlist.Songs[i].Title.Replace(";", string.Empty);

                    await writer.WriteLineAsync(playlist.Songs[i].Title + ";" + playlist.Songs[i].Artist + ";" + playlist.Songs[i].FilePath);
                }
            }
        }

        public void RemovePlaylist(Playlist playlist)
        {

        }

        public void UpdatePlaylist(Playlist playlist)
        {

        }
    }
}
