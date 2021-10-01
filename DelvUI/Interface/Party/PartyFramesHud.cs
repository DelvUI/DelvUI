using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    public class PartyFramesHud : DraggableHudElement
    {
        private new PartyFramesConfig Config => (PartyFramesConfig)base.Config;
        private readonly PartyFramesHealthBarsConfig _healthBarsConfig;

        private Vector2 _contentMargin = new(40, 40);
        private static readonly int MaxMemberCount = 9; // 8 players + chocobo

        // layout
        private Vector2 _origin;
        private Vector2 _size;
        private LayoutInfo _layoutInfo;
        private uint _memberCount = 0;
        private bool _layoutDirty = true;

        private readonly List<PartyFramesBar> _bars;


        public PartyFramesHud(string id, PartyFramesConfig config, string displayName) : base(id, config, displayName)
        {
            _healthBarsConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesHealthBarsConfig>();
            PartyFramesManaBarConfig? manaBarConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesManaBarConfig>();
            PartyFramesCastbarConfig? castbarConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesCastbarConfig>();
            PartyFramesRoleIconConfig? roleIconConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesRoleIconConfig>();
            PartyFramesLeaderIconConfig? leaderIconConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesLeaderIconConfig>();
            PartyFramesBuffsConfig? buffsConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesBuffsConfig>();
            PartyFramesDebuffsConfig? debuffsConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesDebuffsConfig>();

            config.ValueChangeEvent += OnLayoutPropertyChanged;
            _healthBarsConfig.ValueChangeEvent += OnLayoutPropertyChanged;
            _healthBarsConfig.ColorsConfig.ValueChangeEvent += OnLayoutPropertyChanged;

            _bars = new List<PartyFramesBar>(MaxMemberCount);
            for (int i = 0; i < _bars.Capacity; i++)
            {
                var bar = new PartyFramesBar(i.ToString(), _healthBarsConfig, manaBarConfig, castbarConfig, roleIconConfig, leaderIconConfig, buffsConfig, debuffsConfig);
                bar.MovePlayerEvent += OnMovePlayer;

                _bars.Add(bar);
            }

            PartyManager.Instance.MembersChangedEvent += OnMembersChanged;
            UpdateBars(Vector2.Zero);
        }

        protected override void InternalDispose()
        {
            _bars.Clear();

            base.Config.ValueChangeEvent -= OnLayoutPropertyChanged;
            _healthBarsConfig.ValueChangeEvent -= OnLayoutPropertyChanged;
            _healthBarsConfig.ColorsConfig.ValueChangeEvent -= OnLayoutPropertyChanged;
            PartyManager.Instance.MembersChangedEvent -= OnMembersChanged;
        }

        private void OnMovePlayer(PartyFramesBar bar)
        {
            if (Config.PlayerOrderOverrideEnabled && bar.Member != null)
            {
                int offset = bar.Member.Order - 1 > Config.PlayerOrder ? -1 : -2;
                Config.PlayerOrder = Math.Max(0, Math.Min(7, bar.Member.Order + offset));
                PartyManager.Instance.OnPlayerOrderChange();

                ConfigurationManager.Instance.SaveConfigurations();
            }
        }

        private void OnLayoutPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "Size" ||
                args.PropertyName == "FillRowsFirst" ||
                args.PropertyName == "BarsAnchor" ||
                args.PropertyName == "Padding")
            {
                _layoutDirty = true;
            }
        }

        private void OnMembersChanged(PartyManager sender)
        {
            UpdateBars(_origin);
        }

        public void UpdateBars(Vector2 origin)
        {
            uint memberCount = PartyManager.Instance.MemberCount;
            uint row = 0;
            uint col = 0;
            Vector2 spaceSize = Config.Size - _contentMargin * 2;

            for (int i = 0; i < _bars.Count; i++)
            {
                PartyFramesBar bar = _bars[i];
                if (i >= memberCount)
                {
                    bar.Visible = false;
                    continue;
                }

                // update bar
                IPartyFramesMember member = PartyManager.Instance.GroupMembers.ElementAt(i);
                bar.Member = member;
                bar.Visible = true;

                // anchor and position
                CalculateBarPosition(origin, spaceSize, out float x, out float y);
                bar.Position = new Vector2(
                    x + _healthBarsConfig.Size.X * col + _healthBarsConfig.Padding.X * col,
                    y + _healthBarsConfig.Size.Y * row + _healthBarsConfig.Padding.Y * row
                );

                // layout
                if (Config.FillRowsFirst)
                {
                    col = col + 1;
                    if (col >= _layoutInfo.TotalColCount)
                    {
                        col = 0;
                        row = row + 1;
                    }
                }
                else
                {
                    row = row + 1;
                    if (row >= _layoutInfo.TotalRowCount)
                    {
                        row = 0;
                        col = col + 1;
                    }
                }
            }
        }

        private void CalculateBarPosition(Vector2 position, Vector2 spaceSize, out float x, out float y)
        {
            x = position.X;
            y = position.Y;

            if (Config.BarsAnchor == DrawAnchor.Top ||
                Config.BarsAnchor == DrawAnchor.Center ||
                Config.BarsAnchor == DrawAnchor.Bottom)
            {
                x += (spaceSize.X - _layoutInfo.ContentSize.X) / 2f;
            }
            else if (Config.BarsAnchor == DrawAnchor.TopRight ||
                Config.BarsAnchor == DrawAnchor.Right ||
                Config.BarsAnchor == DrawAnchor.BottomRight)
            {
                x += spaceSize.X - _layoutInfo.ContentSize.X;
            }

            if (Config.BarsAnchor == DrawAnchor.Left ||
                Config.BarsAnchor == DrawAnchor.Center ||
                Config.BarsAnchor == DrawAnchor.Right)
            {
                y += (spaceSize.Y - _layoutInfo.ContentSize.Y) / 2f;
            }
            else if (Config.BarsAnchor == DrawAnchor.BottomLeft ||
                Config.BarsAnchor == DrawAnchor.Bottom ||
                Config.BarsAnchor == DrawAnchor.BottomRight)
            {
                y += spaceSize.Y - _layoutInfo.ContentSize.Y;
            }
        }

        private void UpdateBarsPosition(Vector2 delta)
        {
            foreach (PartyFramesBar? bar in _bars)
            {
                bar.Position = bar.Position + delta;
            }
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position + Config.Size / 2f }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!base.Config.Enabled)
            {
                return;
            }

            // size and position
            ImGui.SetNextWindowPos(Config.Position - _contentMargin, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(Config.Size + _contentMargin * 2, ImGuiCond.FirstUseEver);

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoBackground;

            bool canDrag = !Config.Lock && !DraggingEnabled;
            if (!canDrag)
            {
                windowFlags |= ImGuiWindowFlags.NoMove;
            }

            if (Config.Lock || DraggingEnabled)
            {
                ImGui.SetNextWindowPos(origin + Config.Position);
                windowFlags |= ImGuiWindowFlags.NoResize;
            }

            ImGui.Begin("delvui_partyFrames", windowFlags);
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowSize = ImGui.GetWindowSize();
            Config.Size = windowSize;

            if (canDrag)
            {
                Config.Position = windowPos - origin;
            }

            // recalculate layout on settings or size change
            Vector2 contentStartPos = windowPos + _contentMargin;
            Vector2 maxSize = windowSize - _contentMargin * 2;

            // preview
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            if (!Config.Lock)
            {
                var margin = new Vector2(4, 0);
                drawList.AddRectFilled(contentStartPos, contentStartPos + maxSize, 0x66000000);
                drawList.AddRect(windowPos + margin, windowPos + windowSize - margin * 2, 0x88000000, 3, ImDrawFlags.None, 2);
            }

            uint count = PartyManager.Instance.MemberCount;
            if (count < 1)
            {
                ImGui.End();
                return;
            }

            if (_layoutDirty || _size != maxSize || _memberCount != count)
            {
                _layoutInfo = LayoutHelper.CalculateLayout(
                    maxSize,
                    _healthBarsConfig.Size,
                    count,
                    _healthBarsConfig.Padding,
                    Config.FillRowsFirst
                );

                UpdateBars(contentStartPos);
            }
            else if (_origin != contentStartPos)
            {
                UpdateBarsPosition(contentStartPos - _origin);
            }

            _layoutDirty = false;
            _origin = contentStartPos;
            _memberCount = count;
            _size = maxSize;

            Dalamud.Game.ClientState.Objects.Types.GameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            int targetIndex = -1;
            int enmityLeaderIndex = -1;
            int enmitySecondIndex = -1;

            // bars
            for (int i = 0; i < count; i++)
            {
                IPartyFramesMember? member = _bars[i].Member;

                if (member != null)
                {
                    if (target != null && member.ObjectId == target.ObjectId)
                    {
                        targetIndex = i;
                        continue;
                    }

                    if (_healthBarsConfig.ColorsConfig.ShowEnmityBorderColors)
                    {
                        if (member.EnmityLevel == EnmityLevel.Leader)
                        {
                            enmityLeaderIndex = i;
                            continue;
                        }
                        else if (_healthBarsConfig.ColorsConfig.ShowSecondEnmity && member.EnmityLevel == EnmityLevel.Second &&
                            (count > 4 || !_healthBarsConfig.ColorsConfig.HideSecondEnmityInLightParties))
                        {
                            enmitySecondIndex = i;
                            continue;
                        }
                    }
                }

                _bars[i].Draw(origin, drawList);
            }

            // 2nd enmity
            if (enmitySecondIndex >= 0)
            {
                _bars[enmitySecondIndex].Draw(origin, drawList, _healthBarsConfig.ColorsConfig.EnmitySecondBordercolor);
            }

            // 1st enmity
            if (enmityLeaderIndex >= 0)
            {
                _bars[enmityLeaderIndex].Draw(origin, drawList, _healthBarsConfig.ColorsConfig.EnmityLeaderBordercolor);
            }

            // target
            if (targetIndex >= 0)
            {
                _bars[targetIndex].Draw(origin, drawList, _healthBarsConfig.ColorsConfig.TargetBordercolor);
            }

            ImGui.End();
        }
    }
}
