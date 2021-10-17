using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DelvUI.Interface.Party
{
    public class PartyFramesHud : DraggableHudElement, IHudElementWithMouseOver
    {
        private PartyFramesConfig Config => (PartyFramesConfig)_config;
        private PartyFramesHealthBarsConfig _healthBarsConfig;
        private PartyFramesRaiseTrackerConfig _raiseTrackerConfig;
        private PartyFramesInvulnTrackerConfig _invulnTrackerConfig;

        private delegate void OpenContextMenu(IntPtr agentHud, int parentAddonId, int index);
        private readonly OpenContextMenu _openContextMenu;

        private Vector2 _contentMargin = new Vector2(2, 2);
        private static readonly int MaxMemberCount = 9; // 8 players + chocobo

        // layout
        private Vector2 _origin;
        private Vector2 _size;
        private LayoutInfo _layoutInfo;
        private uint _memberCount = 0;
        private bool _layoutDirty = true;

        private readonly List<PartyFramesBar> bars;

        private bool Locked => !ConfigurationManager.Instance.DrawConfigWindow;


        public PartyFramesHud(PartyFramesConfig config, string displayName) : base(config, displayName)
        {
            _healthBarsConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesHealthBarsConfig>();
            _raiseTrackerConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesRaiseTrackerConfig>();
            _invulnTrackerConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesInvulnTrackerConfig>();

            var manaBarConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesManaBarConfig>();
            var castbarConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesCastbarConfig>();
            var roleIconConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesRoleIconConfig>();
            var leaderIconConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesLeaderIconConfig>();
            var buffsConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesBuffsConfig>();
            var debuffsConfig = ConfigurationManager.Instance.GetConfigObject<PartyFramesDebuffsConfig>();

            config.ValueChangeEvent += OnLayoutPropertyChanged;
            _healthBarsConfig.ValueChangeEvent += OnLayoutPropertyChanged;
            _healthBarsConfig.ColorsConfig.ValueChangeEvent += OnLayoutPropertyChanged;

            bars = new List<PartyFramesBar>(MaxMemberCount);
            for (int i = 0; i < bars.Capacity; i++)
            {
                var bar = new PartyFramesBar(
                    "DelvUI_partyFramesBar" + i,
                    _healthBarsConfig,
                    manaBarConfig,
                    castbarConfig,
                    roleIconConfig,
                    leaderIconConfig,
                    buffsConfig,
                    debuffsConfig,
                    _raiseTrackerConfig,
                    _invulnTrackerConfig
                );

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
            _healthBarsConfig.ValueChangeEvent -= OnLayoutPropertyChanged;
            _healthBarsConfig.ColorsConfig.ValueChangeEvent -= OnLayoutPropertyChanged;
            PartyManager.Instance.MembersChangedEvent -= OnMembersChanged;
        }

        private void OnMovePlayer(PartyFramesBar bar)
        {
            if (Config.PlayerOrderOverrideEnabled && bar.Member != null)
            {
                var offset = bar.Member.Order - 1 > Config.PlayerOrder ? -1 : -2;
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
            var memberCount = PartyManager.Instance.MemberCount;
            uint row = 0;
            uint col = 0;
            var spaceSize = Config.Size - _contentMargin * 2;

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
                CalculateBarPosition(origin, spaceSize, out var x, out var y);
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
            foreach (var bar in bars)
            {
                bar.Position = bar.Position + delta;
            }
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position + Config.Size / 2f }, new List<Vector2>() { Config.Size });
        }

        public void StopMouseover()
        {
            foreach (var bar in bars)
            {
                bar.StopMouseover();
            }
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!_config.Enabled)
            {
                return;
            }

            var windowFlags = ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoFocusOnAppearing |
                ImGuiWindowFlags.NoBringToFrontOnFocus;

            bool canDrag = !Locked && !DraggingEnabled;
            if (!canDrag)
            {
                windowFlags |= ImGuiWindowFlags.NoMove;
                windowFlags |= ImGuiWindowFlags.NoResize;
            }

            Action<ImDrawListPtr> drawBarsAction = (drawList) =>
            {
                var windowPos = ImGui.GetWindowPos();
                var windowSize = ImGui.GetWindowSize();
                Config.Size = windowSize;

                if (canDrag)
                {
                    Vector2 newPosition = windowPos - origin;
                    if (Config.Position != newPosition)
                    {
                        // have to flag it like this sadly
                        ConfigurationManager.Instance.ForceNeedsSave();
                        Config.Position = windowPos - origin;
                    }
                }

                var count = PartyManager.Instance.MemberCount;
                if (count < 1)
                {
                    return;
                }

                // recalculate layout on settings or size change
                var contentStartPos = windowPos + _contentMargin;
                var maxSize = windowSize - _contentMargin * 2;

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

                var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
                var targetIndex = -1;
                var enmityLeaderIndex = -1;
                var enmitySecondIndex = -1;
                List<int> raisedIndexes = new List<int>();

                // bars
                for (int i = 0; i < count; i++)
                {
                    var member = bars[i].Member;

                    if (member != null)
                    {
                        if (target != null && member.ObjectId == target.ObjectId)
                        {
                            targetIndex = i;
                            continue;
                        }

                        if (_raiseTrackerConfig.Enabled && _raiseTrackerConfig.ChangeBorderColorWhenRaised && member.RaiseTime.HasValue)
                        {
                            raisedIndexes.Add(i);
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

                    bars[i].Draw(origin, drawList);
                }

                // special colors for borders

                // 2nd enmity
                if (enmitySecondIndex >= 0)
                {
                    bars[enmitySecondIndex].Draw(origin, drawList, _healthBarsConfig.ColorsConfig.EnmitySecondBordercolor);
                }

                // 1st enmity
                if (enmityLeaderIndex >= 0)
                {
                    bars[enmityLeaderIndex].Draw(origin, drawList, _healthBarsConfig.ColorsConfig.EnmityLeaderBordercolor);
                }

                // raise
                foreach (int index in raisedIndexes)
                {
                    bars[index].Draw(origin, drawList, _raiseTrackerConfig.BorderColor);
                }

                // target
                if (targetIndex >= 0)
                {
                    bars[targetIndex].Draw(origin, drawList, _healthBarsConfig.ColorsConfig.TargetBordercolor);
                }
            };

            Action drawElementsAction = () =>
            {
                foreach (var bar in bars)
                {
                    bar.DrawElements(origin);
                }
            };

            // no clipping when unlocked, creates way too many issues
            if (canDrag)
            {
                // size and position
                ImGui.SetNextWindowPos(origin + Config.Position, ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSize(Config.Size + _contentMargin * 2, ImGuiCond.FirstUseEver);

                ImGui.PushStyleColor(ImGuiCol.Border, 0x66FFFFFF);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, 0x66000000);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);

                bool begin = ImGui.Begin(ID, windowFlags);
                if (!begin)
                {
                    ImGui.End();
                    return;
                }

                drawBarsAction(ImGui.GetWindowDrawList());
                ImGui.End();

                ImGui.PopStyleColor(2);
                ImGui.PopStyleVar(2);

                drawElementsAction();
            }
            else
            {
                windowFlags |= ImGuiWindowFlags.NoBackground;
                DrawHelper.DrawInWindow(ID, origin + Config.Position, Config.Size, !Locked, false, true, windowFlags, drawBarsAction);
                drawElementsAction();
            }
        }
    }
}
