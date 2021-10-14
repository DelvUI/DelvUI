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

        protected override bool AnchorToParent => Config is UnitFramePrimaryResourceConfig config ? config.AnchorToUnitFrame : false;
        protected override DrawAnchor ParentAnchor => Config is UnitFramePrimaryResourceConfig config ? config.UnitFrameAnchor : DrawAnchor.Center;

        public PrimaryResourceHud(PrimaryResourceConfig config, string displayName) : base(config, displayName)
        {
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || ResourceType == PrimaryResourceTypes.None || Actor == null || Actor is not PlayerCharacter)
            {
                return;
            }

            var chara = (Character)Actor;
            int current = 0;
            int max = 0;

            GetResources(ref current, ref max, chara);
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
                GetColor(chara)
            );

            bar.Draw(origin + ParentPos());
        }

        private void GetResources(ref int current, ref int max, Character actor)
        {
            switch (ResourceType)
            {
                case PrimaryResourceTypes.MP:
                    {
                        current = (int)actor.CurrentMp;
                        max = (int)actor.MaxMp;
                    }

                    break;

                case PrimaryResourceTypes.CP:
                    {
                        current = (int)actor.CurrentCp;
                        max = (int)actor.MaxCp;
                    }

                    break;

                case PrimaryResourceTypes.GP:
                    {
                        current = (int)actor.CurrentGp;
                        max = (int)actor.MaxGp;
                    }

                    break;
            }
        }

        public virtual PluginConfigColor GetColor(GameObject? actor = null)
        {
            if (!Config.UseJobColor)
            {
                return Config.FillColor;
            }

            return actor is not Character character ? GlobalColors.Instance.NPCFriendlyColor : Utils.ColorForActor(character);
        }
    }
}
