using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using MusicOrganizer.Models;
using System.Text.RegularExpressions;

namespace MusicOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ingrese una ruta de inicio");
            string route = Console.ReadLine();

            Console.WriteLine(route);

            /* If  */
            if (!Directory.Exists(route))
            {
                Console.WriteLine("Directorio no encontrado");
                return;
            };

            DateTime currentTime = DateTime.Now;

            DirectoryInfo baseDirectory = new DirectoryInfo(route);

            string backupRoute = route + "\\" + Commons.Constants.BACKUP_BASE_DIR + 
                "_" + currentTime.ToString("yyMMdd_HHmmss");
            DirectoryInfo backupDir = Directory.Exists(backupRoute) ? new DirectoryInfo(backupRoute) : Directory.CreateDirectory(backupRoute);

            DirectoryInfo[] directories = Lambdas.GetDirectories(route)
                .Where(o => !o.Name.StartsWith(Commons.Constants.BACKUP_BASE_DIR) ).ToArray();

            DirectoryInfo[] subDirectories;
            foreach (DirectoryInfo directory in directories)
            {
                /* Backup subdirectories */
                Directory.Move(directory.FullName, backupDir.FullName + "\\" + directory.Name);
            }

            FileInfo[] fileS = Lambdas.RecursiveFileFinder(backupDir.FullName).ToArray();

            Mp3File[] mp3Files = fileS.Select(o => new Mp3File(o)).ToArray();
            //Id3.Id3Tag[] mp3Tags = mp3Files.Select(o => o.GetTag(Id3.Id3TagFamily.Version2X)).ToArray();

            IGrouping<string, Mp3File>[] byArtists = mp3Files.GroupBy(o => o.Artist).ToArray();

            Console.WriteLine();
                                 
            foreach (IGrouping<string, Mp3File> mp3ByArtist in byArtists)
            {
                if (string.IsNullOrWhiteSpace(mp3ByArtist.Key))
                {
                    /* If there is no artist just write the file in the root */
                    Mp3File[] artistTags = mp3ByArtist.ToArray();
                    foreach (Mp3File mp3 in artistTags)
                    {
                        File.WriteAllBytes(baseDirectory.FullName, mp3.Bytes);
                    }
                }
                else
                {
                    /* Create artist directory */
                    DirectoryInfo artistDirectory = Directory.CreateDirectory(baseDirectory.FullName + "\\" + mp3ByArtist.Key);

                    /* 
                     * No album files 
                     * Write files in the artist root directory
                     */
                    Mp3File[] noAlbumFiles = mp3ByArtist.Where(o => string.IsNullOrWhiteSpace(o.Album)).ToArray();
                    foreach (Mp3File mp3 in noAlbumFiles)
                    {
                        File.WriteAllBytes(artistDirectory.FullName, mp3.Bytes);
                    }

                    /* Album defined files */
                    Mp3File[] albumDefinedFiles = mp3ByArtist.Where(o => !string.IsNullOrWhiteSpace(o.Album)).ToArray();
                    
                    /* Obtain album name, ignoring disc information */
                    const string cdRegex = @"(.+)\((.+\d+)\)";
                    Func<Mp3File, string> albumNameRegEx = o => Regex.Match(o.Album, cdRegex).Groups[1]?.Value.Trim();
                    Func<Mp3File, string> discRegEx = o => Regex.Match(o.Album, cdRegex).Groups[2]?.Value.Trim();

                    /* Files grouped by album */
                    IGrouping<string, Mp3File>[] byAlbum = mp3ByArtist.GroupBy(albumNameRegEx).ToArray();

                    string albumName = string.Empty;
                    foreach (IGrouping<string, Mp3File> mp3ByAlbum in byAlbum)
                    {
                        /* Create album directory with year number */
                        albumName = mp3ByAlbum.Key;

                        int? year = mp3ByAlbum.Min(o => o.Year);

                        string albumDir = artistDirectory.FullName + "\\";
                        albumDir += year.HasValue ? "(" + year.Value + ") " : string.Empty;
                        albumDir += albumName;

                        DirectoryInfo albumDirectory = Directory.CreateDirectory(albumDir);

                        if (!mp3ByAlbum.All(o => !string.IsNullOrWhiteSpace(discRegEx(o))))
                        {
                            /* There is not a clear disc definition */
                            foreach (Mp3File mp3 in mp3ByAlbum)
                            {
                                File.WriteAllBytes(albumDirectory.FullName + "\\" + mp3.FileName, mp3.Bytes);
                            }
                        }
                        else
                        {
                            /* 
                             * All tracks have a disc setted
                             * Group files by disc number
                             */
                            IGrouping<string, Mp3File>[] byDiscNumber = mp3ByAlbum.
                                GroupBy(discRegEx).OrderBy(o => o.Key).ToArray();

                            foreach (IGrouping<string, Mp3File> group in byDiscNumber)
                            {
                                string folderName = group.Key.Replace("(", "").Replace(")", "");
                                string discDir = albumDirectory.FullName + "\\" + folderName;
                                DirectoryInfo discDirectory = Directory.CreateDirectory(discDir);

                                foreach (Mp3File mp3 in group)
                                {
                                    File.WriteAllBytes(discDirectory.FullName + "\\" + mp3.FileName, mp3.Bytes);
                                }
                            }
                        }
                    }
                }
            }



            ///* Load backed up directories */
            //directories = Directory.GetDirectories(backupDir.FullName).Select(o => new DirectoryInfo(o))
            //    .Where(o => o.Name != backupDir.Name).ToArray();

            ///* Find mp3 files */
            //List<FileInfo> files = new List<FileInfo>();



            ///* From root dir */
            //files.AddRange(fileFinder(backupDir.FullName));

            ///* From subdirectories */
            //for

        }
    }
}
