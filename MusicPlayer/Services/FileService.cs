using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MusicPlayer.Services
{
    public class FileService
    {
     
        string targetPath = FileSystem.Current.AppDataDirectory;

        public async void SavePlaylist(Playlist playlist)
        {

            using (var writer = File.CreateText(Path.Combine(targetPath, playlist.Name)))
            {
                for (int i = 0; i < playlist.Songs.Count(); i++)
                {
                    playlist.Songs[i].Title = playlist.Songs[i].Title.Replace(";", string.Empty);

                    await writer.WriteLineAsync(playlist.Songs[i].Title + ";" + playlist.Songs[i].FilePath);
                }
            }
        }

        public async void CreatePlaylist(string playlistName)
        {

            string filePath = Path.Combine(targetPath, playlistName + ".txt");

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            try
            {
                // Create and write to the file
                using (FileStream outputStream = File.Create(filePath)) // Creates or overwrites the file
                using (StreamWriter writer = new StreamWriter(outputStream))
                {
                    await writer.WriteLineAsync("This is a new playlist file: " + playlistName);
                }

                Console.WriteLine($"Playlist '{playlistName}' created successfully at: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating playlist: {ex.Message}");
            }
        }

        public async Task<ObservableCollection<Playlist>> GetPlaylists()
        {
            ObservableCollection<Playlist> allPlaylists = new ObservableCollection<Playlist>();

            string[] filePaths = Directory.GetFiles(targetPath, "*.txt", SearchOption.TopDirectoryOnly);
            Console.WriteLine($"Found {filePaths.Length} files in {targetPath}.");

            for (int i = 0; i < filePaths.Length; i++)
            {
                try
                {
                    Console.WriteLine($"Processing file: {filePaths[i]}");

                    using (FileStream InputStream = File.OpenRead(filePaths[i]))
                    {
                        string fileName = Path.GetFileName(filePaths[i]);
                        Playlist playlist = new Playlist(filePaths[i]) { Name = fileName };

                        using (StreamReader reader = new StreamReader(InputStream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                Console.WriteLine($"Read line: {line}");
                                string[] splitSong = line.Split(";");
                                if (splitSong.Length >= 2)
                                {
                                    Song song = new Song { Title = splitSong[0], FilePath = splitSong[1] };
                                    playlist.Songs.Add(song);
                                }
                                else
                                {   
                                    Console.WriteLine($"Skipping invalid line: {line}");
                                }
                            }
                        }
                        allPlaylists.Add(playlist);
                        Console.WriteLine($"Added playlist: {playlist.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {filePaths[i]}: {ex.Message}");
                }
            }

            Console.WriteLine($"Total playlists created: {allPlaylists.Count}");
            return allPlaylists;
        }



        public void DeletePlaylist(Playlist playlist)
        {
            string filePath = Path.Combine(targetPath, playlist.Name);

            try
            {
                
                File.Delete(filePath + ".txt") ; // Delets file
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error creating playlist: {ex.Message}");
            }

        }
    }
}
