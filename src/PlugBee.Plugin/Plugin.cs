using System;
using System.Linq;
using System.Windows;
using MusicBeePlugin;
using PlugBee.Models;
using PlugBee.Views;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface _musicBee;
        private PluginInfo _pluginInfo;
        private MainWindow _window;
        private LibraryWindow _libraryWindow;
        private bool _apiReady;
        private string _lastArtworkUrl;
        private byte[] _cachedArtwork;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            _musicBee = new MusicBeeApiInterface();
            _musicBee.Initialise(apiInterfacePtr);
            _apiReady = true;

            _pluginInfo = new PluginInfo
            {
                PluginInfoVersion = PluginInfoVersion,
                Type = PluginType.General,
                Name = "Plug-Bee",
                Description = "Una experiencia musical mejorada para MusicBee.",
                Author = "JEEM244",
                TargetApplication = string.Empty,
                VersionMajor = 0,
                VersionMinor = 1,
                Revision = 0,
                MinInterfaceVersion = MinInterfaceVersion,
                MinApiRevision = MinApiRevision,
                ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents,
                ConfigurationPanelHeight = 0
            };

            return _pluginInfo;
        }

        // MusicBee llama a Configure desde las preferencias del plugin.
        // Por ahora es el punto de entrada para abrir la interfaz independiente.
        public bool Configure(IntPtr panelHandle)
        {
            if (_window == null)
            {
                _window = new MainWindow(ReadCurrentTrack, PlayPause, PreviousTrack, NextTrack, OpenLibrary, ReadSpectrum);
                _window.Closed += (sender, args) => _window = null;
                _window.Show();
            }
            else
            {
                if (_window.WindowState == System.Windows.WindowState.Minimized)
                    _window.WindowState = System.Windows.WindowState.Normal;

                _window.Activate();
                _window.RefreshTrackInfo();
            }

            return false;
        }

        public void SaveSettings()
        {
            // La configuración persistente se añadirá cuando incorporemos preferencias.
        }

        public void Close(PluginCloseReason reason)
        {
            if (_libraryWindow != null)
            {
                _libraryWindow.Close();
                _libraryWindow = null;
            }

            if (_window != null)
            {
                _window.Close();
                _window = null;
            }
        }

        public void Uninstall()
        {
            // No hay datos propios que eliminar en esta primera versión.
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            if (_window == null || !_window.IsLoaded)
                return;

            _window.Dispatcher.BeginInvoke(new Action(_window.RefreshTrackInfo));
        }

        private TrackSnapshot ReadCurrentTrack()
        {
            if (!_apiReady)
                return TrackSnapshot.Empty;

            return new TrackSnapshot
            {
                Title = SafeTag(MetaDataType.TrackTitle),
                Artist = SafeTag(MetaDataType.Artist),
                Album = SafeTag(MetaDataType.Album),
                IsPlaying = _musicBee.Player_GetPlayState() == PlayState.Playing,
                ArtworkData = ReadCurrentArtwork()
            };
        }

        private byte[] ReadCurrentArtwork()
        {
            try
            {
                string fileUrl = _musicBee.NowPlaying_GetFileUrl();
                if (string.IsNullOrWhiteSpace(fileUrl))
                {
                    _lastArtworkUrl = null;
                    _cachedArtwork = null;
                    return null;
                }

                if (string.Equals(fileUrl, _lastArtworkUrl, StringComparison.OrdinalIgnoreCase))
                    return _cachedArtwork;

                _lastArtworkUrl = fileUrl;
                PictureLocations locations;
                string pictureUrl;
                byte[] imageData;
                bool found = _musicBee.Library_GetArtworkEx(
                    fileUrl, 0, true, out locations, out pictureUrl, out imageData);

                _cachedArtwork = found && imageData != null && imageData.Length > 0
                    ? imageData
                    : null;

                return _cachedArtwork;
            }
            catch
            {
                return null;
            }
        }


        private float[] ReadSpectrum()
        {
            if (!_apiReady || _musicBee.Player_GetPlayState() != PlayState.Playing)
                return null;

            try
            {
                float[] spectrum = new float[256];
                int count = _musicBee.NowPlaying_GetSpectrumData(spectrum);
                if (count <= 0) return null;
                if (count == spectrum.Length) return spectrum;

                float[] result = new float[Math.Min(count, spectrum.Length)];
                Array.Copy(spectrum, result, result.Length);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private string SafeTag(MetaDataType tag)
        {
            try
            {
                return _musicBee.NowPlaying_GetFileTag(tag) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void OpenLibrary()
        {
            if (_libraryWindow == null)
            {
                _libraryWindow = new LibraryWindow(SearchLibrary, PlayLibraryTrack);
                _libraryWindow.Owner = _window;
                _libraryWindow.Closed += (sender, args) => _libraryWindow = null;
                _libraryWindow.Show();
            }
            else
            {
                _libraryWindow.Activate();
            }
        }

        private PlugBee.Models.LibraryTrack[] SearchLibrary(string searchText)
        {
            if (!_apiReady || string.IsNullOrWhiteSpace(searchText))
                return new PlugBee.Models.LibraryTrack[0];

            string query = BuildLibraryQuery(searchText);
            string[] files;
            bool found = _musicBee.Library_QueryFilesEx(query, out files);
            if (!found || files == null)
                return new PlugBee.Models.LibraryTrack[0];

            return files
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Take(250)
                .Select(path => new PlugBee.Models.LibraryTrack
                {
                    FileUrl = path,
                    Title = ReadLibraryTag(path, MetaDataType.TrackTitle),
                    Artist = ReadLibraryTag(path, MetaDataType.Artist),
                    Album = ReadLibraryTag(path, MetaDataType.Album)
                })
                .ToArray();
        }

        private static string BuildLibraryQuery(string searchText)
        {
            var conditions = new System.Xml.Linq.XElement("Conditions",
                new System.Xml.Linq.XAttribute("CombineMethod", "Any"),
                new System.Xml.Linq.XElement("Condition",
                    new System.Xml.Linq.XAttribute("Field", "Title"),
                    new System.Xml.Linq.XAttribute("Comparison", "Contains"),
                    new System.Xml.Linq.XAttribute("Value", searchText)),
                new System.Xml.Linq.XElement("Condition",
                    new System.Xml.Linq.XAttribute("Field", "ArtistPeople"),
                    new System.Xml.Linq.XAttribute("Comparison", "Contains"),
                    new System.Xml.Linq.XAttribute("Value", searchText)),
                new System.Xml.Linq.XElement("Condition",
                    new System.Xml.Linq.XAttribute("Field", "Album"),
                    new System.Xml.Linq.XAttribute("Comparison", "Contains"),
                    new System.Xml.Linq.XAttribute("Value", searchText)));

            return new System.Xml.Linq.XElement("SmartPlaylist",
                new System.Xml.Linq.XElement("Source",
                    new System.Xml.Linq.XAttribute("Type", "1"),
                    conditions)).ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
        }

        private string ReadLibraryTag(string fileUrl, MetaDataType tag)
        {
            try
            {
                return _musicBee.Library_GetFileTag(fileUrl, tag) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void PlayLibraryTrack(string fileUrl)
        {
            if (_apiReady && !string.IsNullOrWhiteSpace(fileUrl))
                _musicBee.NowPlayingList_PlayNow(fileUrl);
        }

        private void PlayPause()
        {
            _musicBee.Player_PlayPause();
        }

        private void PreviousTrack()
        {
            _musicBee.Player_PlayPreviousTrack();
        }

        private void NextTrack()
        {
            _musicBee.Player_PlayNextTrack();
        }
    }
}
