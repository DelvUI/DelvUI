using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class GCDIndicatorHud : HudElement, IHudElementWithActor
    {
        private PluginConfiguration _pluginConfiguration;
        private GCDIndicatorConfig Config => (GCDIndicatorConfig)_config;
        public Actor Actor { get; set; } = null;

        public GCDIndicatorHud(string ID, GCDIndicatorConfig config, PluginConfiguration pluginConfiguration) : base(ID, config)
        {
            // NOTE: Temporary. Have to do this for now to use the bar builder.
            // Ideally hud elements shouldna't need a reference to PluginConfiguration
            _pluginConfiguration = pluginConfiguration;
        }

        public override void Draw(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null || Actor is not PlayerCharacter)
            {
                return;
            }

            GCDHelper.GetGCDInfo((PlayerCharacter)Actor, out var elapsed, out var total);

            if (!Config.AlwaysShow && total == 0)
            {
                return;
            }

            var scale = elapsed / total;
            if (scale <= 0)
            {
                return;
            }

            var startPos = origin + Config.Position - Config.Size / 2f;
            var size = !Config.VerticalMode ? Config.Size : new Vector2(Config.Size.Y, -Config.Size.X);

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(startPos, size)
                .AddInnerBar(elapsed, total, Config.Color.Map)
                .SetDrawBorder(Config.ShowBorder)
                .SetVertical(Config.VerticalMode);

            builder.Build().Draw(drawList, _pluginConfiguration);
        }
    }
}
