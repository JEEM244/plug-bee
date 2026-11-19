using System;
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

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            _musicBee = new MusicBeeApiInterface();
            _musicBee.Initialise(apiInterfacePtr);

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
                _window = new MainWindow(ReadCurrentTrack, PlayPause, PreviousTrack, NextTrack);
                _window.Closed += (sender, args) => _window = null;
                _window.Show();
            }
            else
            {
                if (_window.WindowState == WindowState.Minimized)
                    _window.WindowState = WindowState.Normal;

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
            if (_musicBee == null)
                return TrackSnapshot.Empty;

            return new TrackSnapshot
            {
                Title = SafeTag(MetaDataType.TrackTitle),
                Artist = SafeTag(MetaDataType.Artist),
                Album = SafeTag(MetaDataType.Album),
                IsPlaying = _musicBee.Player_GetPlayState() == PlayState.Playing
            };
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

        private void PlayPause()
        {
            _musicBee?.Player_PlayPause();
        }

        private void PreviousTrack()
        {
            _musicBee?.Player_PlayPreviousTrack();
        }

        private void NextTrack()
        {
            _musicBee?.Player_PlayNextTrack();
        }
    }
}
