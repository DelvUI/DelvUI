using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using Dalamud.Bindings.ImGui;
using Newtonsoft.Json;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Disableable(false)]
    [Exportable(false)]
    [Section("Visibility")]
    [SubSection("Global", 0)]
    public class GlobalVisibilityConfig : PluginConfigObject
    {
        public new static GlobalVisibilityConfig DefaultConfig() { return new GlobalVisibilityConfig(); }

        [NestedConfig("Visibility", 50, collapsingHeader = false)]
        public VisibilityConfig VisibilityConfig = new VisibilityConfig();

        [JsonIgnore]
        private bool _applying = false;

        [ManualDraw]
        public bool Draw(ref bool changed)
        {
            ImGui.NewLine();

            if (ImGui.Button("Apply to all elements", new Vector2(200, 30)))
            {
                _applying = true;
            }

            if (_applying)
            {
                string[] lines = new string[] { "This will replace the visibility settings", "for ALL DelvUI elements!", "Are you sure?" };
                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal("Apply?", lines);

                if (didConfirm)
                {
                    ConfigurationManager.Instance.OnGlobalVisibilityChanged(VisibilityConfig);
                    changed = true;
                }

                if (didConfirm || didClose)
                {
                    _applying = false;
                }
            }

            return false;
        }
    }
}
