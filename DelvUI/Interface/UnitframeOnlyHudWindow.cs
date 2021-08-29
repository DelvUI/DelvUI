using Dalamud.Plugin;

namespace DelvUI.Interface {
    public class UnitFrameOnlyHudWindow : HudWindow
    {
        public override uint JobId => 0;

        public UnitFrameOnlyHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
        }
    }
}