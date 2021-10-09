using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using DelvUI.Enums;

namespace DelvUI.Interface.GeneralElements
{
    public class PrimaryResourceHud : ParentAnchoredDraggableHudElement, IHudElementWithActor, IHudElementWithAnchorableParent
    {
        private PrimaryResourceConfig Config => (PrimaryResourceConfig)_config;
        private LabelHud _valueLabel;

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
            _valueLabel = new LabelHud(config.ValueLabelConfig);
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
            int percent = 0;

            GetResources(ref current, ref max, ref percent, chara);
            if (Config.HidePrimaryResourceWhenFull && current == max) { return; }

            var scale = (float)current / max;
            Vector2 startPos = origin + GetAnchoredPosition(Config.Position, Config.Size, Config.Anchor);

            DrawHelper.DrawInWindow(ID, startPos, Config.Size, false, false, (drawList) =>
            {
                // bar
                drawList.AddRectFilled(startPos, startPos + Config.Size, 0x88000000);

                var color = Config.ShowThresholdMarker && percent < Config.ThresholdMarkerValue / 100 ? Config.BelowThresholdColor : Color(Actor);

                DrawHelper.DrawGradientFilledRect(startPos, new Vector2(Config.Size.X * scale, Config.Size.Y), color, drawList);

                drawList.AddRect(startPos, startPos + Config.Size, 0xFF000000);

                // threshold
                if (Config.ShowThresholdMarker)
                {
                    var position = new Vector2(startPos.X + Config.ThresholdMarkerValue / 10000f * Config.Size.X - 2, startPos.Y);
                    var size = new Vector2(2, Config.Size.Y);
                    drawList.AddRect(position, position + size, 0xFF000000);
                }
            });


            // label
            if (Config.ValueLabelConfig.Enabled)
            {
                Config.ValueLabelConfig.SetText($"{current,0}");
                _valueLabel.Draw(startPos, Config.Size, Actor);
            }
        }

        private void GetResources(ref int current, ref int max, ref int percent, Character actor)
        {
            switch (ResourceType)
            {
                case PrimaryResourceTypes.MP:
                    {
                        current = (int)actor.CurrentMp;
                        max = (int)actor.MaxMp;
                        if (max != 0) { percent = (int)Math.Round((double)(100 * current / max)); }
                    }

                    break;

                case PrimaryResourceTypes.CP:
                    {
                        current = (int)actor.CurrentCp;
                        max = (int)actor.MaxCp;
                        if (max != 0) { percent = (int)Math.Round((double)(100 * current / max)); }
                    }

                    break;

                case PrimaryResourceTypes.GP:
                    {
                        current = (int)actor.CurrentGp;
                        max = (int)actor.MaxGp;
                        if (max != 0) { percent = (int)Math.Round((double)(100 * current / max)); }
                    }

                    break;
            }
        }

        public virtual PluginConfigColor Color(GameObject? actor = null)
        {
            if (!Config.UseJobColor)
            {
                return Config.Color;
            }

            return actor is not Character character ? GlobalColors.Instance.NPCFriendlyColor : Utils.ColorForActor(character);
        }
    }
}
