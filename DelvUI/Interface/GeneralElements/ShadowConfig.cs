using System.Numerics;
using DelvUI.Config;
using DelvUI.Config.Attributes;

namespace DelvUI.Interface.GeneralElements;

[Exportable(false)]
public class ShadowConfig : PluginConfigObject
{
    [ColorEdit4("Color")]
    [Order(5)]
    public PluginConfigColor Color = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

    [DragInt("Thickness", min = 1, max = 20)]
    [Order(10)]
    public int Thickness = 1;
        
    [DragInt("Offset", min = 0, max = 20)]
    [Order(15)]
    public int Offset = 0;
}
