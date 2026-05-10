// VibeSplash.cs
// Originally authored by: Artzox (https://github.com/artzox/Playnite-Splash-Addon)
// Fork: VibeSplash by EvoShot (https://github.com/EvoShot/Playnite-Splash-Addon-VibeSplash)
// ⚠️ VIBE CODED — written with AI assistance. May contain vibes. Use at your own risk.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace VibeSplash
{
    public class VibeSplashPlugin : GenericPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private VibeSplashSettings _settings;
        private DateTime _gameStartTimestamp;

        // New GUID — distinct from the original Splash Addon
        public override Guid Id { get; } = Guid.Parse("a3f7c812-4d9e-47b1-b563-9e1a2c3d4f50");

        public VibeSplashPlugin(IPlayniteAPI api) : base(api)
        {
            _settings = LoadPluginSettings<VibeSplashSettings>() ?? new VibeSplashSettings();
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return _settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            var settingsView = new VibeSplashSettingsView();
            settingsView.DataContext = _settings;
            return settingsView;
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            SavePluginSettings(_settings);
            base.OnApplicationStopped(args);
        }

        // ─── Mode guard ──────────────────────────────────────────────────────────
        /// <summary>
        /// Returns true when the splash screen should be blocked based on the current
        /// Playnite application mode and the user's Disable settings.
        /// </summary>
        private bool IsSplashBlockedByMode()
        {
            bool isFullscreen = PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen;

            if (isFullscreen && _settings.DisableInFullscreen)
                return true;

            if (!isFullscreen && _settings.DisableInDesktop)
                return true;

            return false;
        }
        // ─────────────────────────────────────────────────────────────────────────

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            _gameStartTimestamp = DateTime.Now;

            if (IsSplashBlockedByMode()) return;

            if (_settings.UseGameStartedTimer)
            {
                // Show splash screen, but let OnGameStarted handle the timer
                ShowSplashScreen(args.Game, 0, false);
            }
            else
            {
                // Show splash screen with its own timer
                ShowSplashScreen(
                    args.Game,
                    _settings.GetDurationForGame(args.Game.Id.ToString(), args.Game.Platforms?.FirstOrDefault()?.Name ?? string.Empty),
                    true
                );
            }
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            if (_settings.UseGameStartedTimer && !IsSplashBlockedByMode())
            {
                TimeSpan elapsed = DateTime.Now - _gameStartTimestamp;
                int remainingDuration = _settings.GetDurationForGame(
                    args.Game.Id.ToString(),
                    args.Game.Platforms?.FirstOrDefault()?.Name ?? string.Empty
                ) - (int)elapsed.TotalSeconds;

                if (remainingDuration > 0)
                {
                    SetCloseTimer(remainingDuration);
                }
                else
                {
                    SetCloseTimer(0);
                }
            }
        }

        // ─── NEW: Show splash screen after a game is closed ──────────────────────
        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (_settings.ShowSplashOnGameClose && !IsSplashBlockedByMode())
            {
                int duration = _settings.GetDurationForGame(
                    args.Game.Id.ToString(),
                    args.Game.Platforms?.FirstOrDefault()?.Name ?? string.Empty
                );

                ShowSplashScreen(args.Game, duration, true);
            }

            base.OnGameStopped(args);
        }
        // ─────────────────────────────────────────────────────────────────────────

        private void SetCloseTimer(int durationInSeconds)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var splashWindow = Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.Title == "VibeSplashScreen");

                if (splashWindow == null) return;

                var closeTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(durationInSeconds)
                };

                closeTimer.Tick += (s, e) =>
                {
                    closeTimer.Stop();
                    FadeAndClose(splashWindow);
                };

                if (durationInSeconds > 0)
                {
                    closeTimer.Start();
                }
                else
                {
                    FadeAndClose(splashWindow);
                }
            });
        }

        private void FadeAndClose(Window window)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1)
            };

            Storyboard.SetTarget(fadeOut, window);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(Window.OpacityProperty));

            var fadeStoryboard = new Storyboard();
            fadeStoryboard.Children.Add(fadeOut);
            fadeStoryboard.Completed += (s2, e2) =>
            {
                try { window.Close(); }
                catch { }
            };
            fadeStoryboard.Begin();
        }

        private void ShowSplashScreen(Game game, int durationInSeconds, bool startTimerImmediately)
        {
            if (_settings.ExcludedGameIds.Any(id => id.Trim() == game.Id.ToString()))
                return;

            string platformName = game.Platforms?.FirstOrDefault()?.Name ?? string.Empty;
            int duration = _settings.GetDurationForGame(game.Id.ToString(), platformName);

            if (duration <= 0)
            {
                duration = _settings.SplashScreenDuration;
                if (duration <= 0) duration = 1;
            }

            // Resolve background image
            string bgImagePath = game.BackgroundImage;
            string resolvedBgPath = null;

            if (!string.IsNullOrEmpty(bgImagePath))
            {
                try
                {
                    if (bgImagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedBgPath = bgImagePath;
                    }
                    else
                    {
                        string playniteDataDir = API.Instance.Paths.IsPortable
                            ? API.Instance.Paths.ApplicationPath
                            : API.Instance.Paths.ConfigurationPath;

                        resolvedBgPath = Path.IsPathRooted(bgImagePath)
                            ? bgImagePath
                            : Path.Combine(playniteDataDir, "library", "files", bgImagePath);

                        if (!File.Exists(resolvedBgPath))
                            resolvedBgPath = null;
                    }
                }
                catch { }
            }

            // Resolve logo (Extra Metadata)
            string logoPath = null;
            try
            {
                string extraMetadataDir = Path.Combine(
                    Playnite.SDK.API.Instance.Paths.ConfigurationPath,
                    "ExtraMetadata", "Games", game.Id.ToString(), "Logo.png"
                );
                if (File.Exists(extraMetadataDir))
                    logoPath = extraMetadataDir;
            }
            catch { }

            // Build window
            var splashWindow = new Window
            {
                Title = "VibeSplashScreen",
                WindowStyle = WindowStyle.None,
                WindowState = WindowState.Maximized,
                Topmost = true,
                Background = Brushes.Black,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Opacity = 0
            };

            Image bgImage = null;
            if (!string.IsNullOrEmpty(resolvedBgPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(resolvedBgPath, UriKind.RelativeOrAbsolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bgImage = new Image { Source = bitmap, Stretch = Stretch.UniformToFill };
                }
                catch { }
            }

            if (bgImage == null)
                bgImage = new Image { Stretch = Stretch.UniformToFill };

            Image logoImage = null;
            if (!string.IsNullOrEmpty(logoPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(logoPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    logoImage = new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.Uniform,
                        Width = _settings.LogoSize,
                        Height = double.NaN,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(20, 0, 0, 20)
                    };
                }
                catch { }
            }

            var grid = new Grid();
            grid.Children.Add(bgImage);
            if (logoImage != null)
                grid.Children.Add(logoImage);

            splashWindow.Content = grid;

            // Fade-in animation
            var storyboard = new Storyboard();
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1)
            };
            Storyboard.SetTarget(fadeIn, splashWindow);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Window.OpacityProperty));
            storyboard.Children.Add(fadeIn);

            if (startTimerImmediately)
            {
                var closeTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(duration)
                };

                closeTimer.Tick += (s, e) =>
                {
                    closeTimer.Stop();
                    FadeAndClose(splashWindow);
                };

                splashWindow.Loaded += (s, e) =>
                {
                    storyboard.Begin();
                    closeTimer.Start();
                };
            }
            else
            {
                splashWindow.Loaded += (s, e) =>
                {
                    storyboard.Begin();
                };
            }

            try
            {
                splashWindow.Show();
                splashWindow.Activate();
            }
            catch { }
        }
    }
}
