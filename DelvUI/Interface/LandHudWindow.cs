using Dalamud.Plugin;
using DelvUI.Config;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace DelvUI.Interface
{
    public class LandHudWindow : HudWindow
    {
        public LandHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) :
            base(pluginInterface, pluginConfiguration)
        {
            JobId = pluginInterface.ClientState.LocalPlayer.ClassJob.Id;
        }

        public override uint JobId { get; }

        protected override void Draw(bool _) { }

        protected override void DrawPrimaryResourceBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            base.DrawPrimaryResourceBar(PrimaryResourceType.GP);
        }
    }
}
