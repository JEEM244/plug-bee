namespace PlugBee.Models
{
    public sealed class TrackSnapshot
    {
        public static readonly TrackSnapshot Empty = new TrackSnapshot
        {
            Title = "Nada se está reproduciendo",
            Artist = "Selecciona una canción en MusicBee",
            Album = string.Empty,
            IsPlaying = false
        };

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public bool IsPlaying { get; set; }
    }
}
