using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.Internal.Gui.Addon;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.StatusEffects;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public abstract class HudWindow
    {
        protected readonly PluginConfiguration PluginConfiguration;
        protected readonly DalamudPluginInterface PluginInterface;
        public bool IsVisible = true;
        private readonly Vector2 Center = new(CenterX, CenterY);

        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;
        }

        public abstract uint JobId { get; }

        protected static float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected static float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected static int XOffset => 160;
        protected static int YOffset => 460;

        protected Vector2 BarSize { get; private set; }

        protected virtual void DrawPrimaryResourceBar()
        {
        }

        public void Draw()
        {
            return;
            if (!ShouldBeVisible() || PluginInterface.ClientState.LocalPlayer == null)
            {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            bool begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar
              | ImGuiWindowFlags.NoScrollbar
              | ImGuiWindowFlags.AlwaysAutoResize
              | ImGuiWindowFlags.NoBackground
              | ImGuiWindowFlags.NoInputs
              | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin)
            {
                return;
            }

            DrawGenericElements();

            Draw(true);

            ImGui.End();
        }

        protected void DrawGenericElements()
        {

        }

        protected abstract void Draw(bool _);

        protected virtual unsafe bool ShouldBeVisible()
        {
            return false;
        }

    }
}
