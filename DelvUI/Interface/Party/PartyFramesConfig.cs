using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.Party
{
    [Serializable]
    [Section("Party Frames")]
    [SubSection("General", 0)]
    public class PartyFramesConfig : MovablePluginConfigObject
    {
        public new static PartyFramesConfig DefaultConfig() { return new PartyFramesConfig(); }

        [DragInt2("Size", isMonitored = true)]
        [Order(30)]
        public Vector2 Size = new Vector2(650, 150);

        [Checkbox("Lock")]
        [Order(35)]
        public bool Lock = true;

        [Checkbox("Preview", isMonitored = true)]
        [Order(40)]
        public bool Preview = false;

        [Checkbox("Fill Rows First", isMonitored = true)]
        [Order(45)]
        public bool FillRowsFirst = true;

        [Combo("Sorting Mode",
            "Tank => DPS => Healer",
            "Tank => Healer => DPS",
            "DPS => Tank => Healer",
            "DPS => Healer => Tank",
            "Healer => Tank => DPS",
            "Healer => DPS => Tank",
            isMonitored = true
        )]
        [Order(50)]
        public PartySortingMode SortingMode = PartySortingMode.Tank_Healer_DPS;

        [Anchor("Bars Anchor", isMonitored = true)]
        [Order(55)]
        public HudElementAnchor BarsAnchor = HudElementAnchor.TopLeft;
    }


    [Serializable]
    [Section("Party Frames")]
    [SubSection("Health Bars", 0)]
    public class PartyFramesHealthBarsConfig : PluginConfigObject
    {
        public new static PartyFramesHealthBarsConfig DefaultConfig() { return new PartyFramesHealthBarsConfig(); }

        [DragInt2("Size", isMonitored = true)]
        [Order(30)]
        public Vector2 Size = new Vector2(150, 50);

        [DragInt2("Padding", isMonitored = true)]
        [Order(35)]
        public Vector2 Padding = new Vector2(1, 1);

        [InputText("Text Format")]
        [Order(40)]
        public string TextFormat = "[name:initials]";

        [NestedConfig("Colors", 45)]
        public PartyFramesColorsConfig ColorsConfig = new PartyFramesColorsConfig();

        [NestedConfig("Job/Role Icons", 50)]
        public PartyFramesRoleIconConfig RoleIconConfig = new PartyFramesRoleIconConfig();

        [NestedConfig("Shield", 55)]
        public ShieldConfig ShieldConfig = new ShieldConfig();
    }

    [Serializable]
    [Portable(false)]
    public class PartyFramesColorsConfig : PluginConfigObject
    {
        [Checkbox("Use Role Colors", isMonitored = true)]
        [CollapseControl(0, 0)]
        public bool UseRoleColors = false;

        [ColorEdit4("Tank Role Color")]
        [CollapseWith(0, 0)]
        public PluginConfigColor TankRoleColor = new PluginConfigColor(new Vector4(21f / 255f, 28f / 255f, 100f / 255f, 100f / 100f));

        [ColorEdit4("DPS Role Color")]
        [CollapseWith(5, 0)]
        public PluginConfigColor DPSRoleColor = new PluginConfigColor(new Vector4(153f / 255f, 23f / 255f, 23f / 255f, 100f / 100f));

        [ColorEdit4("Healer Role Color")]
        [CollapseWith(10, 0)]
        public PluginConfigColor HealerRoleColor = new PluginConfigColor(new Vector4(46f / 255f, 125f / 255f, 50f / 255f, 100f / 100f));

        [ColorEdit4("Generic Role Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor GenericRoleColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        [ColorEdit4("Background Color")]
        [Order(5)]
        public PluginConfigColor BackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 70f / 100f));

        [ColorEdit4("Out of Reach Color")]
        [Order(10)]
        public PluginConfigColor UnreachableColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 30f / 100f));
    }

    [Serializable]
    [Portable(false)]
    public class PartyFramesRoleIconConfig : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 1000)]
        [Order(30)]
        public Vector2 Size = new(20, 20);

        [Combo("Style", "Style 1", "Style 2")]
        [Order(35)]
        public int Style = 0;

        [Checkbox("Use Role Icons")]
        [CollapseControl(40, 0)]
        public bool UseRoleIcons = false;

        [Checkbox("Use Specific DPS Role Icons")]
        [CollapseWith(0, 0)]
        public bool UseSpecificDPSRoleIcons = false;
    }
}
