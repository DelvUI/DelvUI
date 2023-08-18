using DelvUI.Helpers;
using DelvUI.Interface.Party;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DelvUI.Interface.PartyCooldowns
{
    public enum PartyCooldownEnabled
    {
        PartyCooldownsAndPartyFrames = 0,
        PartyCooldowns = 1,
        PartyFrames = 2,
        Disabled = 3
    }

    public class PartyCooldown
    {
        public readonly PartyCooldownData Data;
        public readonly uint SourceId;
        public readonly uint MemberLevel;
        public readonly IPartyFramesMember? Member;

        public double LastTimeUsed = 0;
        public double OverridenCooldownStartTime = -1;

        public PartyCooldown(PartyCooldownData data, uint sourceID, uint level, IPartyFramesMember? member)
        {
            Data = data;
            SourceId = sourceID;
            MemberLevel = level;
            Member = member;
        }

        public float EffectTimeRemaining()
        {
            double timeSinceUse = ImGui.GetTime() - LastTimeUsed;
            if (timeSinceUse > Data.EffectDuration)
            {
                return 0;
            }

            return Data.EffectDuration - (float)timeSinceUse;
        }

        public float CooldownTimeRemaining()
        {
            int cooldown = GetCooldown();
            double timeSinceUse = OverridenCooldownStartTime != -1 ? ImGui.GetTime() - OverridenCooldownStartTime : ImGui.GetTime() - LastTimeUsed;

            if (timeSinceUse > cooldown)
            {
                OverridenCooldownStartTime = -1;
                LastTimeUsed = 0;
                return 0;
            }

            return cooldown - (float)timeSinceUse;
        }

        private int GetCooldown()
        {
            // not happy about this but didn't want to over-complicate things
            // special case for troubadour, shield samba and tactician
            if (MemberLevel < 88) { return Data.CooldownDuration; }
            if (Data.ActionId != 7405 && Data.ActionId != 16012 && Data.ActionId != 16889) { return Data.CooldownDuration; }

            return 90;
        }

        public string TooltipText()
        {
            string effectDuration = Data.EffectDuration > 0 ? $"Duration: {Data.EffectDuration}s \n" : "";
            return $"{effectDuration}Recast Time: {GetCooldown()}s";
        }
    }

    public class PartyCooldownData : IEquatable<PartyCooldownData>
    {
        public PartyCooldownEnabled EnabledV2 = PartyCooldownEnabled.PartyCooldownsAndPartyFrames;

        public uint ActionId = 0;
        public uint RequiredLevel = 0;

        public uint JobId = 0; // keep this for backwards compatibility
        public List<uint>? JobIds = null;

        public JobRoles Role = JobRoles.Unknown; // keep this for backwards compatibility
        public List<JobRoles>? Roles = null;

        public int CooldownDuration = 0;
        public int EffectDuration = 0;

        public int Priority = 0;
        public int Column = 1;

        [JsonIgnore] public uint IconId = 0;
        [JsonIgnore] public string Name = "";
        [JsonIgnore] public string? OverriddenCooldownText = null;

        public virtual bool IsUsableBy(uint jobId)
        {
            JobRoles roleForJob = JobsHelper.RoleForJob(jobId);

            if (Roles != null)
            {
                foreach (JobRoles role in Roles)
                {
                    if (role == roleForJob)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (Role != JobRoles.Unknown)
            {
                return Role == roleForJob;
            }

            if (JobIds != null)
            {
                foreach (uint id in JobIds)
                {
                    if (id == jobId)
                    {
                        return true;
                    }
                }
            }

            return JobId == jobId;
        }

        public bool HasRole(JobRoles role)
        {
            if (Roles != null)
            {
                return Roles.Contains(role);
            }

            if (Role != JobRoles.Unknown)
            {
                return Role == role;
            }

            if (JobIds != null)
            {
                foreach (uint jobId in JobIds)
                {
                    JobRoles roleForJob = JobsHelper.RoleForJob(jobId);
                    if (roleForJob == role)
                    {
                        return true;
                    }
                }

                return false;
            }

            return JobsHelper.RoleForJob(JobId) == role;
        }

        public bool IsEnabledForPartyCooldowns()
        {
            return EnabledV2 == PartyCooldownEnabled.PartyCooldownsAndPartyFrames ||
                   EnabledV2 == PartyCooldownEnabled.PartyCooldowns;
        }

        public bool IsEnabledForPartyFrames()
        {
            return EnabledV2 == PartyCooldownEnabled.PartyCooldownsAndPartyFrames ||
                   EnabledV2 == PartyCooldownEnabled.PartyFrames;
        }

        public bool Equals(PartyCooldownData? other)
        {
            if (other == null) { return false; }

            return
                ActionId == other.ActionId &&
                RequiredLevel == other.RequiredLevel &&
                JobId == other.JobId &&
                (JobIds == null && other.JobIds == null || (JobIds != null && other.JobIds != null && JobIds.Equals(other.JobIds))) &&
                Role == other.Role &&
                (Roles == null && other.Roles == null || (Roles != null && other.Roles != null && Roles.Equals(other.Roles))) &&
                CooldownDuration == other.CooldownDuration &&
                EffectDuration == other.EffectDuration;
        }
    }
}
