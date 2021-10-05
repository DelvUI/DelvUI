using Dalamud.Interface;
using Dalamud.Logging;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace DelvUI.Config.Profiles
{
    [Disableable(false)]
    [Exportable(false)]
    [ProfileShareable(false)]
    [Section("Profiles")]
    [SubSection("General", 0)]
    public class ProfilesConfig : PluginConfigObject
    {
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
        [JsonIgnore] private static string GeneralJsonPath => Path.Combine(ProfilesPath, "General.json");

        [JsonIgnore] private readonly string DefaultProfileName = "Default";
        [JsonIgnore] private string _newProfileName = "";
        [JsonIgnore] private int _copyFromIndex = 0;
        [JsonIgnore] private int _selectedProfileIndex = 0;
        [JsonIgnore] private string? _errorMessage = null;
        [JsonIgnore] private string? _deletingProfileName = null;
        [JsonIgnore] private string? _resetingProfileName = null;

        public SortedList<string, Profile> Profiles = new SortedList<string, Profile>();

        public new static ProfilesConfig DefaultConfig() { return new ProfilesConfig(); }


        public ProfilesConfig()
        {
            // default profile
            if (!Profiles.ContainsKey(DefaultProfileName))
            {
                var defaultProfile = new Profile(DefaultProfileName);
                Profiles.Add(DefaultProfileName, defaultProfile);
            }
        }

        public void Initialize()
        {
            // make sure default profile file is created the first time this runs
            if (!File.Exists(CurrentProfilePath()))
            {
                SaveCurrentProfile();
            }
        }

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

            SaveCurrentProfile(ConfigurationManager.Instance.ExportCurrentConfigs());
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
            catch
            {
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
            catch
            {
                return "Error trying to clone profile \"" + profileName + "\"!";
            }

            return null;
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
            catch
            {
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

            File.WriteAllText(GeneralJsonPath, jsonString);
        }

        public static ProfilesConfig? Load()
        {
            ProfilesConfig? config;

            try
            {
                string jsonString = File.ReadAllText(GeneralJsonPath);
                config = JsonConvert.DeserializeObject<ProfilesConfig>(jsonString);
            }
            catch
            {
                return null;
            }

            return config;
        }

        [ManualDraw]
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

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.SameLine();
                if (ImGui.Button("\uf2f9", new Vector2(0, 0)))
                {
                    _resetingProfileName = _currentProfileName;
                }

                ImGui.SameLine();
                if (_currentProfileName != DefaultProfileName && ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                {
                    _deletingProfileName = _currentProfileName;
                }
                ImGui.PopFont();

                ImGuiHelper.NewLineAndTab();
                DrawAutoSwitchSettings(ref changed);

                ImGuiHelper.DrawSeparator(1, 1);
                ImGuiHelper.Tab();
                ImGui.Text("Create a new profile:");

                ImGuiHelper.NewLineAndTab();
                ImGui.InputText("Profile Name", ref _newProfileName, 200);

                ImGuiHelper.NewLineAndTab();
                ImGui.Combo("Copy from", ref _copyFromIndex, profiles, profiles.Length, 10);

                ImGuiHelper.NewLineAndTab();
                if (ImGui.Button("Create", new Vector2(200, 0)))
                {
                    _newProfileName = _newProfileName.Trim();

                    if (Profiles.Keys.Contains(_newProfileName))
                    {
                        _errorMessage = "A profile with the name \"" + _newProfileName + "\" already exists!";
                    }
                    else
                    {
                        _errorMessage = CloneProfile(profiles[_copyFromIndex], _newProfileName);

                        if (_errorMessage == null)
                        {
                            _errorMessage = SwitchToProfile(_newProfileName);
                        }
                    }
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
}
