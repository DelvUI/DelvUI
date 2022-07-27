using DelvUI.Config;
using DelvUI.Config.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelvUI.Interface.GeneralElements
{
    [Disableable(false)]
    [Exportable(false)]
    [Section("Visibility")]
    [SubSection("Hotbars", 0)]
    public class HotbarsVisibilityConfig : PluginConfigObject
    {
        public new static HotbarsVisibilityConfig DefaultConfig() { return new HotbarsVisibilityConfig(); }

        [NestedConfig("Hotbar 1", 50)]
        public VisibilityConfig HotbarConfig1 = new VisibilityConfig();

        [NestedConfig("Hotbar 2", 51)]
        public VisibilityConfig HotbarConfig2 = new VisibilityConfig();

        [NestedConfig("Hotbar 3", 52)]
        public VisibilityConfig HotbarConfig3 = new VisibilityConfig();

        [NestedConfig("Hotbar 4", 53)]
        public VisibilityConfig HotbarConfig4 = new VisibilityConfig();

        [NestedConfig("Hotbar 5", 54)]
        public VisibilityConfig HotbarConfig5 = new VisibilityConfig();

        [NestedConfig("Hotbar 6", 55)]
        public VisibilityConfig HotbarConfig6 = new VisibilityConfig();

        [NestedConfig("Hotbar 7", 56)]
        public VisibilityConfig HotbarConfig7 = new VisibilityConfig();

        [NestedConfig("Hotbar 8", 57)]
        public VisibilityConfig HotbarConfig8 = new VisibilityConfig();

        [NestedConfig("Hotbar 9", 58)]
        public VisibilityConfig HotbarConfig9 = new VisibilityConfig();

        [NestedConfig("Hotbar 10", 59)]
        public VisibilityConfig HotbarConfig10 = new VisibilityConfig();

        [NestedConfig("Cross Hotbar", 60)]
        public VisibilityConfig HotbarConfigCross = new VisibilityConfig();

        private List<VisibilityConfig> _configs;
        public List<VisibilityConfig> GetHotbarConfigs() => _configs;

        public HotbarsVisibilityConfig()
        {
            _configs = new List<VisibilityConfig>() {
                HotbarConfig1,
                HotbarConfig2,
                HotbarConfig3,
                HotbarConfig4,
                HotbarConfig5,
                HotbarConfig6,
                HotbarConfig7,
                HotbarConfig8,
                HotbarConfig9,
                HotbarConfig10
            };
        }
    }
}
