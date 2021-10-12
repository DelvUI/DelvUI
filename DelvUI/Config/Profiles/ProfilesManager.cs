using Dalamud.Interface;
using Dalamud.Logging;
using DelvUI.Config.Attributes;
using DelvUI.Config.Tree;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DelvUI.Config.Profiles
{
    public class ProfilesManager
    {
        #region Singleton
        public readonly SectionNode ProfilesNode;

        private ProfilesManager()
        {
            // fake nodes
            ProfilesNode = new SectionNode();
            ProfilesNode.Name = "Profiles";

            NestedSubSectionNode subSectionNode = new NestedSubSectionNode();
            subSectionNode.Name = "General";
            subSectionNode.Depth = 0;

            ProfilesConfigPageNode configPageNode = new ProfilesConfigPageNode();

            subSectionNode.Add(configPageNode);
            ProfilesNode.Add(subSectionNode);

            ConfigurationManager.Instance.AddExtraSectionNode(ProfilesNode);

            // default profile
            if (!Profiles.ContainsKey(DefaultProfileName))
            {
                var defaultProfile = new Profile(DefaultProfileName);
                Profiles.Add(DefaultProfileName, defaultProfile);
            }

            // make sure default profile file is created the first time this runs
            if (!File.Exists(CurrentProfilePath()))
            {
                SaveCurrentProfile();
            }
        }

        public static void Initialize()
        {
            try
            {
                string jsonString = File.ReadAllText(JsonPath);
                ProfilesManager? instance = JsonConvert.DeserializeObject<ProfilesManager>(jsonString);
                if (instance != null)
                {
                    Instance = instance;
                }
            }
            catch
            {
                Instance = new ProfilesManager();
            }
        }

        public static ProfilesManager Instance { get; private set; } = null!;

        ~ProfilesManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Instance = null!;
        }
        #endregion

        private string _currentProfileName = "Default";
        public string CurrentProfileName
        {
            get => _currentProfileName;
            set
            {
                if (_currentProfileName == value)
                {
                    return;
                }

                _currentProfileName = value;

                if (_currentProfileName == null || _currentProfileName.Length == 0)
                {
                    _currentProfileName = DefaultProfileName;
                }

                _selectedProfileIndex = Math.Max(0, Profiles.Keys.IndexOf(_currentProfileName));
            }
        }

        [JsonIgnore] private static string ProfilesPath => Path.Combine(ConfigurationManager.Instance.ConfigDirectory, "Profiles");
        [JsonIgnore] private static string JsonPath => Path.Combine(ProfilesPath, "Profiles.json");

        [JsonIgnore] private readonly string DefaultProfileName = "Default";
        [JsonIgnore] private string _newProfileName = "";
        [JsonIgnore] private int _copyFromIndex = 0;
        [JsonIgnore] private int _selectedProfileIndex = 0;
        [JsonIgnore] private string? _errorMessage = null;
        [JsonIgnore] private string? _deletingProfileName = null;
        [JsonIgnore] private string? _resetingProfileName = null;
        [JsonIgnore] private string? _renamingProfileName = null;

        public SortedList<string, Profile> Profiles = new SortedList<string, Profile>();

        public Profile CurrentProfile()
        {
            if (_currentProfileName == null || _currentProfileName.Length == 0)
            {
                _currentProfileName = DefaultProfileName;
            }

            return Profiles[_currentProfileName];
        }

        public void SaveCurrentProfile()
        {
            if (ConfigurationManager.Instance == null)
            {
                return;
            }

            try
            {
                Save();
                SaveCurrentProfile(ConfigurationManager.Instance.ExportCurrentConfigs());
            }
            catch (Exception e)
            {
                PluginLog.Error("Error saving profile: " + e.Message);
            }
        }

        public void SaveCurrentProfile(string? exportString)
        {
            if (exportString == null)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(ProfilesPath);

                File.WriteAllText(CurrentProfilePath(), exportString);
            }
            catch (Exception e)
            {
                PluginLog.Error("Error saving profile: " + e.Message);
            }
        }

        public bool LoadCurrentProfile()
        {
            try
            {
                var importString = File.ReadAllText(CurrentProfilePath());
                return ConfigurationManager.Instance.ImportProfile(importString);
            }
            catch (Exception e)
            {
                PluginLog.Error("Error loading profile: " + e.Message);
            }

            return false;
        }

        public void UpdateCurrentProfile()
        {
            var player = Plugin.ClientState.LocalPlayer;
            if (player == null)
            {
                return;
            }

            uint jobId = player.ClassJob.Id;
            Profile currentProfile = CurrentProfile();
            JobRoles role = JobsHelper.RoleForJob(jobId);
            int index = JobsHelper.JobsByRole[role].IndexOf(jobId);

            if (index < 0)
            {
                return;
            }

            // current profile is enabled for this job, do nothing
            if (currentProfile.AutoSwitchEnabled && currentProfile.AutoSwitchData.Map[role][index])
            {
                return;
            }

            // find a profile that is enabled for this job
            foreach (Profile profile in Profiles.Values)
            {
                if (!profile.AutoSwitchEnabled || profile == currentProfile)
                {
                    continue;
                }

                // found a valid profile, switch to it
                if (profile.AutoSwitchData.Map[role][index])
                {
                    SwitchToProfile(profile.Name);
                    return;
                }
            }
        }

        private string? SwitchToProfile(string profile, bool save = true)
        {
            // save if needed before switching
            if (save)
            {
                ConfigurationManager.Instance.SaveConfigurations();
            }

            string oldProfile = _currentProfileName;
            _currentProfileName = profile;

            if (!LoadCurrentProfile())
            {
                _currentProfileName = oldProfile;
                return "Couldn't load profile \"" + profile + "\"!";
            }

            _selectedProfileIndex = Math.Max(0, Profiles.IndexOfKey(profile));

            try
            {
                Save();
            }
            catch (Exception e)
            {
                PluginLog.Error("Error saving profile: " + e.Message);
                return "Couldn't load profile \"" + profile + "\"!";
            }

            return null;
        }

        private string CurrentProfilePath()
        {
            return Path.Combine(ProfilesPath, _currentProfileName + ".delvui");
        }

        private string? CloneProfile(string profileName, string newProfileName)
        {
            var srcPath = Path.Combine(ProfilesPath, profileName + ".delvui");
            var dstPath = Path.Combine(ProfilesPath, newProfileName + ".delvui");

            return CloneProfile(profileName, srcPath, newProfileName, dstPath);
        }

        private string? CloneProfile(string profileName, string srcPath, string newProfileName, string dstPath)
        {
            if (newProfileName.Length == 0)
            {
                return null;
            }

            if (Profiles.Keys.Contains(newProfileName))
            {
                return "A profile with the name \"" + newProfileName + "\" already exists!";
            }

            try
            {
                if (!File.Exists(srcPath))
                {
                    return "Couldn't find profile \"" + profileName + "\"!";
                }

                if (File.Exists(dstPath))
                {
                    return "A profile with the name \"" + newProfileName + "\" already exists!";
                }

                File.Copy(srcPath, dstPath);
                var newProfile = new Profile(newProfileName);
                Profiles.Add(newProfileName, newProfile);

                Save();
            }
            catch (Exception e)
            {
                PluginLog.Error("Error cloning profile: " + e.Message);
                return "Error trying to clone profile \"" + profileName + "\"!";
            }

            return null;
        }

        private string? RenameCurrentProfile(string newProfileName)
        {
            if (_currentProfileName == newProfileName || newProfileName.Length == 0)
            {
                return null;
            }

            if (Profiles.ContainsKey(newProfileName))
            {
                return "A profile with the name \"" + newProfileName + "\" already exists!";
            }

            var srcPath = Path.Combine(ProfilesPath, _currentProfileName + ".delvui");
            var dstPath = Path.Combine(ProfilesPath, newProfileName + ".delvui");

            try
            {

                if (File.Exists(dstPath))
                {
                    return "A profile with the name \"" + newProfileName + "\" already exists!";
                }

                File.Move(srcPath, dstPath);

                Profile profile = Profiles[_currentProfileName];
                profile.Name = newProfileName;

                Profiles.Remove(_currentProfileName);
                Profiles.Add(newProfileName, profile);

                _currentProfileName = newProfileName;

                Save();
            }
            catch (Exception e)
            {
                PluginLog.Error("Error renaming profile: " + e.Message);
                return "Error trying to rename profile \"" + _currentProfileName + "\"!";
            }

            return null;
        }

        private string? Import(string newProfileName, string importString)
        {
            if (newProfileName.Length == 0)
            {
                return null;
            }

            if (Profiles.Keys.Contains(newProfileName))
            {
                return "A profile with the name \"" + newProfileName + "\" already exists!";
            }

            var dstPath = Path.Combine(ProfilesPath, newProfileName + ".delvui");

            try
            {
                if (File.Exists(dstPath))
                {
                    return "A profile with the name \"" + newProfileName + "\" already exists!";
                }

                File.WriteAllText(dstPath, importString);

                var newProfile = new Profile(newProfileName);
                Profiles.Add(newProfileName, newProfile);

                string? errorMessage = SwitchToProfile(newProfileName, false);

                if (errorMessage != null)
                {
                    Profiles.Remove(newProfileName);
                    File.Delete(dstPath);
                    Save();

                    return errorMessage;
                }
            }
            catch (Exception e)
            {
                PluginLog.Error("Error importing profile: " + e.Message);
                return "Error trying to import profile \"" + newProfileName + "\"!";
            }

            return null;
        }

        private string? ImportFromClipboard(string newProfileName)
        {
            string importString = ImGui.GetClipboardText();
            if (importString.Length == 0)
            {
                return "Invalid import string!";
            }

            return Import(newProfileName, importString);
        }

        private string? ImportFromFile(string newProfileName)
        {
            if (newProfileName.Length == 0)
            {
                return null;
            }

            DialogResult result = DialogResult.Cancel;
            OpenFileDialog? picker;

            try
            {
                picker = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = "DelvUI Profile (*.delvui)|*.delvui",
                    CheckFileExists = true,
                    Title = "Select a DelvUI Profile to import",
                };

                result = picker.ShowDialog();
            }
            catch (Exception e)
            {
                PluginLog.Error("Error with file picker: " + e.Message);
                return "Error trying to open file picker!";
            }

            if (picker == null || result != DialogResult.OK)
            {
                return null;
            }

            try
            {
                var importString = File.ReadAllText(picker.FileName);
                return Import(newProfileName, importString);
            }
            catch (Exception e)
            {
                PluginLog.Error("Error reading import file: " + e.Message);
                return "Error reading the file!";
            }
        }

        private string? DeleteProfile(string profileName)
        {
            if (!Profiles.ContainsKey(profileName))
            {
                return "Couldn't find profile \"" + profileName + "\"!";
            }

            var path = Path.Combine(ProfilesPath, profileName + ".delvui");

            try
            {
                if (!File.Exists(path))
                {
                    return "Couldn't find profile \"" + profileName + "\"!";
                }

                File.Delete(path);
                Profiles.Remove(profileName);

                Save();

                if (_currentProfileName == profileName)
                {
                    return SwitchToProfile(DefaultProfileName, false);
                }
            }
            catch (Exception e)
            {
                PluginLog.Error("Error deleting profile: " + e.Message);
                return "Error trying to delete profile \"" + profileName + "\"!";
            }

            return null;
        }

        private void Save()
        {
            string jsonString = JsonConvert.SerializeObject(
                this,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.Objects
                }
            );

            Directory.CreateDirectory(ProfilesPath);
            File.WriteAllText(JsonPath, jsonString);
        }

        public bool Draw(ref bool changed)
        {
            string[] profiles = Profiles.Keys.ToArray();

            if (ImGui.BeginChild("Profiles", new Vector2(800, 600), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (Profiles.Count == 0)
                {
                    ImGuiHelper.Tab();
                    ImGui.Text("Profiles not found in \"%appdata%/Roaming/XIVLauncher/pluginConfigs/DelvUI/Profiles/\"");
                    return false;
                }

                ImGuiHelper.NewLineAndTab();
                if (ImGui.Combo("Active Profile", ref _selectedProfileIndex, profiles, profiles.Length, 10))
                {
                    var newProfileName = profiles[_selectedProfileIndex];

                    if (_currentProfileName != newProfileName)
                    {
                        _errorMessage = SwitchToProfile(newProfileName);
                    }
                }

                // reset
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.SameLine();
                if (ImGui.Button("\uf2f9", new Vector2(0, 0)))
                {
                    _resetingProfileName = _currentProfileName;
                }
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Reset"); }

                // rename
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.SameLine();
                if (_currentProfileName != DefaultProfileName && ImGui.Button(FontAwesomeIcon.Pen.ToIconString()))
                {
                    _renamingProfileName = _currentProfileName;
                }
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Rename"); }

                // share
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.SameLine();
                if (ImGui.Button(FontAwesomeIcon.ShareSquare.ToIconString()))
                {
                    string? exportString = ConfigurationManager.Instance.ExportCurrentConfigs();
                    if (exportString != null)
                    {
                        ImGui.SetClipboardText(exportString);
                        ImGui.OpenPopup("export_succes_popup");
                    }
                }
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Export"); }

                // export success popup
                if (ImGui.BeginPopup("export_succes_popup"))
                {
                    ImGui.Text("Profile export string copied to clipboard!");
                    ImGui.EndPopup();
                }

                // delete
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.SameLine();
                if (_currentProfileName != DefaultProfileName && ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                {
                    _deletingProfileName = _currentProfileName;
                }
                ImGui.PopFont();
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Delete"); }

                ImGuiHelper.NewLineAndTab();
                DrawAutoSwitchSettings(ref changed);

                ImGuiHelper.DrawSeparator(1, 1);
                ImGuiHelper.Tab();
                ImGui.Text("Create a new profile:");

                ImGuiHelper.NewLineAndTab();
                ImGui.PushItemWidth(408);
                ImGui.InputText("Profile Name", ref _newProfileName, 200);

                ImGuiHelper.NewLineAndTab();
                ImGui.PushItemWidth(200);
                ImGui.Combo("", ref _copyFromIndex, profiles, profiles.Length, 10);

                ImGui.SameLine();
                if (ImGui.Button("Copy", new Vector2(200, 0)))
                {
                    _newProfileName = _newProfileName.Trim();
                    if (_newProfileName.Length == 0)
                    {
                        ImGui.OpenPopup("import_error_popup");
                    }
                    else
                    {
                        _errorMessage = CloneProfile(profiles[_copyFromIndex], _newProfileName);

                        if (_errorMessage == null)
                        {
                            _errorMessage = SwitchToProfile(_newProfileName);
                            _newProfileName = "";
                        }
                    }
                }

                ImGuiHelper.NewLineAndTab();
                if (ImGui.Button("Import From Clipboard", new Vector2(200, 0)))
                {
                    _newProfileName = _newProfileName.Trim();
                    if (_newProfileName.Length == 0)
                    {
                        ImGui.OpenPopup("import_error_popup");
                    }
                    else
                    {
                        _errorMessage = ImportFromClipboard(_newProfileName);

                        if (_errorMessage == null)
                        {
                            _newProfileName = "";
                        }
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button("Import From File", new Vector2(200, 0)))
                {
                    _newProfileName = _newProfileName.Trim();
                    if (_newProfileName.Length == 0)
                    {
                        ImGui.OpenPopup("import_error_popup");
                    }
                    else
                    {
                        _errorMessage = ImportFromFile(_newProfileName);

                        if (_errorMessage == null)
                        {
                            _newProfileName = "";
                        }
                    }
                }

                // no name popup
                if (ImGui.BeginPopup("import_error_popup"))
                {
                    ImGui.Text("Please type a name for the new profile!");
                    ImGui.EndPopup();
                }
            }

            ImGui.EndChild();

            // error message
            if (_errorMessage != null)
            {
                if (ImGuiHelper.DrawErrorModal(_errorMessage))
                {
                    _errorMessage = null;
                }
            }

            // delete confirmation
            if (_deletingProfileName != null)
            {
                string message = "Are you sure you want to delete the profile \"" + _deletingProfileName + "\"?";
                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal("Delete?", message);

                if (didConfirm)
                {
                    _errorMessage = DeleteProfile(_deletingProfileName);
                    changed = true;
                }

                if (didConfirm || didClose)
                {
                    _deletingProfileName = null;
                }
            }

            // reset confirmation
            if (_resetingProfileName != null)
            {
                string message = "Are you sure you want to reset the profile \"" + _resetingProfileName + "\"?";
                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal("Reset?", message);

                if (didConfirm)
                {
                    ConfigurationManager.Instance.ResetConfig();
                    changed = true;
                }

                if (didConfirm || didClose)
                {
                    _resetingProfileName = null;
                }
            }

            // rename modal
            if (_renamingProfileName != null)
            {
                var (didConfirm, didClose) = ImGuiHelper.DrawInputModal("Rename", "Type a new name for the profile:", ref _renamingProfileName);

                if (didConfirm)
                {
                    _errorMessage = RenameCurrentProfile(_renamingProfileName);

                    changed = true;
                }

                if (didConfirm || didClose)
                {
                    _renamingProfileName = null;
                }
            }

            return false;
        }

        private void DrawAutoSwitchSettings(ref bool changed)
        {
            Profile profile = CurrentProfile();

            changed |= ImGui.Checkbox("Auto-Switch For Specific Jobs", ref profile.AutoSwitchEnabled);

            if (!profile.AutoSwitchEnabled)
            {
                return;
            }

            AutoSwitchData data = profile.AutoSwitchData;
            Vector2 cursorPos = ImGui.GetCursorPos() + new Vector2(14, 14);
            Vector2 originalPos = cursorPos;
            float maxY = 0;

            JobRoles[] roles = (JobRoles[])Enum.GetValues(typeof(JobRoles));

            foreach (JobRoles role in roles)
            {
                if (role == JobRoles.Unknown) { continue; }

                bool roleValue = data.GetRoleEnabled(role);
                string roleName = JobsHelper.RoleNames[role];

                ImGui.SetCursorPos(cursorPos);
                if (ImGui.Checkbox(roleName, ref roleValue))
                {
                    data.SetRoleEnabled(role, roleValue);
                    changed = true;
                }

                cursorPos.Y += 40;
                int jobCount = data.Map[role].Count;

                for (int i = 0; i < jobCount; i++)
                {
                    maxY = Math.Max(cursorPos.Y, maxY);
                    uint jobId = JobsHelper.JobsByRole[role][i];
                    bool jobValue = data.Map[role][i];
                    string jobName = JobsHelper.JobNames[jobId];

                    ImGui.SetCursorPos(cursorPos);
                    if (ImGui.Checkbox(jobName, ref jobValue))
                    {
                        data.Map[role][i] = jobValue;
                        changed = true;
                    }

                    cursorPos.Y += 30;
                }

                cursorPos.X += 100;
                cursorPos.Y = originalPos.Y;
            }

            ImGui.SetCursorPos(new Vector2(originalPos.X, maxY + 30));
        }
    }

    // fake config object
    [Disableable(false)]
    [Exportable(false)]
    [Shareable(false)]
    [Resettable(false)]
    public class ProfilesConfig : PluginConfigObject
    {
        public new static ProfilesConfig DefaultConfig() { return new ProfilesConfig(); }
    }

    // fake config page node
    public class ProfilesConfigPageNode : ConfigPageNode
    {
        public ProfilesConfigPageNode()
        {
            ConfigObject = new ProfilesConfig();
        }

        public override bool Draw(ref bool changed)
        {
            return ProfilesManager.Instance.Draw(ref changed);
        }
    }
}
