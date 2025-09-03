using System;

namespace DelvUI.Enums;

// Copied relevant hotbars from https://github.com/zacharied/FFXIV-Plugin-HudManager/blob/testing/HUDManager/Structs/ElementKind.cs
public enum ElementKind : uint
{
    Hotbar1                           = 0xC48D3605, // _ActionBar_a
    Hotbar2                           = 0xFB7B6E1E, // _ActionBar01_a
    Hotbar3                           = 0xF93DD047, // _ActionBar02_a
    Hotbar4                           = 0xF8FFBA70, // _ActionBar03_a
    Hotbar5                           = 0xFDB0ACF5, // _ActionBar04_a
    Hotbar6                           = 0xFC72C6C2, // _ActionBar05_a
    Hotbar7                           = 0xFE34789B, // _ActionBar06_a
    Hotbar8                           = 0xFFF612AC, // _ActionBar07_a
    Hotbar9                           = 0xF4AA5591, // _ActionBar08_a
    Hotbar10                          = 0xF5683FA6, // _ActionBar09_a
    PetHotbar                         = 0xD8D188FF, // _ActionBarEx_a
    CrossHotbar                       = 0xBA81E8D1, // _ActionCross_a
    LeftWCrossHotbar                  = 0x6665735D, // _ActionDoubleCrossL_a
    RightWCrossHotbar                 = 0x70DDFD27, // _ActionDoubleCrossR_a
}

public static class ElementKindHelper
{
    public static ElementKind ElementKindByHotBarId(int hotBarId)
    {
        return hotBarId switch
        {
            0 => ElementKind.Hotbar1,
            1 => ElementKind.Hotbar2,
            2 => ElementKind.Hotbar3,
            3 => ElementKind.Hotbar4,
            4 => ElementKind.Hotbar5,
            5 => ElementKind.Hotbar6,
            6 => ElementKind.Hotbar7,
            7 => ElementKind.Hotbar8,
            8 => ElementKind.Hotbar9,
            9 => ElementKind.Hotbar10,
            10 => ElementKind.PetHotbar,
            _ => throw new ArgumentOutOfRangeException(nameof(hotBarId))
        };
    }
}