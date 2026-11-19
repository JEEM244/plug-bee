using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
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
        private readonly Func<float[]> _readSpectrum;
        private readonly DispatcherTimer _spectrumTimer;
        private readonly Rectangle[] _spectrumBars = new Rectangle[32];

        public MainWindow(
            Func<TrackSnapshot> readCurrentTrack,
            Action playPause,
            Action previousTrack,
            Action nextTrack,
            Action openLibrary,
            Func<float[]> readSpectrum)
        {
            InitializeComponent();
            _readCurrentTrack = readCurrentTrack;
            _playPause = playPause;
            _previousTrack = previousTrack;
            _nextTrack = nextTrack;
            _openLibrary = openLibrary;
            _readSpectrum = readSpectrum;
            CreateSpectrumBars();
            _spectrumTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _spectrumTimer.Tick += (sender, args) => UpdateSpectrum();
            _spectrumTimer.Start();
            Closed += (sender, args) => _spectrumTimer.Stop();
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
            UpdateArtwork(track.ArtworkData);
        }


        private void CreateSpectrumBars()
        {
            for (int i = 0; i < _spectrumBars.Length; i++)
            {
                Rectangle bar = new Rectangle
                {
                    Fill = new SolidColorBrush(Color.FromRgb(30, 215, 96)),
                    RadiusX = 2,
                    RadiusY = 2,
                    Height = 4
                };
                _spectrumBars[i] = bar;
                SpectrumCanvas.Children.Add(bar);
            }
            SpectrumCanvas.SizeChanged += (sender, args) => UpdateSpectrumLayout();
            UpdateSpectrumLayout();
        }

        private void UpdateSpectrum()
        {
            float[] values = _readSpectrum?.Invoke();
            if (values == null || values.Length == 0)
            {
                foreach (Rectangle bar in _spectrumBars)
                    bar.Height = 3;
                return;
            }

            double width = SpectrumCanvas.ActualWidth;
            double height = SpectrumCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            double slot = width / _spectrumBars.Length;
            for (int i = 0; i < _spectrumBars.Length; i++)
            {
                int sample = Math.Min(values.Length - 1, (int)((double)i / _spectrumBars.Length * values.Length));
                double level = Math.Max(0, Math.Min(1, values[sample]));
                double barHeight = Math.Max(3, level * (height - 4));
                Rectangle bar = _spectrumBars[i];
                bar.Width = Math.Max(2, slot - 3);
                bar.Height = barHeight;
                Canvas.SetLeft(bar, i * slot + 1);
                Canvas.SetTop(bar, height - barHeight);
            }
        }

        private void UpdateSpectrumLayout()
        {
            if (_spectrumBars[0] == null) return;
            double slot = SpectrumCanvas.ActualWidth / _spectrumBars.Length;
            for (int i = 0; i < _spectrumBars.Length; i++)
            {
                _spectrumBars[i].Width = Math.Max(2, slot - 3);
                Canvas.SetLeft(_spectrumBars[i], i * slot + 1);
            }
        }

        private void UpdateArtwork(byte[] artworkData)
        {
            if (artworkData == null || artworkData.Length == 0)
            {
                AlbumCover.Source = null;
                AlbumCover.Visibility = Visibility.Collapsed;
                CoverFallback.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                BitmapImage image = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(artworkData))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                }

                AlbumCover.Source = image;
                AlbumCover.Visibility = Visibility.Visible;
                CoverFallback.Visibility = Visibility.Collapsed;
            }
            catch
            {
                AlbumCover.Source = null;
                AlbumCover.Visibility = Visibility.Collapsed;
                CoverFallback.Visibility = Visibility.Visible;
            }
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
