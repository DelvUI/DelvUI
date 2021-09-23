using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config.Attributes;
using DelvUI.Config.Tree;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DelvUI.Config
{
    [Disableable(false)]
    [Portable(false)]
    [Section("Profiles")]
    [SubSection("Profiles", 0)]
    public class ProfilesConfig : PluginConfigObject
    {
        [JsonIgnore] public static string DefaultProfileName = "Defaults";
        [JsonIgnore] public static string DefaultPlayerProfileName = "My profile";
        public Dictionary<string, string> Profiles = new Dictionary<string, string>();
        public string CurrentProfile = DefaultProfileName;

        public Dictionary<uint, string> JobProfileMap = new Dictionary<uint, string>();

        private int _selectedJob = 0;
        private int _selectedProfileForJob = 0;

        private string _newProfileName = "New profile";

        public new static ProfilesConfig DefaultConfig() { return new ProfilesConfig(); }

        // TODO ideally this would be done by DefaultConfig()
        // but this needs to be called after ConfigurationManager's ConfigBaseNode is fully populated
        public void GenerateDefaultsProfile(BaseNode baseNode)
        {
            // generate default and default player profiles
            string defaultString = baseNode.GetBase64String();
            Profiles[DefaultProfileName] = defaultString;
            Profiles[DefaultPlayerProfileName] = defaultString;
            CurrentProfile = DefaultPlayerProfileName;
        }

        public void UpdateCurrentProfile(BaseNode baseNode)
        {
            Profiles[CurrentProfile] = baseNode.GetBase64String();
        }

        public bool SwitchToProfile(string profileName)
        {
            string profileString;
            if (!Profiles.TryGetValue(profileName, out profileString))
            {
                return false;
            }
            string[] importStrings = profileString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            ConfigurationManager.LoadTotalConfiguration(importStrings);
            CurrentProfile = profileString;
            return true;
        }

        [ManualDraw]
        public bool DrawProfile()
        {
            bool changed = false;
            string profileToRemove = "";
            // TODO debug assert that the default profile exists

            if (ImGui.BeginChild("Profiles", new Vector2(0, 0), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.NewLine();

                var flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame;

                ImGui.Text("\u2002");
                ImGui.SameLine();
                if (ImGui.BeginTable("table", 3, flags, new Vector2(500, 150)))
                {
                    ImGui.TableSetupColumn("Profile name", ImGuiTableColumnFlags.WidthStretch, 0, 0);
                    ImGui.TableSetupColumn("Export", ImGuiTableColumnFlags.WidthFixed, 0, 1);
                    ImGui.TableSetupColumn("Remove", ImGuiTableColumnFlags.WidthFixed, 0, 2);

                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    foreach (KeyValuePair<string, string> profile in Profiles)
                    {
                        ImGui.PushID(profile.Key);
                        ImGui.TableNextRow(ImGuiTableRowFlags.None);

                        // profile name/button
                        if (ImGui.TableSetColumnIndex(0))
                        {
                            if (profile.Key == DefaultProfileName)
                            {
                                ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                                if (ImGui.Button(DefaultProfileName + " (export only)"))
                                {
                                }
                                ImGui.PopStyleColor(1);
                            }
                            else
                            {
                                ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                                if (ImGui.Button((profile.Key == CurrentProfile ? "> " : "") + profile.Key) && profile.Value != "")
                                {
                                    string[] importStrings = profile.Value.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    ConfigurationManager.LoadTotalConfiguration(importStrings);
                                    CurrentProfile = profile.Key;
                                    changed = true;
                                }
                                ImGui.PopStyleColor(1);
                            }
                        }

                        // export button
                        if (ImGui.TableSetColumnIndex(1))
                        {
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                            if (ImGui.Button(FontAwesomeIcon.FileExport.ToIconString()))
                            {
                                string _exportString = "";
                                if (profile.Key == CurrentProfile)
                                {
                                    _exportString = ConfigurationManager.GetInstance().ConfigBaseNode.GetBase64String();
                                }
                                else
                                {
                                    _exportString = profile.Value;
                                }
                                try
                                {
                                    if (_exportString != "")
                                    {
                                        ImGui.SetClipboardText(_exportString);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    PluginLog.Log("Could not set clipboard text:\n" + ex.StackTrace);
                                }
                            }
                            ImGui.PopFont();
                            ImGui.PopStyleColor(1);
                        }

                        // enable a remove button for all non-default profiles
                        // except for if there is only one non-default profile available
                        if (profile.Key != DefaultProfileName && Profiles.Count != 2 && ImGui.TableSetColumnIndex(2))
                        {
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                            if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                            {
                                profileToRemove = profile.Key;
                                changed = true;
                            }
                            ImGui.PopFont();
                            ImGui.PopStyleColor(1);
                        }
                        ImGui.PopID();
                    }

                    ImGui.PushID("new profile row");
                    ImGui.TableNextRow(ImGuiTableRowFlags.None);

                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.PushItemWidth(150);
                        ImGui.InputText("", ref _newProfileName, 80);

                        ImGui.SameLine();
                        ImGui.PushFont(UiBuilder.IconFont);
                        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), new Vector2(0, 0)) && _newProfileName != "" && !Profiles.ContainsKey(_newProfileName))
                        {
                            // create a new profile copying the default
                            Profiles[_newProfileName] = Profiles[DefaultProfileName];
                            CurrentProfile = _newProfileName;
                            changed = true;
                        }
                        ImGui.PopFont();

                        ImGui.SameLine();
                        ImGui.PushFont(UiBuilder.IconFont);
                        if (ImGui.Button(FontAwesomeIcon.FileImport.ToIconString(), new Vector2(0, 0)))
                        {
                            string _importString = "";
                            try
                            {
                                _importString = ImGui.GetClipboardText();
                            }
                            catch (Exception ex)
                            {
                                PluginLog.Log("Could not get clipboard text:\n" + ex.StackTrace);
                            }

                            if (_importString != "")
                            {
                                Profiles[_newProfileName] = _importString.Trim();
                                changed = true;
                            }
                        }
                        ImGui.PopFont();
                    }

                    ImGui.EndTable();
                }
            }

            // generate array of jobs
            uint[] jobs = typeof(JobIDs).GetFields()
                .Where(o => o.FieldType == typeof(uint))
                .Select(o => (uint)o.GetValue(null))
                .ToArray();
            string[] jobNames = jobs.Select(o => JobsHelper.JobNames[o]).ToArray();
            string[] availableProfiles = Profiles.Keys.Select(o => o == DefaultProfileName ? "-" : o).ToArray();

            ImGui.Text("\u2002");
            ImGui.SameLine();
            if (ImGui.Combo("Job ##profilejobs", ref _selectedJob, jobNames, jobNames.Length, 10))
            {
                // if a job was selected in the jobs combo box, update the displayed profile for it, if it exists
                string profileForJob;
                if (JobProfileMap.TryGetValue(jobs[_selectedJob], out profileForJob))
                {
                    PluginLog.Log($"loading {profileForJob} from key {jobs[_selectedJob]}");
                    _selectedProfileForJob = Array.FindIndex(availableProfiles, o => o == profileForJob);
                    // if the profile assigned to the job _selectedJob is no longer available
                    // then set it to the default profile, which corresponds to the symbol "-"
                    if (_selectedProfileForJob == -1)
                    {
                        _selectedProfileForJob = Array.FindIndex(availableProfiles, o => o == "-");
                    }
                }
                else
                {
                    _selectedProfileForJob = Array.FindIndex(availableProfiles, o => o == "-");
                }
            }

            ImGui.Text("\u2002");
            ImGui.SameLine();
            ImGui.Text("\u2002");
            ImGui.SameLine();
            if (ImGui.Combo("Profile ##profilejobsoptions", ref _selectedProfileForJob, availableProfiles, availableProfiles.Length, 10))
            {
                if (availableProfiles[_selectedProfileForJob] != "-")
                {
                    JobProfileMap[jobs[_selectedJob]] = availableProfiles[_selectedProfileForJob];
                    PluginLog.Log($"Adding profile {availableProfiles[_selectedProfileForJob]} to {jobs[_selectedJob]}");
                }
            }

            ImGui.EndChild();

            if (profileToRemove != "")
            {
                CurrentProfile = DefaultProfileName;
                Profiles.Remove(profileToRemove);
            }

            return changed;
        }
    }
}
