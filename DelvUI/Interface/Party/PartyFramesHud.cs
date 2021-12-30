using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface.Party
{
    public class PartyFramesHud : DraggableHudElement, IHudElementWithMouseOver, IHudElementWithPreview
    {
        private PartyFramesConfig Config => (PartyFramesConfig)_config;
        private readonly PartyFramesConfigs Configs;

        private delegate void OpenContextMenu(IntPtr agentHud, int parentAddonId, int index);
        private readonly OpenContextMenu _openContextMenu;

        private readonly Vector2 _contentMargin = new(2, 2);
        private const int MaxMemberCount = 9; // 8 players + chocobo

        // layout
        private Vector2 _origin;
        private LayoutInfo _layoutInfo;
        private uint _memberCount;
        private bool _layoutDirty = true;

        private readonly List<PartyFramesBar> bars;

        private bool Locked => !ConfigurationManager.Instance.IsConfigWindowOpened;


        public PartyFramesHud(PartyFramesConfig config, string displayName) : base(config, displayName)
        {
            Configs = PartyFramesConfigs.GetConfigs();

            config.ValueChangeEvent += OnLayoutPropertyChanged;
            Configs.HealthBar.ValueChangeEvent += OnLayoutPropertyChanged;
            Configs.HealthBar.ColorsConfig.ValueChangeEvent += OnLayoutPropertyChanged;

            bars = new List<PartyFramesBar>(MaxMemberCount);
            for (int i = 0; i < bars.Capacity; i++)
            {
                PartyFramesBar bar = new("DelvUI_partyFramesBar" + i, Configs);
                bar.MovePlayerEvent += OnMovePlayer;
                bar.OpenContextMenuEvent += OnOpenContextMenu;

                bars.Add(bar);
            }

            PartyManager.Instance.MembersChangedEvent += OnMembersChanged;
            UpdateBars(Vector2.Zero);

            /*
             Part of openContextMenu disassembly signature
            .text:00007FF648519790                   OpenPartyContextMenu proc near
            .text:00007FF648519790
            .text:00007FF648519790                   arg_0= qword ptr  8
            .text:00007FF648519790                   arg_8= qword ptr  10h
            .text:00007FF648519790                   arg_10= qword ptr  18h
            .text:00007FF648519790
            .text:00007FF648519790 48 89 5C 24 10    mov     [rsp+arg_8], rbx
            .text:00007FF648519795 48 89 6C 24 18    mov     [rsp+arg_10], rbp
            .text:00007FF64851979A 57                push    rdi
            .text:00007FF64851979B 48 83 EC 20       sub     rsp, 20h
            .text:00007FF64851979F 49 63 D8          movsxd  rbx, r8d
            .text:00007FF6485197A2 8B EA             mov     ebp, edx
            .text:00007FF6485197A4 48 8B F9          mov     rdi, rcx
            .text:00007FF6485197A7 E8 74 BC 86 FF    call    sub_7FF647D85420
            .text:00007FF6485197AC 84 C0             test    al, al
            .text:00007FF6485197AE 0F 85 DF 00 00 00 jnz     loc_7FF648519893
            */
            _openContextMenu =
                Marshal.GetDelegateForFunctionPointer<OpenContextMenu>(Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 50 83 FB 01"));
        }

        protected override void InternalDispose()
        {
            bars.Clear();

            _config.ValueChangeEvent -= OnLayoutPropertyChanged;
            Configs.HealthBar.ValueChangeEvent -= OnLayoutPropertyChanged;
            Configs.HealthBar.ColorsConfig.ValueChangeEvent -= OnLayoutPropertyChanged;
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

        private unsafe void OnOpenContextMenu(PartyFramesBar bar)
        {
            if (bar.Member == null || Plugin.ClientState.LocalPlayer == null)
            {
                return;
            }

            if (PartyManager.Instance.PartyListAddon == null || PartyManager.Instance.HudAgent == IntPtr.Zero)
            {
                return;
            }

            int addonId = PartyManager.Instance.PartyListAddon->AtkUnitBase.ID;
            int index = bar.Member.Character?.ObjectId == Plugin.ClientState.LocalPlayer.ObjectId ? 0 : bar.Member.Order - 1;

            _openContextMenu.Invoke(PartyManager.Instance.HudAgent, addonId, index);
        }

        private void OnLayoutPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName is "Size" or "FillRowsFirst" or "BarsAnchor" or "Padding" or "Rows" or "Columns")
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

            for (int i = 0; i < bars.Count; i++)
            {
                PartyFramesBar bar = bars[i];
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
                CalculateBarPosition(origin, Size, out var x, out var y);
                bar.Position = new Vector2(
                    x + Configs.HealthBar.Size.X * col + Configs.HealthBar.Padding.X * col,
                    y + Configs.HealthBar.Size.Y * row + Configs.HealthBar.Padding.Y * row
                );

                // layout
                if (Config.FillRowsFirst)
                {
                    col += 1;
                    if (col >= _layoutInfo.TotalColCount)
                    {
                        col = 0;
                        row += 1;
                    }
                }
                else
                {
                    row += 1;
                    if (row >= _layoutInfo.TotalRowCount)
                    {
                        row = 0;
                        col += 1;
                    }
                }
            }
        }

        private void CalculateBarPosition(Vector2 position, Vector2 spaceSize, out float x, out float y)
        {
            x = position.X;
            y = position.Y;

            switch (Config.BarsAnchor)
            {
                case DrawAnchor.Top or DrawAnchor.Center or DrawAnchor.Bottom:
                    x += (spaceSize.X - _layoutInfo.ContentSize.X) / 2f;
                    break;
                case DrawAnchor.TopRight or DrawAnchor.Right or DrawAnchor.BottomRight:
                    x += spaceSize.X - _layoutInfo.ContentSize.X;
                    break;
            }

            switch (Config.BarsAnchor)
            {
                case DrawAnchor.Left or DrawAnchor.Center or DrawAnchor.Right:
                    y += (spaceSize.Y - _layoutInfo.ContentSize.Y) / 2f;
                    break;
                case DrawAnchor.BottomLeft or DrawAnchor.Bottom or DrawAnchor.BottomRight:
                    y += spaceSize.Y - _layoutInfo.ContentSize.Y;
                    break;
            }
        }

        private void UpdateBarsPosition(Vector2 delta)
        {
            foreach (PartyFramesBar bar in bars)
            {
                bar.Position += delta;
            }
        }

        public void StopPreview()
        {
            Config.Preview = false;
            PartyManager.Instance?.UpdatePreview();

            foreach (PartyFramesBar bar in bars)
            {
                bar.StopPreview();
            }
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes() => (new List<Vector2> { Config.Position + Size / 2f }, new List<Vector2> { Size });

        public void StopMouseover()
        {
            foreach (PartyFramesBar bar in bars)
            {
                bar.StopMouseover();
            }
        }

        private Vector2 Size => new(
            Config.Columns * Configs.HealthBar.Size.X + (Config.Columns - 1) * Configs.HealthBar.Padding.X,
            Config.Rows * Configs.HealthBar.Size.Y + (Config.Rows - 1) * Configs.HealthBar.Padding.Y
        );

        public override void DrawChildren(Vector2 origin)
        {
            if (!_config.Enabled)
            {
                return;
            }

            Vector2 contentStartPos = origin + Config.Position;

            uint count = PartyManager.Instance.MemberCount;
            if (count < 1)
            {
                return;
            }

            // recalculate layout on settings or size change
            if (_layoutDirty || _memberCount != count)
            {
                _layoutInfo = LayoutHelper.CalculateLayout(
                    Size,
                    Configs.HealthBar.Size,
                    count,
                    Configs.HealthBar.Padding,
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

            GameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            int targetIndex = -1;
            int enmityLeaderIndex = -1;
            int enmitySecondIndex = -1;
            List<int> raisedIndexes = new();
            List<int> cleanseIndexes = new();

            // bars
            for (int i = 0; i < count; i++)
            {
                IPartyFramesMember? member = bars[i].Member;

                if (member != null)
                {
                    if (target != null && member.ObjectId == target.ObjectId)
                    {
                        targetIndex = i;
                        continue;
                    }

                    bool cleanseCheck = true;
                    if (Configs.Trackers.Cleanse.CleanseJobsOnly)
                    {
                        cleanseCheck = Utils.IsOnCleanseJob();
                    }

                    if (Configs.Trackers.Cleanse.Enabled && Configs.Trackers.Cleanse.ChangeBorderCleanseColor && member.HasDispellableDebuff && cleanseCheck)
                    {
                        cleanseIndexes.Add(i);
                        continue;
                    }

                    if (Configs.Trackers.Raise.Enabled && Configs.Trackers.Raise.ChangeBorderColorWhenRaised && member.RaiseTime.HasValue)
                    {
                        raisedIndexes.Add(i);
                        continue;
                    }

                    if (Configs.HealthBar.ColorsConfig.ShowEnmityBorderColors)
                    {
                        if (member.EnmityLevel == EnmityLevel.Leader)
                        {
                            enmityLeaderIndex = i;
                            continue;
                        }

                        if (Configs.HealthBar.ColorsConfig.ShowSecondEnmity && member.EnmityLevel == EnmityLevel.Second &&
                            (count > 4 || !Configs.HealthBar.ColorsConfig.HideSecondEnmityInLightParties))
                        {
                            enmitySecondIndex = i;
                            continue;
                        }
                    }
                }

                bars[i].Draw(origin);
            }

            // special colors for borders

            // 2nd enmity
            if (enmitySecondIndex >= 0)
            {
                bars[enmitySecondIndex].Draw(origin, Configs.HealthBar.ColorsConfig.EnmitySecondBordercolor);
            }

            // 1st enmity
            if (enmityLeaderIndex >= 0)
            {
                bars[enmityLeaderIndex].Draw(origin, Configs.HealthBar.ColorsConfig.EnmityLeaderBordercolor);
            }

            // raise
            foreach (int index in raisedIndexes)
            {
                bars[index].Draw(origin, Configs.Trackers.Raise.BorderColor);
            }

            // target
            if (targetIndex >= 0)
            {
                bars[targetIndex].Draw(origin, Configs.HealthBar.ColorsConfig.TargetBordercolor);
            }

            // cleanseable debuff
            foreach (int index in cleanseIndexes)
            {
                bars[index].Draw(origin, Configs.Trackers.Cleanse.BorderColor);
            }

            AddDrawAction(Config.StrataLevel, () =>
            {
                ImGui.PushStyleColor(ImGuiCol.Border, 0x66FFFFFF);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, 0x66000000);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
                
                ImGui.PopStyleColor(2);
                ImGui.PopStyleVar(2);
            });

            foreach (PartyFramesBar bar in bars)
            {
                AddDrawActions(bar.GetElementsDrawActions(origin));
            }
        }
    }

    #region utils
    public struct PartyFramesConfigs
    {
        public PartyFramesHealthBarsConfig HealthBar;
        public PartyFramesManaBarConfig ManaBar;
        public PartyFramesCastbarConfig CastBar;
        public PartyFramesIconsConfig Icons;
        public PartyFramesBuffsConfig Buffs;
        public PartyFramesDebuffsConfig Debuffs;
        public PartyFramesTrackersConfig Trackers;

        public PartyFramesConfigs(
            PartyFramesHealthBarsConfig healthBar,
            PartyFramesManaBarConfig manaBar,
            PartyFramesCastbarConfig castBar,
            PartyFramesIconsConfig icons,
            PartyFramesBuffsConfig buffs,
            PartyFramesDebuffsConfig debuffs,
            PartyFramesTrackersConfig trackers)
        {
            HealthBar = healthBar;
            ManaBar = manaBar;
            CastBar = castBar;
            Icons = icons;
            Buffs = buffs;
            Debuffs = debuffs;
            Trackers = trackers;
        }

        public static PartyFramesConfigs GetConfigs() =>
            new(
                ConfigurationManager.Instance.GetConfigObject<PartyFramesHealthBarsConfig>(),
                ConfigurationManager.Instance.GetConfigObject<PartyFramesManaBarConfig>(),
                ConfigurationManager.Instance.GetConfigObject<PartyFramesCastbarConfig>(),
                ConfigurationManager.Instance.GetConfigObject<PartyFramesIconsConfig>(),
                ConfigurationManager.Instance.GetConfigObject<PartyFramesBuffsConfig>(),
                ConfigurationManager.Instance.GetConfigObject<PartyFramesDebuffsConfig>(),
                ConfigurationManager.Instance.GetConfigObject<PartyFramesTrackersConfig>()
            );
    }
    #endregion
}
