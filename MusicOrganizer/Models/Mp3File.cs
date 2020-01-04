using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicOrganizer.Models
{
    class Mp3File
    {
        private string _FileName;
        private byte[] FileBytes;
        private Id3.Mp3 Mp3;
        private Id3.Id3Tag Tag;

        public Mp3File(FileInfo info)
        {
            this._FileName = info.Name;
            this.FileBytes = System.IO.File.ReadAllBytes(info.FullName);
            this.Mp3 = new Id3.Mp3(this.FileBytes);

            /* Por que no carga los tags de Tu Locura de cerati */
            /* El flag esta en true, pero no hay datos */
            if (this.Mp3.HasTags)
            {
                Id3.Id3Tag[] tags = this.Mp3.GetAllTags().ToArray();
                this.Tag = tags.FirstOrDefault(o => o.Version == Id3.Id3Version.V23) ??
                    tags.FirstOrDefault(o => o.Version == Id3.Id3Version.V1X);
            }
        }

        public string Artist => this.Tag?.Artists.ToString();
        public string Album => this.Tag?.Album.ToString();
        public string Title => this.Tag?.Title.ToString();
        public string Track => this.Tag?.Track;
        public int? Year => this.Tag?.Year.Value;
        public byte[] Bytes => this.FileBytes;
        public string FileName => this._FileName;
    }
}
