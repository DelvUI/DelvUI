using Dalamud.Plugin;
using DelvUI.Config;

namespace DelvUI.Interface {
    public class UnitFrameOnlyHudWindow : HudWindow
    {
        //public override uint JobId => 0;

        public UnitFrameOnlyHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) :
            base(pluginInterface, pluginConfiguration)
        {
            //  To prevent SwapJobs() from being spammed in Plugin.cs Draw()
            JobId = pluginInterface.ClientState.LocalPlayer.ClassJob.Id;
        }

        public override uint JobId { get; }

        protected override void Draw(bool _)
        {
        }
    }
}