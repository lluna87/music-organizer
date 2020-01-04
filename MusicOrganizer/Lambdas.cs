using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MusicOrganizer
{
    public class Lambdas
    {
        public static Func<string, IEnumerable<DirectoryInfo>> GetDirectories = 
            route => Directory.GetDirectories(route).Select(o => new DirectoryInfo(o)).AsEnumerable();

        public static Func<string, IEnumerable<FileInfo>> FileFinder = dir => Directory.GetFiles(dir)
                .Where(o => o.ToLower().EndsWith(Commons.Constants.FILE_EXTENSION)).Select(o => new FileInfo(o));

        public static Func<string, IEnumerable<FileInfo>> RecursiveFileFinder = dir =>
        {
            List<FileInfo> result = new List<FileInfo>();

            if (Directory.Exists(dir))
            {
                result.AddRange(Lambdas.FileFinder(dir));

                DirectoryInfo[] directories = Lambdas.GetDirectories(dir).ToArray();

                if (directories.Length > 0)
                {
                    result.AddRange(directories.Select(o => o.FullName).SelectMany(RecursiveFileFinder));
                }
            }

            return result.AsEnumerable();
        };
    }

}
