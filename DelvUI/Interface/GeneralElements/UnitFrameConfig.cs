using DelvUI.Config;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    public class UnitFrameConfig : MovablePluginConfigObject
    {
        public bool UseCustomColor = false;
        public PluginConfigColor CustomColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        public bool UseCustomBackgroundColor = false;
        public PluginConfigColor CustomBackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        public LabelConfig LeftLabelConfig;
        public LabelConfig RightLabelConfig;
        public ShieldConfig ShieldConfig = new ShieldConfig();
        public TankStanceIndicatorConfig TankStanceIndicatorConfig = null;
        public bool ShowTankInvulnerability = true;

        [JsonIgnore] public string Title;

        public UnitFrameConfig(Vector2 position, Vector2 size, LabelConfig leftLabelConfig, LabelConfig rightLabelConfig, string title = null)
        {
            Position = position;
            Size = size;
            LeftLabelConfig = leftLabelConfig;
            RightLabelConfig = rightLabelConfig;
        }
    }

    [Serializable]
    public class ShieldConfig : PluginConfigObject
    {
        public int Height = 10;
        public bool HeightInPixels = false;
        public bool FillHealthFirst = true;
        public PluginConfigColor Color = new PluginConfigColor(new Vector4(198f / 255f, 210f / 255f, 255f / 255f, 70f / 100f));
    }

    [Serializable]
    public class TankStanceIndicatorConfig : PluginConfigObject
    {
        public int Thickness = 2;
        public Vector4 ActiveColor = new Vector4(0f / 255f, 205f / 255f, 230f / 255f, 100f / 100f);
        public Vector4 UnactiveColor = new Vector4(255f / 255f, 0f / 255f, 32f / 255f, 100f / 100f);
    }
}
