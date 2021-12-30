using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Enums;
using DelvUI.Interface.Bars;
using DelvUI.Interface.Party;

namespace DelvUI.Interface.GeneralElements
{
    public class PrimaryResourceHud : ParentAnchoredDraggableHudElement, IHudElementWithActor, IHudElementWithAnchorableParent
    {
        private PrimaryResourceConfig Config => (PrimaryResourceConfig)_config;

        public PrimaryResourceTypes ResourceType = PrimaryResourceTypes.MP;

        private GameObject? _actor;
        public GameObject? Actor
        {
            get => _actor;
            set
            {
                if (value is PlayerCharacter chara)
                {
                    _actor = value;

                    JobRoles role = JobsHelper.RoleForJob(chara.ClassJob.Id);
                    ResourceType = JobsHelper.PrimaryResourceTypesByRole[role];
                }
                else
                {
                    _actor = null;
                    ResourceType = PrimaryResourceTypes.None;
                }
            }
        }

        public IPartyFramesMember? PartyMember;

        protected override bool AnchorToParent => Config is UnitFramePrimaryResourceConfig config ? config.AnchorToUnitFrame : false;
        protected override DrawAnchor ParentAnchor => Config is UnitFramePrimaryResourceConfig config ? config.UnitFrameAnchor : DrawAnchor.Center;

        public PrimaryResourceHud(PrimaryResourceConfig config, string? displayName = null) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled)
            {
                return;
            }

            if (PartyMember == null && (ResourceType == PrimaryResourceTypes.None || Actor == null || Actor is not PlayerCharacter))
            {
                return;
            }

            Character? chara = Actor != null ? (Character)Actor : null;
            uint current = chara == null ? PartyMember?.MP ?? 0 : 0;
            uint max = chara == null ? PartyMember?.MaxMP ?? 0 : 0;

            if (chara != null)
            {
                GetResources(ref current, ref max, chara);
            }

            if (Config.HidePrimaryResourceWhenFull && current == max)
            {
                return;
            }

            BarHud bar = BarUtilities.GetProgressBar(
                Config,
                Config.ThresholdConfig,
                new LabelConfig[] { Config.ValueLabel },
                current,
                max,
                0,
                chara,
                GetColor()
            );

            Vector2 pos = origin + ParentPos();
            AddDrawActions(bar.GetDrawActions(pos, Config.StrataLevel));
        }

        private void GetResources(ref uint current, ref uint max, Character actor)
        {
            switch (ResourceType)
            {
                case PrimaryResourceTypes.MP:
                    current = actor.CurrentMp;
                    max = actor.MaxMp;
                    break;

                case PrimaryResourceTypes.CP:
                    current = actor.CurrentCp;
                    max = actor.MaxCp;
                    break;

                case PrimaryResourceTypes.GP:
                    current = actor.CurrentGp;
                    max = actor.MaxGp;
                    break;
            }
        }

        public virtual PluginConfigColor GetColor()
        {
            if (!Config.UseJobColor)
            {
                return Config.FillColor;
            }

            if (PartyMember != null)
            {
                return GlobalColors.Instance.SafeColorForJobId(PartyMember.JobId);
            }

            return Utils.ColorForActor(Actor);
        }
    }
}
