using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Plugin.Services;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using StructsCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using StructsCharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using StructsGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace DelvUI.Helpers
{
    internal static class Utils
    {
        private static uint InvalidGameObjectId = 0xE0000000;

        public static IGameObject? GetBattleChocobo(IGameObject? player)
        {
            if (player == null)
            {
                return null;
            }

            return GetBuddy(player.GameObjectId, BattleNpcSubKind.Chocobo);
        }

        public static IGameObject? GetBuddy(ulong ownerId, BattleNpcSubKind kind)
        {
            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (var i = 0; i < 200; i += 2)
            {
                var gameObject = Plugin.ObjectTable[i];

                if (gameObject == null || gameObject.GameObjectId == InvalidGameObjectId || gameObject is not IBattleNpc battleNpc)
                {
                    continue;
                }

                if (battleNpc.BattleNpcKind == kind && battleNpc.OwnerId == ownerId)
                {
                    return gameObject;
                }
            }

            return null;
        }

        public static IGameObject? GetGameObjectByName(string name)
        {
            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (int i = 0; i < 200; i += 2)
            {
                IGameObject? gameObject = Plugin.ObjectTable[i];

                if (gameObject == null || gameObject.GameObjectId == InvalidGameObjectId || gameObject.GameObjectId == 0)
                {
                    continue;
                }

                if (gameObject.Name.ToString() == name)
                {
                    return gameObject;
                }
            }

            return null;
        }

        public static unsafe bool IsHostile(IGameObject obj)
        {
            if (obj is not ICharacter character)
            {
                return false;
            }

            if (character.SubKind != (byte)BattleNpcSubKind.Enemy && character.SubKind != (byte)BattleNpcSubKind.BattleNpcPart)
            {
                return false;
            }

            StructsCharacter* chara = (StructsCharacter*)character.Address;
            if (chara == null)
            {
                return false;
            }

            return chara->CharacterData.Battalion > 0 && chara->IsHostile;
        }

        public static unsafe float ActorShieldValue(IGameObject? actor)
        {
            if (actor == null || actor is not ICharacter)
            {
                return 0f;
            }

            StructsCharacter* chara = (StructsCharacter*)actor.Address;
            return Math.Min(chara->CharacterData.ShieldValue, 100f) / 100f;
        }

        public static bool IsActorCasting(IGameObject? actor)
        {
            if (actor is not IBattleChara chara)
            {
                return false;
            }

            try
            {
                return chara.IsCasting;
            }
            catch { }

            return false;
        }

        public static IEnumerable<Status> StatusListForActor(IGameObject? obj)
        {
            if (obj is IBattleChara chara)
            {
                return StatusListForBattleChara(chara);
            }

            return new List<Status>();
        }

        public static IEnumerable<Status> StatusListForBattleChara(IBattleChara? chara)
        {
            List<Status> statusList = new List<Status>();
            if (chara == null)
            {
                return statusList;
            }

            try
            {
                statusList = chara.StatusList.ToList();
            }
            catch { }

            return statusList;
        }

        public static string DurationToString(double duration, int decimalCount = 0)
        {
            if (duration == 0)
            {
                return "";
            }

            TimeSpan t = TimeSpan.FromSeconds(duration);

            if (t.Hours >= 1) { return t.Hours + "h"; }
            if (t.Minutes >= 5) { return t.Minutes + "m"; }
            if (t.Minutes >= 1) { return $"{t.Minutes}:{t.Seconds:00}"; }

            return duration.ToString("N" + decimalCount, ConfigurationManager.Instance.ActiveCultreInfo);
        }

        public static Status? GetTankInvulnerabilityID(IBattleChara actor)
        {
            return StatusListForBattleChara(actor).FirstOrDefault(o => o.StatusId is 810 or 811 or 3255 or 1302 or 409 or 1836 or 82);
        }

        public static bool IsOnCleanseJob()
        {
            IPlayerCharacter? player = Plugin.ClientState.LocalPlayer;

            return player != null && JobsHelper.IsJobWithCleanse(player.ClassJob.RowId, player.Level);
        }

        public static IGameObject? FindTargetOfTarget(IGameObject? target, IGameObject? player, IObjectTable actors)
        {
            if (target == null)
            {
                return null;
            }

            // Dalamud for now has an issue where it is only able to get the target ID of
            // NON-Networked objects through anything but GetTargetId on ClientStruct Gameobjects.
            // The bypass converts all Dalamud GameObject Data to ClientStructs GameObject Data and handles it accordingly.
            int actualTargetId = GetActualTargetId(target);
            // The Object ID that gets returned from minions is in reality the index
            // Checking for the correct object ID wouldn't work anyways as you would yet again run into the ObjectID = 0xE0000000 issue
            if (actualTargetId >= 0 && actualTargetId < actors.Length)
            {
                return actors[actualTargetId];
            }

            if (target.TargetObjectId == 0 && player != null && player.TargetObjectId == 0)
            {
                return player;
            }

            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (int i = 0; i < 200; i += 2)
            {
                IGameObject? actor = actors[i];
                if (actor?.GameObjectId == target.TargetObjectId)
                {
                    return actor;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the actual target ID of your targets target.
        /// </summary>
        /// <param name="target">Your target</param>
        /// <returns>Target ID of your targets targer. Returns -1 if old code should be ran.</returns>
        private static unsafe int GetActualTargetId(IGameObject target)
        {
            // We only need to check for companions.
            // Why not check target.TargetObject?.ObjectKind == ObjectKind.Companion?
            // Due to the Non-Networked game object bug the game is unaware of what type the object should actually be
            if (target.TargetObject?.ObjectKind != ObjectKind.Player)
            {
                return -1;
            }

            // Here we get the ClientStruct Character of our target (aka the player we are targeting)
            StructsCharacter targetChara = StructsCharacterManager.Instance()->LookupBattleCharaByEntityId(target.EntityId)->Character;

            // This method is key. GetTargetId() returns the targets player target ID. If it is converted to a hex string and starts with the number 4, it is a minion.
            // Even though it is a minion, it still returns the players target ID.
            ulong realTargetID = targetChara.GetTargetId();
            if (!realTargetID.ToString("X").StartsWith("4"))
            {
                return -1;
            }

            // We look up the parents ClientStruct GameObject
            StructsCharacter* realBattleChara = (StructsCharacter*)StructsCharacterManager.Instance()->LookupBattleCharaByEntityId((uint)realTargetID);
            if (realBattleChara == null)
            {
                return -1;
            }

            // And get the companion off of that
            StructsGameObject* companionGameObject = (StructsGameObject*)realBattleChara->CompanionData.CompanionObject;
            if (companionGameObject == null)
            {
                return -1;
            }

            // We return the index of the object here. Why?
            // Again due to the bug where ObjectID = 0xE0000000
            // The index does work and returns the exact minion index.
            return companionGameObject->ObjectIndex;
        }

        public static Vector2 GetAnchoredPosition(Vector2 position, Vector2 size, DrawAnchor anchor)
        {
            return anchor switch
            {
                DrawAnchor.Center => position - size / 2f,
                DrawAnchor.Left => position + new Vector2(0, -size.Y / 2f),
                DrawAnchor.Right => position + new Vector2(-size.X, -size.Y / 2f),
                DrawAnchor.Top => position + new Vector2(-size.X / 2f, 0),
                DrawAnchor.TopLeft => position,
                DrawAnchor.TopRight => position + new Vector2(-size.X, 0),
                DrawAnchor.Bottom => position + new Vector2(-size.X / 2f, -size.Y),
                DrawAnchor.BottomLeft => position + new Vector2(0, -size.Y),
                DrawAnchor.BottomRight => position + new Vector2(-size.X, -size.Y),
                _ => position
            };
        }

        public static string UserFriendlyConfigName(string configTypeName) => UserFriendlyString(configTypeName, "Config");

        public static string UserFriendlyString(string str, string? remove)
        {
            string? s = remove != null ? str.Replace(remove, "") : str;

            Regex? regex = new(@"
                    (?<=[A-Z])(?=[A-Z][a-z]) |
                    (?<=[^A-Z])(?=[A-Z]) |
                    (?<=[A-Za-z])(?=[^A-Za-z])",
                RegexOptions.IgnorePatternWhitespace);

            return regex.Replace(s, " ");
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                try
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows))
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", url);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Logger.Error("Error trying to open url: " + e.Message);
                }
            }
        }

        public static unsafe bool? IsTargetCasting()
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_TargetInfo", 1).Address;
            if (addon != null && addon->IsVisible)
            {
                if (addon->UldManager.NodeListCount < 41) { return true; }

                return addon->UldManager.NodeList[41]->IsVisible();
            }

            addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_TargetInfoCastBar", 1).Address;
            if (addon != null && addon->IsVisible)
            {
                if (addon->UldManager.NodeListCount < 2) { return true; }

                return addon->UldManager.NodeList[2]->IsVisible();
            }

            return null;
        }

        public static unsafe bool? IsFocusTargetCasting()
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_FocusTargetInfo", 1).Address;
            if (addon != null && addon->IsVisible)
            {
                if (addon->UldManager.NodeListCount < 16) { return true; }

                return addon->UldManager.NodeList[16]->IsVisible();
            }

            return null;
        }

        public static unsafe bool? IsEnemyInListCasting(int index)
        {
            if (index < 0 || index > 7) { return null; }

            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_EnemyList", 1).Address;
            if (addon != null && addon->IsVisible)
            {
                if (addon->UldManager.NodeListCount < 12) { return true; }

                AtkResNode* node = addon->UldManager.NodeList[11 - index];
                if (node == null || !node->IsVisible()) { return false; }

                AtkComponentBase* component = node->GetComponent();
                if (component == null || component->UldManager.NodeListCount < 13) { return true; }

                return component->UldManager.NodeList[12]->IsVisible();
            }

            return null;
        }

        public static unsafe uint? SignIconIDForActor(IGameObject? actor)
        {
            if (actor == null)
            {
                return null;
            }

            return SignIconIDForObjectID(actor.GameObjectId);
        }

        public static unsafe uint? SignIconIDForObjectID(ulong objectId)
        {
            MarkingController* markingController = MarkingController.Instance();
            if (objectId == 0 || objectId == InvalidGameObjectId || markingController == null)
            {
                return null;
            }

            for (int i = 0; i < 17; i++)
            {
                if (objectId == markingController->Markers[i])
                {
                    // attack1-5
                    if (i <= 4)
                    {
                        return (uint)(61201 + i);
                    }
                    // attack6-8
                    else if (i >= 14)
                    {
                        return (uint)(61201 + i - 9);
                    }
                    // shapes
                    else if (i >= 10)
                    {
                        return (uint)(61231 + i - 10);
                    }
                    // ignore1-2
                    else if (i >= 8)
                    {
                        return (uint)(61221 + i - 8);
                    }
                    // bind1-3
                    else if (i >= 5)
                    {
                        return (uint)(61211 + i - 5);
                    }
                }
            }

            return null;
        }

        public static bool IsHealthLabel(LabelConfig config)
        {
            return config.GetText().Contains("[health");
        }
    }
}
