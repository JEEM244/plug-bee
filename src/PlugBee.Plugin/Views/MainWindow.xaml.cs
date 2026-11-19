using System;
using System.Windows;
using PlugBee.Models;

namespace PlugBee.Views
{
    public partial class MainWindow : Window
    {
        private readonly Func<TrackSnapshot> _readCurrentTrack;
        private readonly Action _playPause;
        private readonly Action _previousTrack;
        private readonly Action _nextTrack;
        private readonly Action _openLibrary;

        public MainWindow(
            Func<TrackSnapshot> readCurrentTrack,
            Action playPause,
            Action previousTrack,
            Action nextTrack,
            Action openLibrary)
        {
            InitializeComponent();
            _readCurrentTrack = readCurrentTrack;
            _playPause = playPause;
            _previousTrack = previousTrack;
            _nextTrack = nextTrack;
            _openLibrary = openLibrary;
            RefreshTrackInfo();
        }

        public void RefreshTrackInfo()
        {
            TrackSnapshot track = _readCurrentTrack?.Invoke() ?? TrackSnapshot.Empty;
            string title = string.IsNullOrWhiteSpace(track.Title) ? "Sin título" : track.Title;
            string artist = string.IsNullOrWhiteSpace(track.Artist) ? "Artista desconocido" : track.Artist;

            SongTitle.Text = title;
            SongArtist.Text = artist;
            SongAlbum.Text = track.Album;
            PlayerTitle.Text = title;
            PlayerArtist.Text = artist;
            PlayPauseButton.Content = track.IsPlaying ? "Ⅱ" : "▶";
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            _playPause?.Invoke();
            RefreshTrackInfo();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            _previousTrack?.Invoke();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _nextTrack?.Invoke();
        }

        private void Library_Click(object sender, RoutedEventArgs e)
        {
            _openLibrary?.Invoke();
        }
    }
}
