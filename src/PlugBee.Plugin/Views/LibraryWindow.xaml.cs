using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PlugBee.Models;

namespace PlugBee.Views
{
    public partial class LibraryWindow : Window
    {
        private readonly Func<string, LibraryTrack[]> _searchLibrary;
        private readonly Action<string> _playTrack;

        public ObservableCollection<LibraryTrack> Tracks { get; } =
            new ObservableCollection<LibraryTrack>();

        public LibraryWindow(Func<string, LibraryTrack[]> searchLibrary, Action<string> playTrack)
        {
            InitializeComponent();
            _searchLibrary = searchLibrary;
            _playTrack = playTrack;
            DataContext = this;
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            await SearchAsync();
        }

        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await SearchAsync();
        }

        private async Task SearchAsync()
        {
            string term = SearchBox.Text == null ? string.Empty : SearchBox.Text.Trim();
            if (term.Length < 2)
            {
                StatusText.Text = "Escribe al menos dos caracteres para buscar.";
                return;
            }

            StatusText.Text = "Buscando en MusicBee…";
            Tracks.Clear();

            try
            {
                LibraryTrack[] results = await Task.Run(() => _searchLibrary(term));
                foreach (LibraryTrack track in results ?? new LibraryTrack[0])
                    Tracks.Add(track);

                StatusText.Text = Tracks.Count == 0
                    ? "No se encontraron canciones."
                    : Tracks.Count + " resultado(s). Doble clic para reproducir.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "No se pudo consultar la biblioteca: " + ex.Message;
            }
        }

        private void PlaySelected_Click(object sender, RoutedEventArgs e)
        {
            PlaySelectedTrack();
        }

        private void TracksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySelectedTrack();
        }

        private void TracksGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // La selección se utiliza al pulsar Reproducir o hacer doble clic.
        }

        private void PlaySelectedTrack()
        {
            LibraryTrack selected = TracksGrid.SelectedItem as LibraryTrack;
            if (selected == null || string.IsNullOrWhiteSpace(selected.FileUrl))
                return;

            _playTrack?.Invoke(selected.FileUrl);
            StatusText.Text = "Enviada a reproducción: " + selected.Title;
        }
    }
}
