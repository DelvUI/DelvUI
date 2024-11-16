using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Collections.Generic;

namespace DelvUI.Interface.Party
{
    public static class PartyOrderHelper
    {
        private enum PartySortingSetting
        {
            Tank_Healer_DPS = 0,
            Tank_DPS_Healer = 1,
            Healer_Tank_DPS = 2,
            Healer_DPS_Tank = 3,
            DPS_Tank_Healer = 4,
            DPS_Healer_Tank = 5,
            Count = 6
        }

        private class PartyRoles
        {
            internal int Tank;
            internal int Healer;
            internal int DPS;
            internal int Other;

            public PartyRoles()
            {
                Tank = 0;
                Healer = 0;
                DPS = 0;
                Other = 0;
            }

            public PartyRoles(int tank, int healer, int dps, int other)
            {
                Tank = tank;
                Healer = healer;
                DPS = dps;
                Other = other;
            }
        }

        // calcualates the position for the player if they select the
        // option to always appear as the first of their current role
        // in the party frames
        public static int? GetRoleFirstOrder(List<IPartyFramesMember> members)
        {
            IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player == null) { return null; }

            JobRoles role = JobsHelper.RoleForJob(player.ClassJob.RowId);

            PartySortingSetting? setting = GetPartySortingSetting(role);
            if (!setting.HasValue) { return null; }

            PartyRoles rolesCount = GetPartyCountByRole(members);
            PartyRoles roleWeights = GetRoleWeights(role, setting.Value);

            return rolesCount.Tank * roleWeights.Tank +
                   rolesCount.Healer * roleWeights.Healer +
                   rolesCount.DPS * roleWeights.DPS +
                   rolesCount.Other * roleWeights.Other;
        }

        private static unsafe PartySortingSetting? GetPartySortingSetting(JobRoles role)
        {
            ConfigModule* config = ConfigModule.Instance();
            if (config == null) { return null; }

            ConfigOption option;
            switch (role)
            {
                case JobRoles.Tank: option = ConfigOption.PartyListSortTypeTank; break;

                case JobRoles.Healer: option = ConfigOption.PartyListSortTypeHealer; break;

                case JobRoles.DPSMelee:
                case JobRoles.DPSRanged:
                case JobRoles.DPSCaster: option = ConfigOption.PartyListSortTypeDps; break;

                default: option = ConfigOption.PartyListSortTypeOther; break;
            }

            Framework* framework = Framework.Instance();
            if (framework == null || framework->SystemConfig.SystemConfigBase.UiConfig.ConfigCount <= (int)option) {
                return PartySortingSetting.Tank_Healer_DPS; 
            }

            uint value = framework->SystemConfig.SystemConfigBase.UiConfig.ConfigEntry[(int)option].Value.UInt;
            if (value < 0 || value > (int)PartySortingSetting.Count) { return null; }

            return (PartySortingSetting)value;
        }

        private static unsafe PartyRoles GetPartyCountByRole(List<IPartyFramesMember> members)
        {
            PartyRoles rolesCount = new PartyRoles();

            IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;
            if (player == null) { return rolesCount; }

            foreach (IPartyFramesMember member in members)
            {
                if (member.ObjectId == player.GameObjectId) { continue; }

                JobRoles role = JobsHelper.RoleForJob(member.JobId);
                switch (role)
                {
                    case JobRoles.Tank: rolesCount.Tank++; break;

                    case JobRoles.Healer: rolesCount.Healer++; break;

                    case JobRoles.DPSMelee:
                    case JobRoles.DPSRanged:
                    case JobRoles.DPSCaster: rolesCount.DPS++; break;

                    default: rolesCount.Other++; break;
                }
            }

            return rolesCount;
        }

        private static unsafe PartyRoles GetRoleWeights(JobRoles role, PartySortingSetting setting)
        {
            if (role == JobRoles.Crafter || role == JobRoles.Gatherer || role == JobRoles.Unknown)
            {
                return new PartyRoles(1, 1, 1, 0);
            }

            JobRoles mapRole = role == JobRoles.DPSRanged || role == JobRoles.DPSCaster ? JobRoles.DPSMelee : role;
            return RoleWeights[mapRole][setting];
        }

        private static Dictionary<JobRoles, Dictionary<PartySortingSetting, PartyRoles>> RoleWeights = new Dictionary<JobRoles, Dictionary<PartySortingSetting, PartyRoles>>()
        {
            [JobRoles.Tank] = new Dictionary<PartySortingSetting, PartyRoles>()
            {
                [PartySortingSetting.Tank_Healer_DPS] = new PartyRoles(),
                [PartySortingSetting.Tank_DPS_Healer] = new PartyRoles(),
                [PartySortingSetting.Healer_Tank_DPS] = new PartyRoles(0, 1, 0, 0),
                [PartySortingSetting.Healer_DPS_Tank] = new PartyRoles(0, 1, 1, 0),
                [PartySortingSetting.DPS_Tank_Healer] = new PartyRoles(0, 0, 1, 0),
                [PartySortingSetting.DPS_Healer_Tank] = new PartyRoles(0, 1, 1, 0)
            },

            [JobRoles.Healer] = new Dictionary<PartySortingSetting, PartyRoles>()
            {
                [PartySortingSetting.Tank_Healer_DPS] = new PartyRoles(1, 0, 0, 0),
                [PartySortingSetting.Tank_DPS_Healer] = new PartyRoles(1, 0, 1, 0),
                [PartySortingSetting.Healer_Tank_DPS] = new PartyRoles(),
                [PartySortingSetting.Healer_DPS_Tank] = new PartyRoles(),
                [PartySortingSetting.DPS_Tank_Healer] = new PartyRoles(1, 0, 1, 0),
                [PartySortingSetting.DPS_Healer_Tank] = new PartyRoles(0, 0, 1, 0)
            },

            [JobRoles.DPSMelee] = new Dictionary<PartySortingSetting, PartyRoles>()
            {
                [PartySortingSetting.Tank_Healer_DPS] = new PartyRoles(1, 1, 0, 0),
                [PartySortingSetting.Tank_DPS_Healer] = new PartyRoles(1, 0, 0, 0),
                [PartySortingSetting.Healer_Tank_DPS] = new PartyRoles(1, 1, 0, 0),
                [PartySortingSetting.Healer_DPS_Tank] = new PartyRoles(0, 1, 0, 0),
                [PartySortingSetting.DPS_Tank_Healer] = new PartyRoles(),
                [PartySortingSetting.DPS_Healer_Tank] = new PartyRoles()
            }
        };
    }
}
