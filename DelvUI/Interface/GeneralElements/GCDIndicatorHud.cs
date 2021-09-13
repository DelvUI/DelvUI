using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class GCDIndicatorHud : HudElement, IHudElementWithActor
    {
        private GCDIndicatorConfig Config => (GCDIndicatorConfig)_config;
        public Actor Actor { get; set; } = null;

        public GCDIndicatorHud(string ID, GCDIndicatorConfig config) : base(ID, config)
        {
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

            var percentNonQueue = 1F - (500f / 1000f) / total;

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(startPos, size)
                .SetChunks(new float[2]{percentNonQueue, 1F - percentNonQueue})
                .AddInnerBar(elapsed, total, Config.Color.Map)
                .SetDrawBorder(Config.ShowBorder)
                .SetVertical(Config.VerticalMode);

            var queueStartOffset = Config.VerticalMode ? new Vector2(0, percentNonQueue * size.Y) : new Vector2(percentNonQueue * size.X, 0);
            var queueEndOffset = Config.VerticalMode ? new Vector2(size.X, percentNonQueue * size.Y + 1f) : new Vector2(percentNonQueue * size.X + 1f, size.Y);
            if (Config.ShowGCDQueueIndicator)
            {
                builder.SetChunksColors(new Dictionary<string, uint>[2]{Config.Color.Map, Config.QueueColor.Map});
                drawList.AddRect(startPos + queueStartOffset, startPos + queueEndOffset, Config.QueueColor.Base);
            }

            builder.Build().Draw(drawList);
        }
    }
}
