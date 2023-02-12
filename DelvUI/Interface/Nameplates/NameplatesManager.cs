using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Numerics;
using Lumina.Excel;
using Title = Lumina.Excel.GeneratedSheets.Title;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonNamePlate;
using static FFXIVClientStructs.FFXIV.Client.UI.UI3DModule;
using StructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using StructsCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace DelvUI.Interface.Nameplates
{
    public struct NameplateData
    {
        public GameObject? GameObject;
        public string Name;
        public string Title;
        public ObjectKind Kind;
        public byte SubKind;
        public Vector2 Position;

        public NameplateData(GameObject? gameObject, string name, string title, ObjectKind kind, byte subKind, Vector2 position)
        {
            GameObject = gameObject;
            Name = name;
            Title = title;
            Kind = kind;
            SubKind = subKind;
            Position = position;
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

            _sheet = Plugin.DataManager.GetExcelSheet<Title>();
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

        private ExcelSheet<Title>? _sheet;
        private Dictionary<string, bool> _titlePositionCache = new Dictionary<string, bool>();


        private unsafe void FrameworkOnOnUpdateEvent(Framework framework)
        {
            if (!_config.Enabled) { return; }

            //Character? target = Plugin.ClientState.LocalPlayer?.TargetObject as Character;
            //if (target != null)
            //{
            //    PluginLog.Log(target.StatusFlags.ToString());

            //    StructsCharacter* chara = (StructsCharacter*)target.Address;

            //    PluginLog.Log(chara->EventState.ToString());
            //    PluginLog.Log(chara->Mode.ToString());
            //    PluginLog.Log(chara->StatusFlags.ToString());
            //    PluginLog.Log(" ");

            //}

            UIModule* uiModule = StructsFramework.Instance()->GetUiModule();
            if (uiModule == null) { return; }

            UI3DModule* ui3DModule = uiModule->GetUI3DModule();
            if (ui3DModule == null) { return; }

            AddonNamePlate* addon = (AddonNamePlate*)Plugin.GameGui.GetAddonByName("NamePlate", 1);
            if (addon == null) { return; }

            RaptureAtkModule* atkModule = uiModule->GetRaptureAtkModule();
            if (atkModule == null || atkModule->AtkModule.AtkArrayDataHolder.StringArrayCount <= NameplateDataArrayIndex) { return; }

            StringArrayData* stringArrayData = atkModule->AtkModule.AtkArrayDataHolder.StringArrays[NameplateDataArrayIndex];

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
                if (arrayIndex < 0) { continue; }

                // position
                Vector2 position = new Vector2(
                    nameplateObject.RootNode->AtkResNode.X + nameplateObject.RootNode->AtkResNode.Width / 2f,
                    nameplateObject.RootNode->AtkResNode.Y + nameplateObject.RootNode->AtkResNode.Height
                );

                // name
                string name = "";
                if (stringArrayData != null && stringArrayData->AtkArrayData.Size > arrayIndex)
                {
                    name = MemoryHelper.ReadSeStringNullTerminated(new IntPtr(stringArrayData->StringArray[arrayIndex])).ToString();
                }

                //name += " " + Control.Instance()->TargetSystem.IsObjectInViewRange(obj).ToString();
                //name += " " + objectInfo->Unk_50.ToString() + " " + objectInfo->SortPriority.ToString();
                //name += " " + nameplateObject.Priority + " " + arrayIndex + " " + ui3DModule->NamePlateObjectIdList[arrayIndex];

                // title
                string title = "";
                if (stringArrayData != null && stringArrayData->AtkArrayData.Size > arrayIndex + NameplateCount)
                {
                    title = MemoryHelper.ReadSeStringNullTerminated(new IntPtr(stringArrayData->StringArray[arrayIndex + NameplateCount])).ToString();
                    if (title.Length > 2)
                    {
                        title = title[1..^1];
                    }
                }

                _data.Add(new NameplateData(gameObject, name, title, (ObjectKind)obj->ObjectKind, obj->SubKind, position));
            }

            _data.Reverse();
        }

        public bool IsTitleInFront(string title)
        {
            if (title.Length == 0)
            {
                return true;
            }

            if (_titlePositionCache.TryGetValue(title, out bool inFront))
            {
                return inFront;
            }

            inFront = true;
            Title? data = _sheet?.FirstOrDefault(row => row.Masculine == title || row.Feminine == title);
            if (data != null)
            {
                inFront = data.IsPrefix;
            }

            _titlePositionCache.Add(title, inFront);
            return inFront;
        }
    }
}
