﻿using DelvUI.Helpers;
using DelvUI.Interface.Party;
using ImGuiNET;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DelvUI.Interface.PartyCooldowns
{
    public class PartyCooldown
    {
        public readonly PartyCooldownData Data;
        public readonly uint SourceId;
        public readonly IPartyFramesMember? Member;

        public double LastTimeUsed = 0;

        public PartyCooldown(PartyCooldownData data, uint sourceID, IPartyFramesMember? member)
        {
            Data = data;
            SourceId = sourceID;
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
            double timeSinceUse = ImGui.GetTime() - LastTimeUsed;
            if (timeSinceUse > Data.CooldownDuration)
            {
                return 0;
            }

            return Data.CooldownDuration - (float)timeSinceUse;
        }
    }

    public class PartyCooldownData
    {
        public bool Enabled = true;

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

        public string TooltipText()
        {
            string effectDuration = EffectDuration > 0 ? $"Duration: {EffectDuration}s \n" : "";
            return $"{effectDuration}Recast Time: {CooldownDuration}s";
        }

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
    }
}
