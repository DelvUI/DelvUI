using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Data;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUIPlugin.GameStructs;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ImGuiScene;
using Lumina.Data.Files;
using Lumina.Excel.GeneratedSheets;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUIPlugin.Interface {
    
    public abstract class HudWindow {
        public bool IsVisible = true;
        protected readonly DalamudPluginInterface PluginInterface;
        protected readonly PluginConfiguration PluginConfiguration;
        private Vector2 _barsize;

        public abstract uint JobId { get; }

        protected float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected int XOffset => 160;
        protected int YOffset => 460;
        protected int HealthBarHeight => PluginConfiguration.HealthBarHeight;
        protected int HealthBarWidth => PluginConfiguration.HealthBarWidth;
        protected int PrimaryResourceBarHeight => PluginConfiguration.PrimaryResourceBarHeight;
        protected int PrimaryResourceBarWidth => PluginConfiguration.PrimaryResourceBarWidth;
        protected int TargetBarHeight => PluginConfiguration.TargetBarHeight;
        protected int TargetBarWidth => PluginConfiguration.TargetBarWidth;
        protected int ToTBarHeight => PluginConfiguration.ToTBarHeight;
        protected int ToTBarWidth => PluginConfiguration.ToTBarWidth;        
        protected int FocusBarHeight => PluginConfiguration.FocusBarHeight;
        protected int FocusBarWidth => PluginConfiguration.FocusBarWidth;
        protected int CastBarWidth => PluginConfiguration.CastBarWidth;
        protected int CastBarHeight => PluginConfiguration.CastBarHeight;
        protected int CastBarXOffset => PluginConfiguration.CastBarXOffset;
        protected int CastBarYOffset => PluginConfiguration.CastBarYOffset;
        protected Vector2 BarSize => _barsize;

        private Lumina.Excel.GeneratedSheets.Action LastUsedAction;
        private Mount LastUsedMount;
        private Item LastUsedItem;
        
        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;
            //_barsize = new Vector2(BarWidth, BarHeight);
        }

        protected virtual void DrawHealthBar() {
            _barsize = new Vector2(HealthBarWidth, HealthBarHeight);
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentHp / actor.MaxHp;
            
            if(actor.ClassJob.Id == 19 || actor.ClassJob.Id == 32 || actor.ClassJob.Id == 21 || actor.ClassJob.Id == 37)
                DrawTankStanceIndicator();

           
            var cursorPos = new Vector2(CenterX - HealthBarWidth - XOffset, CenterY + YOffset);
            DrawOutlinedText($"{actor.Name.Abbreviate().Truncate(16)}", new Vector2(cursorPos.X + 5, cursorPos.Y -22));
            
            var hp = $"{actor.MaxHp.KiloFormat(),6} | ";
            var hpSize = ImGui.CalcTextSize(hp);
            var percentageSize = ImGui.CalcTextSize("100");
            DrawOutlinedText(hp, new Vector2(cursorPos.X + HealthBarWidth - hpSize.X - percentageSize.X - 5, cursorPos.Y -22));
            DrawOutlinedText($"{(int)(scale * 100),3}", new Vector2(cursorPos.X + HealthBarWidth - percentageSize.X - 5, cursorPos.Y -22));
            
            ImGui.SetCursorPos(cursorPos);
            
            if (ImGui.BeginChild("health_bar", BarSize)) {
                var colors = PluginConfiguration.JobColorMap[PluginInterface.ClientState.LocalPlayer.ClassJob.Id];
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, colors["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(HealthBarWidth * scale, HealthBarHeight), 
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                if (ImGui.IsItemClicked()) {
                    PluginInterface.ClientState.Targets.SetCurrentTarget(actor);
                }
                
            }
            
            ImGui.EndChild();
        }

        protected virtual void DrawPrimaryResourceBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentMp / actor.MaxMp;
            _barsize = new Vector2(PrimaryResourceBarWidth, PrimaryResourceBarHeight);
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 27);
            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(BarSize.X * scale, BarSize.Y), 
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
        }
        
        protected virtual void DrawTargetBar() {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is null) {
                return;
            }

            _barsize = new Vector2(TargetBarWidth, TargetBarHeight);

            var cursorPos = new Vector2(CenterX + XOffset, CenterY + YOffset);
            ImGui.SetCursorPos(cursorPos);
            var drawList = ImGui.GetWindowDrawList();

            if (!(target is Chara actor)) {
                var friendly = PluginConfiguration.NPCColorMap["friendly"];
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, friendly["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(TargetBarWidth, TargetBarHeight), 
                    friendly["gradientLeft"], friendly["gradientRight"], friendly["gradientRight"], friendly["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            }
            else {
                var scale = actor.MaxHp > 0f ? (float) actor.CurrentHp / actor.MaxHp : 0f;
                var colors = DetermineTargetPlateColors(actor);
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, colors["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(TargetBarWidth * scale, TargetBarHeight), 
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                var percentage = $"{(int) (scale * 100),3}";
                var percentageSize = ImGui.CalcTextSize(percentage);
                var maxPercentageSize = ImGui.CalcTextSize("100");
                DrawOutlinedText(percentage, new Vector2(cursorPos.X + 5 + maxPercentageSize.X - percentageSize.X, cursorPos.Y - 22));
                DrawOutlinedText($" | {actor.MaxHp.KiloFormat(),-6}", new Vector2(cursorPos.X + 5 + maxPercentageSize.X, cursorPos.Y - 22));
            }

            var name = $"{target.Name.Abbreviate().Truncate(16)}";
            var nameSize = ImGui.CalcTextSize(name);
            DrawOutlinedText(name, new Vector2(cursorPos.X + TargetBarWidth - nameSize.X - 5, cursorPos.Y - 22));

            DrawTargetOfTargetBar(target.TargetActorID);
        }
        protected virtual void DrawFocusBar() {
            var focus = PluginInterface.ClientState.Targets.FocusTarget;
            if (focus is null) {
                return;
            }
            var barSize = new Vector2(FocusBarWidth, FocusBarHeight);
            
            var cursorPos = new Vector2(CenterX - XOffset - HealthBarWidth - FocusBarWidth-2, CenterY + YOffset);
            ImGui.SetCursorPos(cursorPos);  
            var drawList = ImGui.GetWindowDrawList();
            
            if (!(focus is Chara actor)) {
                var friendly = PluginConfiguration.NPCColorMap["friendly"];
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, friendly["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(FocusBarWidth, FocusBarHeight), 
                    friendly["gradientLeft"], friendly["gradientRight"], friendly["gradientRight"], friendly["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            else
            {
                var colors = DetermineTargetPlateColors(actor);
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, colors["background"]);
                
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((float)FocusBarWidth * actor.CurrentHp / actor.MaxHp, FocusBarHeight), 
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            
            var name = $"{focus.Name.Abbreviate().Truncate(12)}";
            var textSize = ImGui.CalcTextSize(name);
            DrawOutlinedText(name, new Vector2(cursorPos.X + FocusBarWidth / 2f - textSize.X / 2f, cursorPos.Y - 22));

            
        }
        
        protected virtual void DrawTargetOfTargetBar(int targetActorId) {
            Actor target = null;
            
            for (var i = 0; i < 200; i += 2) {
                if (PluginInterface.ClientState.Actors[i]?.ActorId == targetActorId) {
                    target = PluginInterface.ClientState.Actors[i];
                }
            }
            
            if (!(target is Chara actor)) {
                return;
            }


            var barSize = new Vector2(ToTBarWidth, ToTBarHeight);

            var name = $"{actor.Name.Abbreviate().Truncate(12)}";
            var textSize = ImGui.CalcTextSize(name);

            var cursorPos = new Vector2(CenterX + XOffset + TargetBarWidth + 2, CenterY + YOffset);
            DrawOutlinedText(name, new Vector2(cursorPos.X + ToTBarWidth / 2f - textSize.X / 2f, cursorPos.Y - 22));
            ImGui.SetCursorPos(cursorPos);    
            
            var colors = DetermineTargetPlateColors(actor);
            if (ImGui.BeginChild("target_bar", barSize)) {
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, colors["background"]);
                
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((float)ToTBarWidth * actor.CurrentHp / actor.MaxHp, ToTBarHeight), 
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                
                if (ImGui.IsItemClicked()) {
                    PluginInterface.ClientState.Targets.SetCurrentTarget(target);
                }
            }
            
            ImGui.EndChild();
        }

        protected virtual unsafe void DrawCastBar()
        {
            if (! PluginConfiguration.ShowCastBar)
              return;

            var actor = PluginInterface.ClientState.LocalPlayer;
            var castBar = (AddonCastBar*) PluginInterface.Framework.Gui.GetUiObjectByName("_CastBar", 1);

            var castScale = castBar->CastPercent / 100;

            var castText = "Interrupted";
            var iconTexFile = PluginInterface.Data.GetIcon(0);
            if (!CastIsInterrupted(castBar))
            {
                // GameObject.CurrentCastId (for 6.0)
                var currentCastId = GetCurrentCast(actor.Address);
                var currentCastType = GetCurrentCastType(actor.Address);
                
                switch (currentCastType)
                {
                    case 0:
                        return;
                    case 1:
                    {
                        var currentAction = PluginInterface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()
                            .GetRow(currentCastId);
                        if (currentAction.Name != "") LastUsedAction = currentAction;
                        castText = LastUsedAction.Name;
                        iconTexFile = PluginInterface.Data.GetIcon(LastUsedAction.Icon);
                        break;
                    }
                    case 13:
                    {
                        var currentMount = PluginInterface.Data.GetExcelSheet<Mount>()
                            .GetRow(currentCastId);
                        LastUsedMount = currentMount;
                        castText = LastUsedMount.Singular;
                        iconTexFile = PluginInterface.Data.GetIcon(LastUsedMount.Icon);
                        break;
                    }
                    case 2:
                    {
                        var currentItem = PluginInterface.Data.GetExcelSheet<Item>()
                            .GetRow(currentCastId);
                        LastUsedItem = currentItem;
                        castText = "Using Item...";
                        iconTexFile = PluginInterface.Data.GetIcon(LastUsedItem.Icon);
                        break;
                    }                        
                    case 4:
                    {
                        castText = "Interacting...";
                        break;
                    }                    
                    default:
                    {
                        castText = "Casting...";
                        break;
                    }
                }
            }

            var castTime = Math.Round((castBar->CastTime - castBar->CastTime * castScale) / 100, 1)
                .ToString(CultureInfo.InvariantCulture);

            var barSize = new Vector2(CastBarWidth, CastBarHeight);
            var cursorPos = new Vector2(
                CenterX + PluginConfiguration.CastBarXOffset - CastBarWidth / 2f,
                CenterY + PluginConfiguration.CastBarYOffset
            );

            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (PluginConfiguration.SlideCast)
            {
                var slideColor = PluginConfiguration.CastBarColorMap["slidecast"];
                var slideCastScale = PluginConfiguration.SlideCastTime / 10f / castBar->CastTime;
                // Slide Cast
                drawList.AddRectFilledMultiColor(
                    cursorPos + barSize - new Vector2(barSize.X * slideCastScale, barSize.Y), cursorPos + barSize,
                    slideColor["gradientLeft"], slideColor["gradientRight"], slideColor["gradientRight"],
                    slideColor["gradientLeft"]
                );
            }

            var castColor = PluginConfiguration.CastBarColorMap["castbar"];
            // Actual Cast
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * castScale, barSize.Y),
                castColor["gradientLeft"], castColor["gradientRight"], castColor["gradientRight"],
                castColor["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            var emptyIconPath = "ui/icon/000000/000000.tex";
            // Action Icon
            if (PluginConfiguration.ShowActionIcon && iconTexFile.FilePath.Path != emptyIconPath)
            {
                var texture = PluginInterface.UiBuilder.LoadImageRaw(iconTexFile.GetRgbaImageData(), iconTexFile.Header.Width, iconTexFile.Header.Height, 4);
            
                ImGui.Image(texture.ImGuiHandle, new Vector2(CastBarHeight, CastBarHeight));
                drawList.AddRect(cursorPos, cursorPos + new Vector2(CastBarHeight, CastBarHeight), 0xFF000000);
            }
            
            var castTextSize = ImGui.CalcTextSize(castText);
            var castTimeTextSize = ImGui.CalcTextSize(castTime);
            
            if (PluginConfiguration.ShowCastTime) DrawOutlinedText(castTime, 
                new Vector2(cursorPos.X + CastBarWidth - castTimeTextSize.X - 5, cursorPos.Y + CastBarHeight / 2f - castTimeTextSize.Y / 2f));
            if (PluginConfiguration.ShowActionName) DrawOutlinedText(castText, 
                new Vector2(cursorPos.X + (PluginConfiguration.ShowActionIcon && iconTexFile.FilePath.Path != emptyIconPath ? CastBarHeight : 0) + 5, 
                cursorPos.Y + CastBarHeight / 2f - castTextSize.Y / 2f));
        }

        protected virtual void DrawTankStanceIndicator()
        {
            var tankStanceBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => 
                o.EffectId == 79 || //IRON WILL
                o.EffectId == 91 || //DEFIANCE
                o.EffectId == 392 || // ROYAL GUARD
                o.EffectId == 393 || //IRON WILL
                o.EffectId == 743 || //GRIT
                o.EffectId == 1396 || //DEFIANCE
                o.EffectId == 1397 || //GRIT
                o.EffectId == 1833 //ROYAL GUARD
            );

            if (tankStanceBuff.Count() != 1)
            {
                var barSize = new Vector2(HealthBarHeight>HealthBarWidth?HealthBarWidth:HealthBarHeight, HealthBarHeight);
                var cursorPos = new Vector2(CenterX - HealthBarWidth - XOffset - 5, CenterY + YOffset + 5);
                ImGui.SetCursorPos(cursorPos);  
                var drawList = ImGui.GetWindowDrawList();
            
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize, 
                    0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            else
            {
                var barSize = new Vector2(HealthBarHeight>HealthBarWidth?HealthBarWidth:HealthBarHeight, HealthBarHeight);
                var cursorPos = new Vector2(CenterX - HealthBarWidth - XOffset - 5, CenterY + YOffset + 5);
                ImGui.SetCursorPos(cursorPos);  
                var drawList = ImGui.GetWindowDrawList();
            
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize, 
                    0xFFE6CD00, 0xFFE6CD00, 0xFFE6CD00, 0xFFE6CD00
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }


        }

        protected Dictionary<string, uint> DetermineTargetPlateColors(Chara actor) {
            var colors = PluginConfiguration.NPCColorMap["neutral"];
            
            // Still need to figure out the "orange" state; aggroed but not yet attacked.
            switch (actor.ObjectKind) {
                case ObjectKind.Player:
                    colors = PluginConfiguration.JobColorMap[actor.ClassJob.Id];
                    break;

                case ObjectKind.BattleNpc when (actor.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat:
                    colors = PluginConfiguration.NPCColorMap["hostile"];
                    break;

                case ObjectKind.BattleNpc:
                {
                    if (!IsHostileMemory((BattleNpc)actor)) {
                        colors = PluginConfiguration.NPCColorMap["friendly"];
                    }

                    break;
                }
            }

            return colors;
        }

        protected void DrawOutlinedText(string text, Vector2 pos) {
            DrawOutlinedText(text, pos, Vector4.One, new Vector4(0f, 0f, 0f, 1f));
        }
        
        protected void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor) {
            ImGui.SetCursorPos(new Vector2(pos.X - 1, pos.Y + 1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y+1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y+1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X-1, pos.Y));
            ImGui.TextColored(outlineColor, text);

            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X-1, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X+1, pos.Y-1));
            ImGui.TextColored(outlineColor, text);
                
            ImGui.SetCursorPos(new Vector2(pos.X, pos.Y));
            ImGui.TextColored(color, text);
        }
        
        public void Draw() {
            if (!ShouldBeVisible() || PluginInterface.ClientState.LocalPlayer == null) {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
            
            var begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | 
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin) {
                return;
            }

            Draw(true);
            
            ImGui.End();
        }
        
        protected abstract void Draw(bool _);

        protected virtual unsafe bool ShouldBeVisible() {

            if (PluginConfiguration.HideHud)
            {
                return false;
            }

            if (IsVisible)
            {
                return true;
            }

            var parameterWidget = (AtkUnitBase*) PluginInterface.Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*) PluginInterface.Framework.Gui.GetUiObjectByName("FadeMiddle", 1);
            
            // Display HUD only if parameter widget is visible and we're not in a fade event
            return PluginInterface.ClientState.LocalPlayer == null || parameterWidget == null || fadeMiddleWidget == null || !parameterWidget->IsVisible || fadeMiddleWidget->IsVisible;
        }
        
        private ushort GetCurrentCast(IntPtr actor)
        {
            return (ushort) Marshal.ReadInt16(actor, ActorOffsets.CurrentCastSpellActionId);
        }

        private ushort GetCurrentCastType(IntPtr actor)
        {
            return (ushort) Marshal.ReadInt16(actor, 0x1B82);
            //[FieldOffset(0x1B82)] public ushort CastType; // Mounts = 6 or 9, Regular = 1
        }
        
        private bool IsCasting(IntPtr actor)
        {
            return Marshal.ReadInt16(actor, ActorOffsets.IsCasting) > 0;
        }

        private unsafe bool CastIsInterrupted(AddonCastBar* castBar)
        {
            for (var i = 0; i != castBar->AtkUnitBase.UldManager.NodeListCount; ++i)
            {
                var node = castBar->AtkUnitBase.UldManager.NodeList[i];
                // ReSharper disable once InvertIf
                if (node->NodeID == 2 && node->IsVisible) // Interrupted text node
                {
                    return true;
                }
            }

            return false;
        }
        
        unsafe bool IsHostileMemory(BattleNpc npc)
        {
            return (npc.BattleNpcKind == BattleNpcSubKind.Enemy || (int)npc.BattleNpcKind == 1) 
                   && *(byte*)(npc.Address + 0x1980) != 0 
                   && *(byte*)(npc.Address + 0x193C) != 1;
        }
    }
}