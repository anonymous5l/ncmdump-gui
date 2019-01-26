using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NcmDecrypt
{
    [DataContract]
    public class NeteaseCopyrightData
    {
        [DataMember(Name = "musicId")]
        public int MusicId { get; set; }

        [DataMember(Name = "musicName")]
        public string MusicName { get; set; }

        [DataMember(Name = "artist")]
        public List<List<object>> Artist { get; set; }

        [DataMember(Name = "albumId")]
        public int AlbumId { get; set; }

        [DataMember(Name = "album")]
        public string Album { get; set; }

        [DataMember(Name = "albumPicDocId")]
        public string AlbumPicDocId { get; set; }

        [DataMember(Name = "albumPic")]
        public string AlbumPic { get; set; }

        [DataMember(Name = "bitrate")]
        public int Bitrate { get; set; }

        [DataMember(Name = "mp3DocId")]
        public string Mp3DocId { get; set; }

        [DataMember(Name = "duration")]
        public int Duration { get; set; }

        [DataMember(Name = "mvId")]
        public int MvId { get; set; }

        [DataMember(Name = "alias")]
        public List<string> Alias { get; set; }

        // missing `transNames` unknow type usually empty

        [DataMember(Name = "format")]
        public string Format { get; set; }
    }
}
