using Dalamud.Memory;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Arrays;
using StructsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace DelvUI.Interface.EnemyList
{
    public unsafe class EnemyListHelper
    {
        private List<EnemyListData> _enemiesData = new List<EnemyListData>();
        public IReadOnlyCollection<EnemyListData> EnemiesData => _enemiesData.AsReadOnly();
        public int EnemyCount => _enemiesData.Count;

        public void Update()
        {
            _enemiesData.Clear();

            var enemyListNumberInstance = EnemyListNumberArray.Instance();
            var enemyNumberArrayEnemies = enemyListNumberInstance->Enemies;
            int enemyCount = enemyListNumberInstance->Unk1;

            if(enemyCount == 0)
            {
                return;
            }

            for (int i = 0; i < enemyCount; i++)
            {
                int entityId = enemyNumberArrayEnemies[i].EntityId;
                int? letter = GetEnemyLetter(entityId, i);
                int enmityLevel = GetEnmityLevelForIndex(i);
                _enemiesData.Add(new EnemyListData(entityId, letter, enmityLevel));
            }
        }

        private int? GetEnemyLetter(int objectId, int index)
        {
            var enemyStringArrayMembers = EnemyListStringArray.Instance()->Members;
            if (enemyStringArrayMembers.IsEmpty || enemyStringArrayMembers.Length <= index)
            {
                return null;
            }

            string name = enemyStringArrayMembers[index].EnemyName;

            bool isMarked = Utils.SignIconIDForObjectID((uint)objectId) != null;
            char letterSymbol = isMarked && name.Length > 1 ? name[2] : name[0];
            return letterSymbol - 57457;
        }

        private int GetEnmityLevelForIndex(int index)
        {
            // gets enmity level by checking texture in enemy list addon

            AtkUnitBase* enemyList = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_EnemyList", 1).Address;
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
        public int EntityId;
        public int? LetterIndex;
        public int EnmityLevel;

        public EnemyListData(int entityId, int? letterIndex, int enmityLevel)
        {
            EntityId = entityId;
            LetterIndex = letterIndex;
            EnmityLevel = enmityLevel;
        }
    }
}
