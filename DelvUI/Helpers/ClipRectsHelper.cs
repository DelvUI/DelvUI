using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DelvUI.Helpers
{
    public class ClipRectsHelper
    {
        #region Singleton
        private ClipRectsHelper()
        {
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;
            OnConfigReset(ConfigurationManager.Instance);

            // other plugins can add clip rects for DelvUI
            // rect start point = vector.X, vector.Y
            // rect end point = vector.Z, vector.W
            _thirdPartyClipRects = Plugin.PluginInterface.GetOrCreateData<Dictionary<string, Vector4>>(_sharedDataId, () => new());
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

            Plugin.PluginInterface.RelinquishData(_sharedDataId);

            Instance = null!;
        }
        #endregion

        private WindowClippingConfig _config = null!;

        private void OnConfigReset(ConfigurationManager sender)
        {
            _config = sender.GetConfigObject<WindowClippingConfig>();
        }

        public bool Enabled => _config.Enabled;
        public WindowClippingMode? Mode => _config.Enabled ? _config.Mode : null;

        private List<ClipRect> _clipRects = new List<ClipRect>();
        private List<ClipRect> _extraClipRects = new List<ClipRect>();

        private static Dictionary<string, Vector4> _thirdPartyClipRects = new();
        private static string _sharedDataId = "DelvUI.ClipRects";

        private static List<string> _ignoredAddonNames = new List<string>()
        {
            "_FocusTargetInfo",
        };

        private readonly string[] _hotbarAddonNames = { "_ActionBar", "_ActionBar01", "_ActionBar02", "_ActionBar03", "_ActionBar04", "_ActionBar05", "_ActionBar06", "_ActionBar07", "_ActionBar08", "_ActionBar09" };

        public unsafe void Update()
        {
            if (!_config.Enabled) { return; }

            _clipRects.Clear();
            _extraClipRects.Clear();

            // find clip rects for game windows
            AtkStage* stage = AtkStage.Instance();
            if (stage == null) { return; }

            RaptureAtkUnitManager* manager = stage->RaptureAtkUnitManager;
            if (manager == null) { return; }

            AtkUnitList* loadedUnitsList = &manager->AtkUnitManager.AllLoadedUnitsList;
            if (loadedUnitsList == null) { return; }

            for (int i = 0; i < loadedUnitsList->Count; i++)
            {
                try
                {
                    AtkUnitBase* addon = *(AtkUnitBase**)Unsafe.AsPointer(ref loadedUnitsList->Entries[i]);
                    if (addon == null || addon->RootNode == null || !addon->IsVisible || addon->WindowNode == null || addon->Scale == 0 || !addon->WindowNode->IsVisible())
                    {
                        continue;
                    }

                    string name = addon->NameString;
                    if (_ignoredAddonNames.Contains(name))
                    {
                        continue;
                    }

                    float margin = 5 * addon->Scale;
                    float bottomMargin = 13 * addon->Scale;

                    Vector2 pos = new Vector2(addon->RootNode->X + margin, addon->RootNode->Y + margin);
                    Vector2 size = new Vector2(
                        addon->RootNode->Width * addon->Scale - margin,
                        addon->RootNode->Height * addon->Scale - bottomMargin
                    );

                    // just in case this causes weird issues / crashes (doubt it though...)
                    ClipRect clipRect = new ClipRect(pos, pos + size);
                    if (clipRect.Max.X < clipRect.Min.X || clipRect.Max.Y < clipRect.Min.Y)
                    {
                        continue;
                    }

                    _clipRects.Add(clipRect);
                }
                catch { }
            }

            if (_config.ThirdPartyClipRectsEnabled)
            {
                // find clip rects from other plugins
                Dictionary<string, Vector4> dict = _thirdPartyClipRects;
                foreach (Vector4 vector in dict.Values)
                {
                    ClipRect clipRect = new ClipRect(new(vector.X, vector.Y), new(vector.Z, vector.W));
                    _clipRects.Add(clipRect);
                }
            }
        }

        private List<ClipRect> ActiveClipRects()
        {
            return [.. _clipRects, .. _extraClipRects];
        }

        public void AddNameplatesClipRects()
        {
            if (!_config.NameplatesClipRectsEnabled) { return; }

            // target cast bar
            ClipRect? targetCastbarClipRect = GetTargetCastbarClipRect();
            if (targetCastbarClipRect.HasValue)
            {
                _extraClipRects.Add(targetCastbarClipRect.Value);
            }

            // hotbars
            _extraClipRects.AddRange(GetHotbarsClipRects());

            // chat bubbles
            _extraClipRects.AddRange(GetChatBubbleClipRect());
        }

        public void RemoveNameplatesClipRects()
        {
            _extraClipRects.Clear();
        }

        private unsafe ClipRect? GetTargetCastbarClipRect()
        {
            if (!_config.TargetCastbarClipRectEnabled) { return null; }

            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_TargetInfoCastBar", 1);
            if (addon == null || !addon->IsVisible) { return null; }

            if (addon->UldManager.NodeListCount < 2) { return null; }

            AtkResNode* baseNode = addon->UldManager.NodeList[1];
            AtkResNode* imageNode = addon->UldManager.NodeList[2];

            if (baseNode == null || !baseNode->IsVisible()) { return null; }
            if (imageNode == null || !imageNode->IsVisible()) { return null; }

            Vector2 pos = new Vector2(
                addon->X + (baseNode->X * addon->Scale),
                addon->Y + (baseNode->Y * addon->Scale)
            );
            Vector2 size = new Vector2(
                imageNode->Width * addon->Scale,
                imageNode->Height * addon->Scale
            );

            return new ClipRect(pos, pos + size);
        }

        private unsafe List<ClipRect> GetHotbarsClipRects()
        {
            List<ClipRect> rects = new List<ClipRect>();
            if (!_config.HotbarsClipRectsEnabled) { return rects; }

            foreach (string addonName in _hotbarAddonNames)
            {
                AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName(addonName, 1);
                if (addon == null || !addon->IsVisible) { continue; }

                if (addon->UldManager.NodeListCount < 20) { continue; }

                AtkResNode* firstNode = addon->UldManager.NodeList[20];
                AtkResNode* lastNode = addon->UldManager.NodeList[9];

                if (firstNode == null || lastNode == null) { continue; }

                float margin = 10f * addon->Scale;

                Vector2 min = new Vector2(
                    addon->X + (firstNode->X * addon->Scale) + margin,
                    addon->Y + (firstNode->Y * addon->Scale) + margin
                );
                Vector2 max = new Vector2(
                    addon->X + (lastNode->X * addon->Scale) + (lastNode->Width * addon->Scale) - margin,
                    addon->Y + (lastNode->Y * addon->Scale) + (lastNode->Height * addon->Scale) - margin
                );

                rects.Add(new ClipRect(min, max));
            }

            return rects;
        }

        private unsafe List<ClipRect> GetChatBubbleClipRect()
        {
            List<ClipRect> rects = new List<ClipRect>();
            if (!_config.ChatBubblesClipRectsEnabled) { return rects; }

            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_MiniTalk", 1);
            if (addon == null || !addon->IsVisible) { return rects; }
            if (addon->UldManager.NodeListCount < 10) { return rects; }

            for (int i = 1; i <= 10; i++)
            {
                AtkResNode* node = addon->UldManager.NodeList[i];
                if (node == null || !node->IsVisible()) { continue; }

                AtkComponentNode* component = node->GetAsAtkComponentNode();
                if (component == null) { continue; }
                if (component->Component->UldManager.NodeListCount < 1) { continue; }

                AtkResNode* bubble = component->Component->UldManager.NodeList[1];
                Vector2 pos = new Vector2(
                    node->X + (bubble->X * addon->Scale),
                    node->Y + (bubble->Y * addon->Scale)
                );
                Vector2 size = new Vector2(
                    bubble->Width * addon->Scale,
                    bubble->Height * addon->Scale
                );

                rects.Add(new ClipRect(pos, pos + size));
            }

            return rects;
        }

        public ClipRect? GetClipRectForArea(Vector2 pos, Vector2 size)
        {
            if (!_config.Enabled) { return null; }

            List<ClipRect> rects = ActiveClipRects();

            foreach (ClipRect clipRect in rects)
            {
                ClipRect area = new ClipRect(pos, pos + size);
                if (clipRect.IntersectsWith(area))
                {
                    return clipRect;
                }
            }

            return null;
        }

        public static ClipRect[] GetInvertedClipRects(ClipRect clipRect)
        {
            float maxX = ImGui.GetMainViewport().Size.X;
            float maxY = ImGui.GetMainViewport().Size.Y;

            Vector2 aboveMin = new Vector2(0, 0);
            Vector2 aboveMax = new Vector2(maxX, clipRect.Min.Y);
            Vector2 leftMin = new Vector2(0, clipRect.Min.Y);
            Vector2 leftMax = new Vector2(clipRect.Min.X, maxY);

            Vector2 rightMin = new Vector2(clipRect.Max.X, clipRect.Min.Y);
            Vector2 rightMax = new Vector2(maxX, clipRect.Max.Y);
            Vector2 belowMin = new Vector2(clipRect.Min.X, clipRect.Max.Y);
            Vector2 belowMax = new Vector2(maxX, maxY);

            ClipRect[] invertedClipRects = new ClipRect[4];
            invertedClipRects[0] = new ClipRect(aboveMin, aboveMax);
            invertedClipRects[1] = new ClipRect(leftMin, leftMax);
            invertedClipRects[2] = new ClipRect(rightMin, rightMax);
            invertedClipRects[3] = new ClipRect(belowMin, belowMax);

            return invertedClipRects;
        }

        public bool IsPointClipped(Vector2 point)
        {
            if (!_config.Enabled) { return false; }

            List<ClipRect> rects = ActiveClipRects();

            foreach (ClipRect clipRect in rects)
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
            Vector2 screenSize = ImGui.GetMainViewport().Size;

            Min = Clamp(min, Vector2.Zero, screenSize);
            Max = Clamp(max, Vector2.Zero, screenSize);

            Vector2 size = Max - Min;

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
