﻿using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using StructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace DelvUI.Interface.EnemyList
{
    public unsafe class EnemyListHelper
    {
        private RaptureAtkModule* _raptureAtkModule = null;
        private const int EnemyListInfoIndex = 19;
        private const int EnemyListNamesIndex = 17;

        private List<EnemyListData> _enemiesData = new List<EnemyListData>();
        public IReadOnlyCollection<EnemyListData> EnemiesData => _enemiesData.AsReadOnly();
        public int EnemyCount => _enemiesData.Count;

        public void Update()
        {
            UIModule* uiModule = StructsFramework.Instance()->GetUiModule();
            if (uiModule != null)
            {
                _raptureAtkModule = uiModule->GetRaptureAtkModule();
            }

            _enemiesData.Clear();

            if (_raptureAtkModule == null || _raptureAtkModule->AtkModule.AtkArrayDataHolder.NumberArrayCount <= EnemyListInfoIndex)
            {
                return;
            }

            var numberArrayData = _raptureAtkModule->AtkModule.AtkArrayDataHolder.NumberArrays[EnemyListInfoIndex];
            if (numberArrayData->AtkArrayData.Size < 2) { return; }

            int enemyCount = numberArrayData->IntArray[1];
            for (int i = 0; i < enemyCount; i++)
            {
                int index = 8 + (i * 5);
                if (numberArrayData->AtkArrayData.Size <= index) { break; }

                int objectId = numberArrayData->IntArray[index];
                string? letter = GetEnemyLetterForIndex(i);
                int enmityLevel = GetEnmityLevelForIndex(i);
                _enemiesData.Add(new EnemyListData(objectId, letter, enmityLevel));
            }
        }

        private string? GetEnemyLetterForIndex(int index)
        {
            if (_raptureAtkModule == null || _raptureAtkModule->AtkModule.AtkArrayDataHolder.StringArrayCount <= EnemyListNamesIndex)
            {
                return null;
            }

            var stringArrayData = _raptureAtkModule->AtkModule.AtkArrayDataHolder.StringArrays[EnemyListNamesIndex];

            int i = index * 2;
            if (stringArrayData->AtkArrayData.Size <= i)
            {
                return null;
            }

            string name = MemoryHelper.ReadSeStringNullTerminated(new IntPtr(stringArrayData->StringArray[i])).ToString();
            if (name.Length == 0)
            {
                return null;
            }

            char letterSymbol = name[0];
            char letter = (char)(65 + letterSymbol - 57457);
            return letter.ToString();
        }

        private int GetEnmityLevelForIndex(int index)
        {
            // gets enmity level by checking texture in enemy list addon

            AtkUnitBase* enemyList = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_EnemyList", 1);
            if (enemyList == null || enemyList->RootNode == null) { return 0; }

            int id = index == 0 ? 2 : 20000 + index; // makes no sense but it is what it is (blame SE)
            AtkResNode* node = enemyList->GetNodeById((uint)id);
            if (node == null || node->GetComponent() == null) { return 0; }

            AtkImageNode* imageNode = (AtkImageNode*)node->GetComponent()->UldManager.SearchNodeById(13);
            if (imageNode == null) { return 0; }

            return Math.Min(4, imageNode->PartId + 1);
        }
    }

    public struct EnemyListData
    {
        public int ObjectId;
        public string? Letter;
        public int EnmityLevel;

        public EnemyListData(int objectId, string? letter, int enmityLevel)
        {
            ObjectId = objectId;
            Letter = letter;
            EnmityLevel = enmityLevel;
        }
    }
}
