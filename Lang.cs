using System.Collections.Generic;

namespace MkMods
{
    /// <summary>
    /// Simple localization system — EN (default) / RU toggle.
    /// </summary>
    public static class Lang
    {
        public static bool IsRussian { get; set; } = false; // default = English

        public static string Get(string key)
        {
            var dict = IsRussian ? RU : EN;
            return dict.TryGetValue(key, out var val) ? val : key;
        }

        // ─────────────── English ───────────────
        private static readonly Dictionary<string, string> EN = new()
        {
            // Title bar
            ["title"] = "MkMods",

            // Home — game panel
            ["path_not_found"] = "Path not found",

            // Home — system panel
            ["system_header"] = "SYSTEM",
            ["status_header"] = "STATUS",
            ["disk_prefix"] = "Disk C:",
            ["disk_suffix"] = "GB free",
            ["cpu_na"] = "CPU: N/A",
            ["gpu_na"] = "GPU: N/A",
            ["ram_na"] = "RAM: N/A",
            ["disk_na"] = "Disk: N/A",

            // License
            ["license_checking"] = "⏳ Checking...",
            ["license_ok"] = "✅ Steam License",
            ["license_pirate"] = "❌ Pirated copy detected",
            ["license_no_steam"] = "⚠ Steam not running",
            ["license_no_game"] = "⚠ Game not found",
            ["pirate_msg"] = "A pirated copy of the game has been detected.\nMkMods only works with the licensed Steam version.\n\nBuy the game on Steam!",
            ["pirate_title"] = "MkMods — License Error",

            // Mod loader
            ["mscloader_active"] = "MSCLoader: ✅ Active",
            ["mscloader_missing"] = "MSCLoader: ❌ Not found",
            ["mscloader_na"] = "MSCLoader: N/A",
            ["mwc_loader"] = "Loader: MWC built-in",

            // Mod count
            ["mods_count"] = "Mods: {0} .dll ({1} files)",
            ["mods_na"] = "Mods: N/A",

            // Saves
            ["saves_count"] = "Saves: {0} files",
            ["saves_not_found"] = "Saves: Not found",

            // Status
            ["status_ready"] = "Ready",
            ["status_launching"] = "Launching {0} via Steam...",

            // Buttons
            ["btn_play"] = "▶  PLAY",
            ["btn_install_mods"] = "📦  Install Mods",
            ["btn_saves"] = "💾  Saves",
            ["btn_mods_folder"] = "📂  Mods Folder",
            ["btn_download_mscloader"] = "⬇  Download MSCLoader",
            ["btn_download_mods"] = "⬇  Download Mods",
            ["btn_settings"] = "⚙  Settings",
            ["btn_telegram"] = "✈  Telegram",
            ["btn_back"] = "←  Back",

            // Mods page
            ["mods_page_title"] = "INSTALL MODS",
            ["drop_files_here"] = "Drag & drop files / folders here",
            ["or_click_to_browse"] = "or click to browse",
            ["supported_formats"] = ".dll  .zip  .rar  .7z  folders",
            ["btn_install_selected"] = "✔  Install Selected",
            ["dlg_select_mods"] = "Select mods to install",

            // Saves page
            ["saves_page_title"] = "SAVE GAMES",
            ["drop_saves_here"] = "Drag & drop save files here",
            ["btn_install_saves"] = "✔  Install Saves",
            ["btn_open_saves_folder"] = "📂  Open Saves Folder",
            ["dlg_select_saves"] = "Select save files",

            // Settings page
            ["settings_title"] = "SETTINGS",
            ["msc_mods_path"] = "MSC — Mods Folder",
            ["mwc_mods_path"] = "MWC — Mods Folder",
            ["msc_saves_path"] = "MSC — Saves Folder",
            ["mwc_saves_path"] = "MWC — Saves Folder",
            ["btn_save_settings"] = "💾  Save Settings",
            ["dlg_select_folder"] = "Select a folder",

            // Messages
            ["err_mods_not_configured"] = "Mods folder is not configured. Go to Settings.",
            ["err_saves_not_configured"] = "Saves folder is not configured. Go to Settings.",
            ["err_create_folder"] = "Could not create folder:\n{0}",
            ["err_no_files"] = "No files to install. Drag files or click to browse.",
            ["err_no_save_files"] = "No files to install.",
            ["err_mods_folder_missing"] = "Mods folder not found. Set the path in Settings.",
            ["err_saves_folder_missing"] = "Saves folder not found.",
            ["err_launch_failed"] = "Failed to launch the game:\n{0}",
            ["err_file_not_found"] = "{0}: file not found",
            ["msg_error"] = "Error",
            ["msg_info"] = "Information",
            ["msg_installed"] = "Installed: {0}",
            ["msg_errors"] = "\nErrors: {0}",
            ["msg_install_mods_title"] = "Mod Installation",
            ["msg_install_saves_title"] = "Save Installation",
            ["msg_status_mods"] = "Mods installed: {0}, errors: {1}",
            ["msg_status_saves"] = "Saves installed: {0}, errors: {1}",
            ["msg_settings_saved"] = "Settings saved!",
            ["msg_settings_saved_status"] = "Settings saved ✔",
        };

        // ─────────────── Russian ───────────────
        private static readonly Dictionary<string, string> RU = new()
        {
            // Title bar
            ["title"] = "MkMods",

            // Home — game panel
            ["path_not_found"] = "Путь не найден",

            // Home — system panel
            ["system_header"] = "СИСТЕМА",
            ["status_header"] = "СТАТУС",
            ["disk_prefix"] = "Диск C:",
            ["disk_suffix"] = "ГБ свободно",
            ["cpu_na"] = "CPU: Н/Д",
            ["gpu_na"] = "GPU: Н/Д",
            ["ram_na"] = "RAM: Н/Д",
            ["disk_na"] = "Диск: Н/Д",

            // License
            ["license_checking"] = "⏳ Проверка...",
            ["license_ok"] = "✅ Steam Лицензия",
            ["license_pirate"] = "❌ Пиратка обнаружена",
            ["license_no_steam"] = "⚠ Steam не запущен",
            ["license_no_game"] = "⚠ Игра не найдена",
            ["pirate_msg"] = "Обнаружена пиратская копия игры.\nMkMods работает только с лицензионной Steam версией.\n\nКупите игру в Steam!",
            ["pirate_title"] = "MkMods — Ошибка лицензии",

            // Mod loader
            ["mscloader_active"] = "MSCLoader: ✅ Активен",
            ["mscloader_missing"] = "MSCLoader: ❌ Не найден",
            ["mscloader_na"] = "MSCLoader: Н/Д",
            ["mwc_loader"] = "Загрузчик: MWC встроенный",

            // Mod count
            ["mods_count"] = "Модов: {0} .dll ({1} файлов)",
            ["mods_na"] = "Модов: Н/Д",

            // Saves
            ["saves_count"] = "Сейвы: {0} файлов",
            ["saves_not_found"] = "Сейвы: Не найдены",

            // Status
            ["status_ready"] = "Готово",
            ["status_launching"] = "Запуск {0} через Steam...",

            // Buttons
            ["btn_play"] = "▶  ИГРАТЬ",
            ["btn_install_mods"] = "📦  Установить моды",
            ["btn_saves"] = "💾  Сейвы",
            ["btn_mods_folder"] = "📂  Папка Mods",
            ["btn_download_mscloader"] = "⬇  Скачать MSCLoader",
            ["btn_download_mods"] = "⬇  Скачать моды",
            ["btn_settings"] = "⚙  Настройки",
            ["btn_telegram"] = "✈  Telegram",
            ["btn_back"] = "←  Назад",

            // Mods page
            ["mods_page_title"] = "УСТАНОВКА МОДОВ",
            ["drop_files_here"] = "Перетащите файлы / папки сюда",
            ["or_click_to_browse"] = "или нажмите для выбора",
            ["supported_formats"] = ".dll  .zip  .rar  .7z  папки",
            ["btn_install_selected"] = "✔  Установить выбранное",
            ["dlg_select_mods"] = "Выберите моды для установки",

            // Saves page
            ["saves_page_title"] = "СОХРАНЕНИЯ",
            ["drop_saves_here"] = "Перетащите файлы сейвов сюда",
            ["btn_install_saves"] = "✔  Установить сейвы",
            ["btn_open_saves_folder"] = "📂  Открыть папку сейвов",
            ["dlg_select_saves"] = "Выберите файлы сохранений",

            // Settings page
            ["settings_title"] = "НАСТРОЙКИ",
            ["msc_mods_path"] = "MSC — Папка модов",
            ["mwc_mods_path"] = "MWC — Папка модов",
            ["msc_saves_path"] = "MSC — Папка сейвов",
            ["mwc_saves_path"] = "MWC — Папка сейвов",
            ["btn_save_settings"] = "💾  Сохранить настройки",
            ["dlg_select_folder"] = "Выберите папку",

            // Messages
            ["err_mods_not_configured"] = "Папка модов не настроена. Перейдите в Настройки.",
            ["err_saves_not_configured"] = "Папка сейвов не настроена. Перейдите в Настройки.",
            ["err_create_folder"] = "Не удалось создать папку:\n{0}",
            ["err_no_files"] = "Нет файлов для установки. Перетащите файлы или нажмите для выбора.",
            ["err_no_save_files"] = "Нет файлов для установки.",
            ["err_mods_folder_missing"] = "Папка модов не найдена. Укажите путь в настройках.",
            ["err_saves_folder_missing"] = "Папка сейвов не найдена.",
            ["err_launch_failed"] = "Не удалось запустить игру:\n{0}",
            ["err_file_not_found"] = "{0}: файл не найден",
            ["msg_error"] = "Ошибка",
            ["msg_info"] = "Информация",
            ["msg_installed"] = "Установлено: {0}",
            ["msg_errors"] = "\nОшибки: {0}",
            ["msg_install_mods_title"] = "Установка модов",
            ["msg_install_saves_title"] = "Установка сейвов",
            ["msg_status_mods"] = "Установлено модов: {0}, ошибок: {1}",
            ["msg_status_saves"] = "Установлено сейвов: {0}, ошибок: {1}",
            ["msg_settings_saved"] = "Настройки сохранены!",
            ["msg_settings_saved_status"] = "Настройки сохранены ✔",
        };
    }
}
