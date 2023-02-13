using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonNamePlate;
using static FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkModule;
using static FFXIVClientStructs.FFXIV.Client.UI.UI3DModule;
using StructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace DelvUI.Interface.Nameplates
{
    public struct NameplateData
    {
        public GameObject? GameObject;
        public string Name;
        public string Title;
        public bool IsTitlePrefix;
        public int NamePlateIconId;
        public ObjectKind Kind;
        public byte SubKind;
        public Vector2 ScreenPosition;
        public Vector3 WorldPosition;

        public NameplateData(GameObject? gameObject, string name, string title, bool isTitlePrefix, int namePlateIconId, ObjectKind kind, byte subKind, Vector2 screenPosition, Vector3 worldPosition)
        {
            GameObject = gameObject;
            Name = name;
            Title = title;
            IsTitlePrefix = isTitlePrefix;
            NamePlateIconId = namePlateIconId;
            Kind = kind;
            SubKind = subKind;
            ScreenPosition = screenPosition;
            WorldPosition = worldPosition;
        }
    }

    internal class NameplatesManager : IDisposable
    {
        #region Singleton
        public static NameplatesManager Instance { get; private set; } = null!;
        private NameplatesGeneralConfig _config = null!;

        private NameplatesManager()
        {
            Plugin.Framework.Update += FrameworkOnOnUpdateEvent;
            ConfigurationManager.Instance.ResetEvent += OnConfigReset;

            OnConfigReset(ConfigurationManager.Instance);
        }

        public static void Initialize()
        {
            Instance = new NameplatesManager();
        }

        ~NameplatesManager()
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

            Plugin.Framework.Update -= FrameworkOnOnUpdateEvent;

            Instance = null!;
        }

        private void OnConfigReset(ConfigurationManager sender)
        {
            _config = sender.GetConfigObject<NameplatesGeneralConfig>();
        }
        #endregion Singleton

        private const int NameplateCount = 50;
        private const int NameplateDataArrayIndex = 4;

        private List<NameplateData> _data = new List<NameplateData>();
        public IReadOnlyCollection<NameplateData> Data => _data.AsReadOnly();

        private unsafe void FrameworkOnOnUpdateEvent(Framework framework)
        {
            if (!_config.Enabled) { return; }

            UIModule* uiModule = StructsFramework.Instance()->GetUiModule();
            if (uiModule == null) { return; }

            UI3DModule* ui3DModule = uiModule->GetUI3DModule();
            if (ui3DModule == null) { return; }

            AddonNamePlate* addon = (AddonNamePlate*)Plugin.GameGui.GetAddonByName("NamePlate", 1);
            if (addon == null) { return; }

            RaptureAtkModule* atkModule = uiModule->GetRaptureAtkModule();
            if (atkModule == null || atkModule->AtkModule.AtkArrayDataHolder.StringArrayCount <= NameplateDataArrayIndex) { return; }

            NamePlateInfo* infoArray = &atkModule->NamePlateInfoArray;

            _data = new List<NameplateData>();
            int activeCount = ui3DModule->NamePlateObjectInfoCount;

            for (int i = 0; i < activeCount; i++)
            {
                ObjectInfo* objectInfo = ((ObjectInfo**)ui3DModule->NamePlateObjectInfoPointerArray)[i];
                if (objectInfo == null || objectInfo->NamePlateIndex >= NameplateCount) { continue; }

                // actor
                FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* obj = objectInfo->GameObject;
                if (obj == null) { continue; }
                GameObject? gameObject = Plugin.ObjectTable.CreateObjectReference(new IntPtr(obj));

                // ui nameplate
                NamePlateObject nameplateObject = addon->NamePlateObjectArray[objectInfo->NamePlateIndex];
                int arrayIndex = activeCount - nameplateObject.Priority - 1;
                if (arrayIndex < 0 || arrayIndex > NameplateCount) { continue; }

                // position
                Vector2 screenPos = new Vector2(
                    nameplateObject.RootNode->AtkResNode.X + nameplateObject.RootNode->AtkResNode.Width / 2f,
                    nameplateObject.RootNode->AtkResNode.Y + nameplateObject.RootNode->AtkResNode.Height
                );
                Vector3 worldPos = new Vector3(obj->Position.X, obj->Position.Y + obj->Height * 2.2f, obj->Position.Z);

                // name
                NamePlateInfo info = infoArray[objectInfo->NamePlateIndex];
                string name = info.Name.ToString();

                // title
                string title = info.Title.ToString();
                bool isTitlePrefix = info.IsPrefixTitle;

                // state icon
                int iconId = 0;
                AtkUldAsset* textureInfo = nameplateObject.IconImageNode->PartsList->Parts[nameplateObject.IconImageNode->PartId].UldAsset;
                if (textureInfo != null && textureInfo->AtkTexture.Resource != null)
                {
                    iconId = textureInfo->AtkTexture.Resource->IconID;
                }

                _data.Add(new NameplateData(gameObject, name, title, isTitlePrefix, iconId, (ObjectKind)obj->ObjectKind, obj->SubKind, screenPos, worldPos));
            }

            _data.Reverse();
        }
    }
}
