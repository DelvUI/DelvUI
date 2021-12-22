using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace DelvUI.Helpers
{
    public class ClipRectsHelper
    {
        #region Singleton
        private ClipRectsHelper()
        {
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
            OnConfigReset(ConfigurationManager.Instance);
        }

        public static void Initialize() { Instance = new ClipRectsHelper(); }

        public static ClipRectsHelper Instance { get; private set; } = null!;

        ~ClipRectsHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            ConfigurationManager.Instance.ResetEvent -= OnConfigReset;
            _config.ValueChangeEvent -= OnConfigPropertyChanged;

            Instance = null!;
        }
        #endregion

        private HUDOptionsConfig _config = null!;

        private void OnConfigReset(ConfigurationManager sender)
        {
            if (_config != null)
            {
                _config.ValueChangeEvent -= OnConfigPropertyChanged;
            }

            _config = sender.GetConfigObject<HUDOptionsConfig>();
            _config.ValueChangeEvent += OnConfigPropertyChanged;
        }

        private void OnConfigPropertyChanged(object sender, OnChangeBaseArgs args)
        {
            if (args.PropertyName == "EnableClipRects" && !_config.EnableClipRects)
            {
                _clipRects.Clear();
            }
        }

        public bool Enabled => _config.EnableClipRects;
        public bool ClippingEnabled => _config.EnableClipRects && !_config.HideInsteadOfClip;

        // these are ordered by priority, if 2 game windows are on top of a DelvUI element
        // the one that comes first in this list is the one that will be clipped around
        internal static string[] AddonNames = new string[]
        {
            "ContextMenu",
            "ItemDetail", // tooltip
            "ActionDetail", // tooltip
            "AreaMap",
            "JournalAccept",
            "Talk",
            "Teleport",
            "ActionMenu",
            "Character",
            "CharacterInspect",
            "CharacterTitle",
            "Tryon",
            "ArmouryBoard",
            "RecommendList",
            "GearSetList",
            "MiragePrismMiragePlate",
            "ItemSearch",
            "RetainerList",
            "Bank",
            "RetainerSellList",
            "RetainerSell",
            "SelectString",
            "Shop",
            "ShopExchangeCurrency",
            "ShopExchangeItem",
            "CollectablesShop",
            "MateriaAttach",
            "Repair",
            "Inventory",
            "InventoryLarge",
            "InventoryExpansion",
            "InventoryEvent",
            "InventoryBuddy",
            "Buddy",
            "BuddyEquipList",
            "BuddyInspect",
            "Currency",
            "Macro",
            "PcSearchDetail",
            "Social",
            "SocialDetailA",
            "SocialDetailB",
            "LookingForGroup",
            "LookingForGroupSearch",
            "LookingForGroupCondition",
            "LookingForGroupDetail",
            "ReadyCheck",
            "Marker",
            "FieldMarker",
            "CountdownSettingDialog",
            "CircleFinder",
            "CircleList",
            "CircleNameInputString",
            "Emote",
            "FreeCompany",
            "FreeCompanyProfile",
            "HousingMenu",
            "HousingSubmenu",
            "HousingSignBoard",
            "CrossWorldLinkshell",
            "ContactList",
            "CircleBookInputString",
            "CircleBookQuestion",
            "CircleBookGroupSetting",
            "MultipleHelpWindow",
            "CircleFinderSetting",
            "CircleBook",
            "CircleBookWriteMessage",
            "ColorantColoring",
            "MonsterNote",
            "RecipeNote",
            "GatheringNote",
            "ContentsNote",
            "Orchestrion",
            "MountNoteBook",
            "MinionNoteBook",
            "AetherCurrent",
            "MountSpeed",
            "FateProgress",
            "SystemMenu",
            "ConfigCharacter",
            "ConfigSystem",
            "ConfigKeybind",
            "AOZNotebook",
            "PvpProfile",
            "GoldSaucerInfo",
            "Achievement",
            "RecommendList",
            "JournalDetail",
            "Journal",
            "ContentsFinder",
            "ContentsFinderSetting",
            "ContentsFinderMenu",
            "ContentsInfo",
            "Dawn",
            "BeginnersMansionProblem",
            "BeginnersMansionProblemCompList",
            "SupportDesk",
            "HowToList",
            "HudLayout",
            "LinkShell",
            "ChatConfig",
            "ColorPicker",
            "PlayGuide",
            "SelectYesno"
        };

        private List<ClipRect> _clipRects = new List<ClipRect>();

        public unsafe void Update()
        {
            if (!_config.EnableClipRects) { return; }

            _clipRects.Clear();

            foreach (string addonName in AddonNames)
            {
                var addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(addonName, 1);
                if (addon == null || !addon->IsVisible || addon->WindowNode == null || addon->Scale == 0)
                {
                    continue;
                }

                var margin = 5 * addon->Scale;
                var bottomMargin = 13 * addon->Scale;

                var clipRect = new ClipRect(
                    new Vector2(addon->X + margin, addon->Y + margin),
                    new Vector2(
                        addon->X + addon->WindowNode->AtkResNode.Width * addon->Scale - margin,
                        addon->Y + addon->WindowNode->AtkResNode.Height * addon->Scale - bottomMargin
                    )
                );

                // just in case this causes weird issues / crashes (doubt it though...)
                if (clipRect.Max.X < clipRect.Min.X || clipRect.Max.Y < clipRect.Min.Y)
                {
                    continue;
                }

                _clipRects.Add(clipRect);
            }
        }

        public ClipRect? GetClipRectForArea(Vector2 pos, Vector2 size)
        {
            foreach (ClipRect clipRect in _clipRects)
            {
                var area = new ClipRect(pos, pos + size);
                if (clipRect.IntersectsWith(area))
                {
                    return clipRect;
                }
            }

            return null;
        }

        public static ClipRect[] GetInvertedClipRects(ClipRect clipRect)
        {
            var maxX = ImGui.GetMainViewport().Size.X;
            var maxY = ImGui.GetMainViewport().Size.Y;

            var aboveMin = new Vector2(0, 0);
            var aboveMax = new Vector2(maxX, clipRect.Min.Y);
            var leftMin = new Vector2(0, clipRect.Min.Y);
            var leftMax = new Vector2(clipRect.Min.X, maxY);

            var rightMin = new Vector2(clipRect.Max.X, clipRect.Min.Y);
            var rightMax = new Vector2(maxX, clipRect.Max.Y);
            var belowMin = new Vector2(clipRect.Min.X, clipRect.Max.Y);
            var belowMax = new Vector2(maxX, maxY);

            ClipRect[] invertedClipRects = new ClipRect[4];
            invertedClipRects[0] = new ClipRect(aboveMin, aboveMax);
            invertedClipRects[1] = new ClipRect(leftMin, leftMax);
            invertedClipRects[2] = new ClipRect(rightMin, rightMax);
            invertedClipRects[3] = new ClipRect(belowMin, belowMax);

            return invertedClipRects;
        }

        public bool IsPointClipped(Vector2 point)
        {
            foreach (ClipRect clipRect in _clipRects)
            {
                if (clipRect.Contains(point))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public struct ClipRect
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        private readonly Rectangle Rectangle;

        public ClipRect(Vector2 min, Vector2 max)
        {
            var screenSize = ImGui.GetMainViewport().Size;

            Min = Clamp(min, Vector2.Zero, screenSize);
            Max = Clamp(max, Vector2.Zero, screenSize);

            var size = Max - Min;

            Rectangle = new Rectangle((int)Min.X, (int)Min.Y, (int)size.X, (int)size.Y);
        }

        public bool Contains(Vector2 point)
        {
            return Rectangle.Contains((int)point.X, (int)point.Y);
        }

        public bool IntersectsWith(ClipRect other)
        {
            return Rectangle.IntersectsWith(other.Rectangle);
        }

        private static Vector2 Clamp(Vector2 vector, Vector2 min, Vector2 max)
        {
            return new Vector2(Math.Max(min.X, Math.Min(max.X, vector.X)), Math.Max(min.Y, Math.Min(max.Y, vector.Y)));
        }
    }
}
