using Colourful;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using StructsCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace DelvUI.Helpers
{
    internal static class Utils
    {
        public static GameObject? GetBattleChocobo(GameObject? player)
        {
            if (player == null)
            {
                return null;
            }

            return GetBuddy(player.ObjectId, BattleNpcSubKind.Chocobo);
        }

        public static GameObject? GetBuddy(uint ownerId, BattleNpcSubKind kind)
        {
            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (var i = 0; i < 200; i += 2)
            {
                var gameObject = Plugin.ObjectTable[i];

                if (gameObject == null || gameObject.ObjectId == GameObject.InvalidGameObjectId || gameObject is not BattleNpc battleNpc)
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

        public static GameObject? GetGameObjectByName(string name)
        {
            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (int i = 0; i < 200; i += 2)
            {
                GameObject? gameObject = Plugin.ObjectTable[i];

                if (gameObject == null || gameObject.ObjectId == GameObject.InvalidGameObjectId || gameObject.ObjectId == 0)
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

        public static unsafe bool IsHostile(Character character)
        {
            byte* unk = (byte*)(new IntPtr(character.Address) + 0x1F0);

            return character != null
                && ((character.SubKind == (byte)BattleNpcSubKind.Enemy || (int)character.SubKind == 1)
                && *unk != 0);
        }

        public static unsafe float ActorShieldValue(GameObject? actor)
        {
            if (actor == null || actor is not Character)
            {
                return 0f;
            }

            StructsCharacter* chara = (StructsCharacter*)actor.Address;
            return Math.Min((float)chara->ShieldValue, 100f) / 100f;
        }

        public static bool IsActorCasting(GameObject? actor)
        {
            if (actor is not BattleChara chara)
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

        public static string DurationToString(double duration)
        {
            if (duration == 0)
            {
                return "";
            }

            TimeSpan t = TimeSpan.FromSeconds(duration);

            return t.Hours switch
            {
                >= 1 => t.Hours + "h",
                _ => t.Minutes switch
                {
                    >= 5 => t.Minutes + "m",
                    >= 1 => $"{t.Minutes}:{t.Seconds:00}",
                    _ => t.Seconds.ToString()
                }
            };
        }

        public static string DurationToFullString(double duration)
        {
            if (duration == 0)
            {
                return "";
            }

            TimeSpan t = TimeSpan.FromSeconds(duration);

            return $"{t.Minutes:00}:{t.Seconds:00}";
        }


        public static Status? GetTankInvulnerabilityID(BattleChara actor)
        {
            Status? tankInvulnBuff = actor.StatusList.FirstOrDefault(o => o.StatusId is 810 or 811 or 3255 or 1302 or 409 or 1836 or 82);

            return tankInvulnBuff;
        }

        public static bool IsOnCleanseJob()
        {
            PlayerCharacter? player = Plugin.ClientState.LocalPlayer;

            return player != null && JobsHelper.IsJobWithCleanse(player.ClassJob.Id, player.Level);
        }

        public static GameObject? FindTargetOfTarget(GameObject? target, GameObject? player, ObjectTable actors)
        {
            if (target == null)
            {
                return null;
            }

            if (target.TargetObjectId == 0 && player != null && player.TargetObjectId == 0)
            {
                return player;
            }

            // only the first 200 elements in the array are relevant due to the order in which SE packs data into the array
            // we do a step of 2 because its always an actor followed by its companion
            for (int i = 0; i < 200; i += 2)
            {
                GameObject? actor = actors[i];
                if (actor?.ObjectId == target.TargetObjectId)
                {
                    return actor;
                }
            }

            return null;
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
                    PluginLog.Error("Error trying to open url: " + e.Message);
                }
            }
        }

        public static unsafe bool? IsTargetCasting()
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_TargetInfo", 1);
            if (addon != null && addon->IsVisible)
            {
                if (addon->UldManager.NodeListCount < 41) { return true; }

                return addon->UldManager.NodeList[41]->IsVisible;
            }

            addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_TargetInfoCastBar", 1);
            if (addon != null && addon->IsVisible)
            {
                if (addon->UldManager.NodeListCount < 2) { return true; }

                return addon->UldManager.NodeList[2]->IsVisible;
            }

            return null;
        }

        public static unsafe bool? IsFocusTargetCasting()
        {
            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_FocusTargetInfo", 1);
            if (addon != null && addon->IsVisible)
            {
                if (addon->UldManager.NodeListCount < 16) { return true; }

                return addon->UldManager.NodeList[16]->IsVisible;
            }

            return null;
        }

        public static unsafe bool? IsEnemyInListCasting(int index)
        {
            if (index < 0 || index > 7) { return null; }

            AtkUnitBase* addon = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_EnemyList", 1);
            if (addon != null && addon->IsVisible)
            {
                if (addon->UldManager.NodeListCount < 12) { return true; }

                AtkResNode* node = addon->UldManager.NodeList[11 - index];
                if (node == null || !node->IsVisible) { return false; }

                AtkComponentBase* component = node->GetComponent();
                if (component == null || component->UldManager.NodeListCount < 13) { return true; }

                return component->UldManager.NodeList[12]->IsVisible;
            }

            return null;
        }

        public static unsafe uint? SignIconIDForActor(GameObject? actor)
        {
            if (actor == null)
            {
                return null;
            }

            return SignIconIDForObjectID(actor.ObjectId);
        }

        public static unsafe uint? SignIconIDForObjectID(uint objectId)
        {
            MarkingController* markingController = MarkingController.Instance();
            if (objectId == 0 || objectId == GameObject.InvalidGameObjectId || markingController == null)
            {
                return null;
            }

            for (int i = 0; i < 14; i++)
            {
                if (objectId == markingController->MarkerArray[i])
                {
                    return (uint)(60701 + i);
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
