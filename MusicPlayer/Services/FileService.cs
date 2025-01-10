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
     
        string backingFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString();
            

        public async void SavePlaylist(Playlist playlist)
        {
            
            using (var writer = File.CreateText(backingFilePath + "/" + playlist.Name))
            {
                for (int i = 0; i < playlist.Songs.Count(); i++)
                {
                    playlist.Songs[i].Title = playlist.Songs[i].Title.Replace(";", string.Empty);
                    playlist.Songs[i].Artist = playlist.Songs[i].Title.Replace(";", string.Empty);

                    await writer.WriteLineAsync(playlist.Songs[i].Title + ";" + playlist.Songs[i].Artist + ";" + playlist.Songs[i].FilePath);
                }
            }
        }

        public async void CreatePlaylist(string playlistName)
        {
            using (var writer = File.CreateText(backingFilePath + "/" + playlistName + ".txt"))
            {
                await writer.WriteLineAsync(backingFilePath + "/" + playlistName + ".txt");
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
