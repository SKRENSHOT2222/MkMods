using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;

namespace MkMods
{
    public partial class MainWindow : Window
    {
        // ═══════════════════════════════════════════════
        //  State
        // ═══════════════════════════════════════════════
        private bool _isMWC = false;  // false = MSC, true = MWC

        private string _mscModsPath = "";
        private string _mwcModsPath = "";
        private string _mscSavesPath = "";
        private string _mwcSavesPath = "";

        private readonly List<string> _pendingModFiles = new();
        private readonly List<string> _pendingSaveFiles = new();

        // Steam App IDs
        private const int MSC_APPID = 516750;
        private const int MWC_APPID = 4164420;

        // Registry key
        private const string REG_KEY = @"Software\MkMods";

        // ═══════════════════════════════════════════════
        //  Constructor
        // ═══════════════════════════════════════════════
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Auto-detect paths
            AutoDetectPaths();

            // Load saved paths from registry (overrides auto-detect if set)
            LoadPathsFromRegistry();

            // Apply language to all controls
            ApplyLanguage();

            // Check Steam license
            CheckSteamLicense();

            // Gather system info
            GatherSystemInfo();

            // Update UI
            RefreshUI();
        }

        // ═══════════════════════════════════════════════
        //  Window Chrome
        // ═══════════════════════════════════════════════
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        private void BtnClose_Click(object sender, RoutedEventArgs e)
            => Close();

        // ═══════════════════════════════════════════════
        //  Page Navigation with Fade
        // ═══════════════════════════════════════════════
        private void ShowPage(FrameworkElement page)
        {
            var pages = new FrameworkElement[] { PageHome, PageMods, PageSaves, PageSettings };

            foreach (var p in pages)
            {
                if (p == page)
                {
                    p.Visibility = Visibility.Visible;
                    p.Opacity = 0;
                    var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    p.BeginAnimation(OpacityProperty, fadeIn);
                }
                else
                {
                    p.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => ShowPage(PageHome);

        // ═══════════════════════════════════════════════
        //  Game Toggle
        // ═══════════════════════════════════════════════
        private void GameToggle_Click(object sender, MouseButtonEventArgs e)
        {
            _isMWC = !_isMWC;

            if (_isMWC)
            {
                ToggleMSC.Background = Brushes.Transparent;
                ToggleMWC.Background = new SolidColorBrush(Color.FromRgb(0x7C, 0x3A, 0xED));
                // Update MSC label color to gray
                ((TextBlock)ToggleMSC.Child).Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF));
                ((TextBlock)ToggleMWC.Child).Foreground = Brushes.White;
            }
            else
            {
                ToggleMSC.Background = new SolidColorBrush(Color.FromRgb(0x7C, 0x3A, 0xED));
                ToggleMWC.Background = Brushes.Transparent;
                ((TextBlock)ToggleMSC.Child).Foreground = Brushes.White;
                ((TextBlock)ToggleMWC.Child).Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF));
            }

            RefreshUI();
        }

        // ═══════════════════════════════════════════════
        //  Language Toggle & Localization
        // ═══════════════════════════════════════════════
        private void LangToggle_Click(object sender, MouseButtonEventArgs e)
        {
            Lang.IsRussian = !Lang.IsRussian;
            ApplyLanguage();

            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(REG_KEY);
                key.SetValue("Language", Lang.IsRussian ? "ru" : "en");
            }
            catch { }

            CheckSteamLicense();
            GatherSystemInfo();
            RefreshUI();
        }

        private void ApplyLanguage()
        {
            if (Lang.IsRussian)
            {
                ToggleEN.Background = Brushes.Transparent;
                ToggleRU.Background = new SolidColorBrush(Color.FromRgb(0x7C, 0x3A, 0xED));
                ((TextBlock)ToggleEN.Child).Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF));
                ((TextBlock)ToggleRU.Child).Foreground = Brushes.White;
            }
            else
            {
                ToggleEN.Background = new SolidColorBrush(Color.FromRgb(0x7C, 0x3A, 0xED));
                ToggleRU.Background = Brushes.Transparent;
                ((TextBlock)ToggleEN.Child).Foreground = Brushes.White;
                ((TextBlock)ToggleRU.Child).Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF));
            }

            // Headers & Buttons
            TxtSystemHeader.Text = Lang.Get("system_header");
            TxtStatusHeader.Text = Lang.Get("status_header");

            BtnPlay.Content = Lang.Get("btn_play");
            BtnInstallMods.Content = Lang.Get("btn_install_mods");
            BtnSavesBtn.Content = Lang.Get("btn_saves");
            BtnModsFolder.Content = Lang.Get("btn_mods_folder");
            BtnDownloadLoader.Content = Lang.Get("btn_download_mscloader");
            BtnDownloadMods.Content = Lang.Get("btn_download_mods");
            BtnSettingsBtn.Content = Lang.Get("btn_settings");
            BtnTelegramBtn.Content = Lang.Get("btn_telegram");

            // Mods Page
            BtnBackMods.Content = Lang.Get("btn_back");
            TxtModsTitle.Text = Lang.Get("mods_page_title");
            TxtDropModsTitle.Text = Lang.Get("drop_files_here");
            TxtDropModsSub.Text = Lang.Get("or_click_to_browse");
            TxtDropModsFormats.Text = Lang.Get("supported_formats");
            BtnDoInstallMods.Content = Lang.Get("btn_install_selected");

            // Saves Page
            BtnBackSaves.Content = Lang.Get("btn_back");
            TxtSavesTitle.Text = Lang.Get("saves_page_title");
            TxtDropSavesTitle.Text = Lang.Get("drop_saves_here");
            TxtDropSavesSub.Text = Lang.Get("or_click_to_browse");
            BtnDoInstallSaves.Content = Lang.Get("btn_install_saves");
            BtnOpenSavesFolder.Content = Lang.Get("btn_open_saves_folder");

            // Settings Page
            BtnBackSettings.Content = Lang.Get("btn_back");
            TxtSettingsTitle.Text = Lang.Get("settings_title");
            TxtLblMSCMods.Text = Lang.Get("msc_mods_path");
            TxtLblMWCMods.Text = Lang.Get("mwc_mods_path");
            TxtLblMSCSaves.Text = Lang.Get("msc_saves_path");
            TxtLblMWCSaves.Text = Lang.Get("mwc_saves_path");
            BtnSaveSettings.Content = Lang.Get("btn_save_settings");
        }

        // ═══════════════════════════════════════════════
        //  Auto-detect Paths
        // ═══════════════════════════════════════════════
        private void AutoDetectPaths()
        {
            // Known Steam library locations to check
            var searchRoots = new List<string>
            {
                @"C:\Program Files (x86)\Steam\steamapps\common",
                @"C:\Program Files\Steam\steamapps\common",
                @"D:\SteamLibrary\steamapps\common",
                @"E:\SteamLibrary\steamapps\common",
                @"F:\SteamLibrary\steamapps\common",
                @"G:\SteamLibrary\steamapps\common",
            };

            // Also try reading Steam's libraryfolders.vdf
            try
            {
                var vdfPath = @"C:\Program Files (x86)\Steam\steamapps\libraryfolders.vdf";
                if (File.Exists(vdfPath))
                {
                    var lines = File.ReadAllLines(vdfPath);
                    foreach (var line in lines)
                    {
                        if (line.Contains("\"path\""))
                        {
                            var parts = line.Split('"');
                            if (parts.Length >= 4)
                            {
                                var libPath = parts[3].Replace(@"\\", @"\");
                                var commonDir = Path.Combine(libPath, "steamapps", "common");
                                if (!searchRoots.Contains(commonDir))
                                    searchRoots.Add(commonDir);
                            }
                        }
                    }
                }
            }
            catch { }

            // Find MSC
            foreach (var root in searchRoots)
            {
                var mscDir = Path.Combine(root, "My Summer Car");
                if (Directory.Exists(mscDir))
                {
                    var modsDir = Path.Combine(mscDir, "Mods");
                    if (!Directory.Exists(modsDir))
                        Directory.CreateDirectory(modsDir);
                    _mscModsPath = modsDir;
                    break;
                }
            }

            // Find MWC
            foreach (var root in searchRoots)
            {
                var mwcDir = Path.Combine(root, "My Winter Car");
                if (Directory.Exists(mwcDir))
                {
                    var modsDir = Path.Combine(mwcDir, "Mods");
                    if (!Directory.Exists(modsDir))
                        Directory.CreateDirectory(modsDir);
                    _mwcModsPath = modsDir;
                    break;
                }
            }

            // Find Saves
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // LocalLow is one level up from LocalApplicationData ... \AppData\Local -> \AppData\LocalLow
            var localLow = Path.Combine(Directory.GetParent(appData).FullName, "LocalLow");

            var mscSaves = Path.Combine(localLow, "Amistech", "My Summer Car");
            if (Directory.Exists(mscSaves))
                _mscSavesPath = mscSaves;

            var mwcSaves = Path.Combine(localLow, "Amistech", "My Winter Car");
            if (Directory.Exists(mwcSaves))
                _mwcSavesPath = mwcSaves;
        }

        // ═══════════════════════════════════════════════
        //  Registry Load / Save
        // ═══════════════════════════════════════════════
        private void LoadPathsFromRegistry()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REG_KEY);
                if (key == null) return;

                var v = key.GetValue("MSCMods") as string;
                if (!string.IsNullOrEmpty(v) && Directory.Exists(v)) _mscModsPath = v;

                v = key.GetValue("MWCMods") as string;
                if (!string.IsNullOrEmpty(v) && Directory.Exists(v)) _mwcModsPath = v;

                v = key.GetValue("MSCSaves") as string;
                if (!string.IsNullOrEmpty(v) && Directory.Exists(v)) _mscSavesPath = v;

                v = key.GetValue("MWCSaves") as string;
                if (!string.IsNullOrEmpty(v) && Directory.Exists(v)) _mwcSavesPath = v;

                var lang = key.GetValue("Language") as string;
                if (!string.IsNullOrEmpty(lang))
                {
                    Lang.IsRussian = (lang == "ru");
                }
                else
                {
                    Lang.IsRussian = false; // default is English
                }
            }
            catch { }
        }

        private void SavePathsToRegistry()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(REG_KEY);
                key.SetValue("MSCMods", _mscModsPath ?? "");
                key.SetValue("MWCMods", _mwcModsPath ?? "");
                key.SetValue("MSCSaves", _mscSavesPath ?? "");
                key.SetValue("MWCSaves", _mwcSavesPath ?? "");
                key.SetValue("Language", Lang.IsRussian ? "ru" : "en");
            }
            catch { }
        }

        // ═══════════════════════════════════════════════
        //  Steam License Check
        // ═══════════════════════════════════════════════
        private void CheckSteamLicense()
        {
            bool steamRunning = Process.GetProcessesByName("steam").Length > 0;
            bool hasMSC = !string.IsNullOrEmpty(_mscModsPath);
            bool hasMWC = !string.IsNullOrEmpty(_mwcModsPath);

            // Check for piracy indicators
            bool hasCrack = false;
            if (hasMSC)
            {
                var gameRoot = Directory.GetParent(_mscModsPath)?.FullName ?? "";
                hasCrack = CheckForCrack(gameRoot);
            }
            if (hasMWC && !hasCrack)
            {
                var gameRoot = Directory.GetParent(_mwcModsPath)?.FullName ?? "";
                hasCrack = CheckForCrack(gameRoot);
            }

            if (hasCrack)
            {
                TxtLicense.Text = Lang.Get("license_pirate");
                TxtLicense.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                MessageBox.Show(
                    Lang.Get("pirate_msg"),
                    Lang.Get("pirate_title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (steamRunning && (hasMSC || hasMWC))
            {
                TxtLicense.Text = Lang.Get("license_ok");
                TxtLicense.Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99));
            }
            else if (!steamRunning)
            {
                TxtLicense.Text = Lang.Get("license_no_steam");
                TxtLicense.Foreground = new SolidColorBrush(Color.FromRgb(0xFC, 0xD3, 0x4D));
            }
            else
            {
                TxtLicense.Text = Lang.Get("license_no_game");
                TxtLicense.Foreground = new SolidColorBrush(Color.FromRgb(0xFC, 0xD3, 0x4D));
            }
        }

        private bool CheckForCrack(string gameRoot)
        {
            if (string.IsNullOrEmpty(gameRoot) || !Directory.Exists(gameRoot)) return false;

            var crackIndicators = new[]
            {
                "steam_api_o.dll", "steam_emu.ini", "codex.cfg", "3dmgame.dll",
                "SmartSteamEmu.ini", "valve.ini", "hlm.ini", "cream_api.ini",
                "GoldbergSteamEmuSettings", "ALI213.ini", "Razor1911.nfo",
                "SKIDROW.nfo", "RELOADED.nfo", "PROPHET.nfo"
            };

            foreach (var indicator in crackIndicators)
            {
                var path = Path.Combine(gameRoot, indicator);
                if (File.Exists(path) || Directory.Exists(path))
                    return true;
            }
            return false;
        }

        // ═══════════════════════════════════════════════
        //  System Info
        // ═══════════════════════════════════════════════
        private void GatherSystemInfo()
        {
            try
            {
                // CPU
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        TxtCPU.Text = "CPU: " + obj["Name"]?.ToString()?.Trim();
                        break;
                    }
                }

                // GPU
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(name) && !name.Contains("Microsoft"))
                        {
                            TxtGPU.Text = "GPU: " + name;
                            break;
                        }
                    }
                }

                // RAM
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var bytes = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                        TxtRAM.Text = $"RAM: {bytes / (1024 * 1024 * 1024)} GB";
                        break;
                    }
                }

                // Disk
                var drive = new DriveInfo("C");
                TxtDisk.Text = $"{Lang.Get("disk_prefix")} {drive.AvailableFreeSpace / (1024 * 1024 * 1024)} {Lang.Get("disk_suffix")}";
            }
            catch
            {
                TxtCPU.Text = Lang.Get("cpu_na");
                TxtGPU.Text = Lang.Get("gpu_na");
                TxtRAM.Text = Lang.Get("ram_na");
                TxtDisk.Text = Lang.Get("disk_na");
            }
        }

        // ═══════════════════════════════════════════════
        //  Refresh UI
        // ═══════════════════════════════════════════════
        private void RefreshUI()
        {
            var modsPath = _isMWC ? _mwcModsPath : _mscModsPath;
            var savesPath = _isMWC ? _mwcSavesPath : _mscSavesPath;

            TxtActiveGame.Text = _isMWC ? "MY WINTER CAR" : "MY SUMMER CAR";
            TxtGamePath.Text = string.IsNullOrEmpty(modsPath) ? Lang.Get("path_not_found") : modsPath;

            // MSCLoader check (MSC only)
            if (!_isMWC && !string.IsNullOrEmpty(_mscModsPath))
            {
                var gameRoot = Directory.GetParent(_mscModsPath)?.FullName ?? "";
                var hasLoader = File.Exists(Path.Combine(gameRoot, "mysummercar_Data", "Managed", "MSCLoader.dll"))
                    || File.Exists(Path.Combine(gameRoot, "doorstop_config.ini"));
                TxtModLoader.Text = hasLoader ? Lang.Get("mscloader_active") : Lang.Get("mscloader_missing");
                TxtModLoader.Foreground = hasLoader
                    ? new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99))
                    : new SolidColorBrush(Color.FromRgb(0xFC, 0xD3, 0x4D));
            }
            else
            {
                TxtModLoader.Text = _isMWC ? Lang.Get("mwc_loader") : Lang.Get("mscloader_na");
                TxtModLoader.Foreground = new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB));
            }

            // Mod count
            if (!string.IsNullOrEmpty(modsPath) && Directory.Exists(modsPath))
            {
                var dllCount = Directory.GetFiles(modsPath, "*.dll", SearchOption.AllDirectories).Length;
                var totalFiles = Directory.GetFiles(modsPath, "*", SearchOption.AllDirectories).Length;
                TxtModCount.Text = string.Format(Lang.Get("mods_count"), dllCount, totalFiles);
            }
            else
            {
                TxtModCount.Text = Lang.Get("mods_na");
            }

            // Saves
            if (!string.IsNullOrEmpty(savesPath) && Directory.Exists(savesPath))
            {
                var saveFiles = Directory.GetFiles(savesPath).Length;
                TxtSavesStatus.Text = string.Format(Lang.Get("saves_count"), saveFiles);
            }
            else
            {
                TxtSavesStatus.Text = Lang.Get("saves_not_found");
            }

            // Settings textboxes
            TxtMSCModsPath.Text = _mscModsPath;
            TxtMWCModsPath.Text = _mwcModsPath;
            TxtMSCSavesPath.Text = _mscSavesPath;
            TxtMWCSavesPath.Text = _mwcSavesPath;
        }

        // ═══════════════════════════════════════════════
        //  Button Handlers — Home
        // ═══════════════════════════════════════════════
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            var appId = _isMWC ? MWC_APPID : MSC_APPID;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"steam://rungameid/{appId}",
                    UseShellExecute = true
                });
                TxtStatus.Text = string.Format(Lang.Get("status_launching"), _isMWC ? "MWC" : "MSC");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.Get("err_launch_failed"), ex.Message), Lang.Get("msg_error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnInstallMods_Click(object sender, RoutedEventArgs e)
        {
            _pendingModFiles.Clear();
            ModFileList.Items.Clear();
            ModFileList.Visibility = Visibility.Collapsed;
            ShowPage(PageMods);
        }

        private void BtnSaves_Click(object sender, RoutedEventArgs e)
        {
            _pendingSaveFiles.Clear();
            SaveFileList.Items.Clear();
            SaveFileList.Visibility = Visibility.Collapsed;
            ShowPage(PageSaves);
        }

        private void BtnOpenModsFolder_Click(object sender, RoutedEventArgs e)
        {
            var path = _isMWC ? _mwcModsPath : _mscModsPath;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            else
            {
                MessageBox.Show(Lang.Get("err_mods_folder_missing"), Lang.Get("msg_error"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDownloadMSCLoader_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/piotrulos/MSCModLoader/releases",
                UseShellExecute = true
            });
        }

        private void BtnDownloadMods_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.nexusmods.com/mysummercar/mods/",
                UseShellExecute = true
            });
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(PageSettings);
        }

        private void BtnTelegram_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://t.me/skrensho7",
                UseShellExecute = true
            });
        }

        // ═══════════════════════════════════════════════
        //  Mods Drag & Drop / Install
        // ═══════════════════════════════════════════════
        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e) { }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddModFiles(files);
        }

        private void DropZone_Click(object sender, MouseButtonEventArgs e)
        {
            // Don't open dialog if clicking on the file list
            if (e.OriginalSource is System.Windows.Controls.TextBlock 
                || ModFileList.IsMouseOver)
                return;

            // Don't open dialog if files are already loaded
            if (_pendingModFiles.Count > 0)
                return;

            e.Handled = true;

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = Lang.Get("dlg_select_mods"),
                Filter = "All supported|*.dll;*.zip;*.rar;*.7z|DLL|*.dll|Archives|*.zip;*.rar;*.7z|All files|*.*",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                AddModFiles(dlg.FileNames);
            }
        }

        private void AddModFiles(string[] paths)
        {
            foreach (var path in paths)
            {
                if (!_pendingModFiles.Contains(path))
                {
                    _pendingModFiles.Add(path);
                    var name = Path.GetFileName(path);
                    var isDir = Directory.Exists(path);
                    var label = isDir ? $"📁 {name}" : $"📄 {name}";
                    ModFileList.Items.Add(label);
                }
            }

            if (_pendingModFiles.Count > 0)
                ModFileList.Visibility = Visibility.Visible;
        }

        private void BtnDoInstallMods_Click(object sender, RoutedEventArgs e)
        {
            var targetPath = _isMWC ? _mwcModsPath : _mscModsPath;
            if (string.IsNullOrEmpty(targetPath))
            {
                MessageBox.Show(Lang.Get("err_mods_not_configured"), Lang.Get("msg_error"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create target directory if it doesn't exist
            try { Directory.CreateDirectory(targetPath); } catch { }

            if (!Directory.Exists(targetPath))
            {
                MessageBox.Show(string.Format(Lang.Get("err_create_folder"), targetPath), Lang.Get("msg_error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_pendingModFiles.Count == 0)
            {
                MessageBox.Show(Lang.Get("err_no_files"), Lang.Get("msg_info"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int installed = 0;
            int errors = 0;
            var errorDetails = new List<string>();

            // Work on a copy of the list to avoid collection modification issues
            var filesToInstall = new List<string>(_pendingModFiles);

            foreach (var filePath in filesToInstall)
            {
                try
                {
                    if (Directory.Exists(filePath))
                    {
                        var dirName = Path.GetFileName(filePath);
                        if (string.IsNullOrEmpty(dirName)) dirName = "mod_folder";
                        var destDir = Path.Combine(targetPath, dirName);
                        CopyDirectory(filePath, destDir);
                        installed++;
                    }
                    else if (File.Exists(filePath))
                    {
                        var ext = Path.GetExtension(filePath).ToLowerInvariant();
                        if (ext == ".zip" || ext == ".rar" || ext == ".7z")
                        {
                            ExtractArchive(filePath, targetPath);
                            installed++;
                        }
                        else
                        {
                            var destFile = Path.Combine(targetPath, Path.GetFileName(filePath));
                            File.Copy(filePath, destFile, overwrite: true);
                            installed++;
                        }
                    }
                    else
                    {
                        errorDetails.Add(string.Format(Lang.Get("err_file_not_found"), Path.GetFileName(filePath)));
                        errors++;
                    }
                }
                catch (Exception ex)
                {
                    errorDetails.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                    errors++;
                }
            }

            _pendingModFiles.Clear();
            ModFileList.Items.Clear();
            ModFileList.Visibility = Visibility.Collapsed;

            try { RefreshUI(); } catch { }

            var msg = string.Format(Lang.Get("msg_installed"), installed);
            if (errors > 0)
            {
                msg += string.Format(Lang.Get("msg_errors"), errors);
                if (errorDetails.Count > 0)
                    msg += "\n\n" + string.Join("\n", errorDetails.Take(5));
            }
            TxtStatus.Text = string.Format(Lang.Get("msg_status_mods"), installed, errors);

            MessageBox.Show(msg, Lang.Get("msg_install_mods_title"), MessageBoxButton.OK,
                errors > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            ShowPage(PageHome);
        }

        // ═══════════════════════════════════════════════
        //  Saves Drag & Drop / Install
        // ═══════════════════════════════════════════════
        private void DropZoneSaves_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AddSaveFiles(files);
        }

        private void DropZoneSaves_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.TextBlock
                || SaveFileList.IsMouseOver)
                return;

            if (_pendingSaveFiles.Count > 0)
                return;

            e.Handled = true;

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = Lang.Get("dlg_select_saves"),
                Filter = "All files|*.*",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                AddSaveFiles(dlg.FileNames);
            }
        }

        private void AddSaveFiles(string[] paths)
        {
            foreach (var path in paths)
            {
                if (!_pendingSaveFiles.Contains(path))
                {
                    _pendingSaveFiles.Add(path);
                    SaveFileList.Items.Add($"📄 {Path.GetFileName(path)}");
                }
            }

            if (_pendingSaveFiles.Count > 0)
                SaveFileList.Visibility = Visibility.Visible;
        }

        private void BtnDoInstallSaves_Click(object sender, RoutedEventArgs e)
        {
            var targetPath = _isMWC ? _mwcSavesPath : _mscSavesPath;
            if (string.IsNullOrEmpty(targetPath))
            {
                MessageBox.Show(Lang.Get("err_saves_not_configured"), Lang.Get("msg_error"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try { Directory.CreateDirectory(targetPath); } catch { }

            if (!Directory.Exists(targetPath))
            {
                MessageBox.Show(string.Format(Lang.Get("err_create_folder"), targetPath), Lang.Get("msg_error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_pendingSaveFiles.Count == 0)
            {
                MessageBox.Show(Lang.Get("err_no_save_files"), Lang.Get("msg_info"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int installed = 0;
            int errors = 0;
            var errorDetails = new List<string>();

            var filesToInstall = new List<string>(_pendingSaveFiles);

            foreach (var filePath in filesToInstall)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        var ext = Path.GetExtension(filePath).ToLowerInvariant();
                        if (ext == ".zip" || ext == ".rar" || ext == ".7z")
                        {
                            ExtractArchive(filePath, targetPath);
                        }
                        else
                        {
                            File.Copy(filePath, Path.Combine(targetPath, Path.GetFileName(filePath)), true);
                        }
                        installed++;
                    }
                    else
                    {
                        errorDetails.Add(string.Format(Lang.Get("err_file_not_found"), Path.GetFileName(filePath)));
                        errors++;
                    }
                }
                catch (Exception ex)
                {
                    errorDetails.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                    errors++;
                }
            }

            _pendingSaveFiles.Clear();
            SaveFileList.Items.Clear();
            SaveFileList.Visibility = Visibility.Collapsed;

            try { RefreshUI(); } catch { }

            var msg = string.Format(Lang.Get("msg_installed"), installed);
            if (errors > 0)
            {
                msg += string.Format(Lang.Get("msg_errors"), errors);
                if (errorDetails.Count > 0)
                    msg += "\n\n" + string.Join("\n", errorDetails.Take(5));
            }
            TxtStatus.Text = string.Format(Lang.Get("msg_status_saves"), installed, errors);

            MessageBox.Show(msg, Lang.Get("msg_install_saves_title"), MessageBoxButton.OK,
                errors > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            ShowPage(PageHome);
        }

        private void BtnOpenSavesFolder_Click(object sender, RoutedEventArgs e)
        {
            var path = _isMWC ? _mwcSavesPath : _mscSavesPath;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            else
            {
                MessageBox.Show(Lang.Get("err_saves_folder_missing"), Lang.Get("msg_error"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ═══════════════════════════════════════════════
        //  Settings
        // ═══════════════════════════════════════════════
        private void BtnBrowseMSCMods_Click(object sender, RoutedEventArgs e)
            => BrowseFolder(TxtMSCModsPath);

        private void BtnBrowseMWCMods_Click(object sender, RoutedEventArgs e)
            => BrowseFolder(TxtMWCModsPath);

        private void BtnBrowseMSCSaves_Click(object sender, RoutedEventArgs e)
            => BrowseFolder(TxtMSCSavesPath);

        private void BtnBrowseMWCSaves_Click(object sender, RoutedEventArgs e)
            => BrowseFolder(TxtMWCSavesPath);

        private void BrowseFolder(TextBox target)
        {
            // Use OpenFileDialog with folder picker hack, or just use FolderBrowserDialog via WinForms interop
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = Lang.Get("dlg_select_folder"),
                ShowNewFolderButton = true
            };

            if (!string.IsNullOrEmpty(target.Text) && Directory.Exists(target.Text))
                dlg.SelectedPath = target.Text;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                target.Text = dlg.SelectedPath;
            }
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _mscModsPath = TxtMSCModsPath.Text.Trim();
            _mwcModsPath = TxtMWCModsPath.Text.Trim();
            _mscSavesPath = TxtMSCSavesPath.Text.Trim();
            _mwcSavesPath = TxtMWCSavesPath.Text.Trim();

            SavePathsToRegistry();
            RefreshUI();

            TxtStatus.Text = Lang.Get("msg_settings_saved_status");
            MessageBox.Show(Lang.Get("msg_settings_saved"), "MkMods", MessageBoxButton.OK, MessageBoxImage.Information);
            ShowPage(PageHome);
        }

        // ═══════════════════════════════════════════════
        //  Archive Extraction (zip, rar, 7z)
        // ═══════════════════════════════════════════════
        private void ExtractArchive(string archivePath, string targetDir)
        {
            using var archive = SharpCompress.Archives.ArchiveFactory.Open(archivePath);
            foreach (var entry in archive.Entries)
            {
                try
                {
                    if (entry.IsDirectory) continue;

                    // entry.Key can be null in some archives
                    var key = entry.Key;
                    if (string.IsNullOrWhiteSpace(key)) continue;

                    // Sanitize path separators
                    key = key.Replace('/', Path.DirectorySeparatorChar)
                             .Replace('\\', Path.DirectorySeparatorChar);

                    // Remove leading separators
                    key = key.TrimStart(Path.DirectorySeparatorChar);
                    if (string.IsNullOrWhiteSpace(key)) continue;

                    // Remove invalid chars from filename
                    var fileName = Path.GetFileName(key);
                    var invalidChars = Path.GetInvalidFileNameChars();
                    foreach (var c in invalidChars)
                        fileName = fileName.Replace(c, '_');

                    // Build final path
                    var subDir = Path.GetDirectoryName(key) ?? "";
                    var destPath = Path.Combine(targetDir, subDir, fileName);
                    var destDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    using var entryStream = entry.OpenEntryStream();
                    using var fileStream = File.Create(destPath);
                    entryStream.CopyTo(fileStream);
                }
                catch
                {
                    // Skip individual entries that fail, don't crash entire extraction
                    continue;
                }
            }
        }

        // ═══════════════════════════════════════════════
        //  Copy Directory Recursively
        // ═══════════════════════════════════════════════
        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
            }
        }
    }
}
