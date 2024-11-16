using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures.TextureWraps;
using DelvUI.Enums;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Companion = Lumina.Excel.Sheets.Companion;

namespace DelvUI.Helpers
{
    public class LastUsedCast
    {
        private object? _lastUsedAction;

        public readonly bool Interruptible;
        public readonly ActionType ActionType;
        public readonly uint CastId;
        private uint? _iconId;

        public string ActionText { get; private set; } = "";
        public DamageType DamageType { get; private set; } = DamageType.Unknown;

        public LastUsedCast(uint castId, ActionType actionType, bool interruptible)
        {
            CastId = castId;
            ActionType = actionType;
            Interruptible = interruptible;

            SetCastProperties();
        }

        private void SetCastProperties()
        {
            IGameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            ObjectKind? targetKind = target?.ObjectKind;

            switch (targetKind)
            {
                case null:
                    break;

                case ObjectKind.Aetheryte:
                    ActionText = "Attuning...";
                    _iconId = 112;

                    return;

                case ObjectKind.EventObj:
                case ObjectKind.EventNpc:
                    ActionText = "Interacting...";
                    _iconId = null;

                    return;
            }

            _lastUsedAction = null;
            if (CastId == 1 && ActionType != ActionType.Mount)
            {
                ActionText = "Interacting...";

                return;
            }

            ActionText = "Casting";
            _iconId = null;

            switch (ActionType)
            {
                case ActionType.PetAction:
                case ActionType.Action:
                case ActionType.BgcArmyAction:
                case ActionType.PvPAction:
                case ActionType.CraftAction:
                case ActionType.Ability:
                    Action? action = Plugin.DataManager.GetExcelSheet<Action>()?.GetRow(CastId);
                    ActionText = action?.Name.ToString() ?? "";
                    DamageType = GetDamageType(action);
                    _lastUsedAction = action;

                    break;

                case ActionType.Mount:
                    Mount? mount = Plugin.DataManager.GetExcelSheet<Mount>()?.GetRow(CastId);
                    ActionText = mount?.Singular.ToString() ?? "";
                    DamageType = DamageType.Unknown;
                    _lastUsedAction = mount;
                    break;

                case ActionType.KeyItem:
                case ActionType.Item:
                    Item? item = Plugin.DataManager.GetExcelSheet<Item>()?.GetRow(CastId);
                    ActionText = item?.Name.ToString() ?? "Using item...";
                    DamageType = DamageType.Unknown;
                    _lastUsedAction = item;
                    break;

                case ActionType.Companion:
                    Companion? companion = Plugin.DataManager.GetExcelSheet<Companion>()?.GetRow(CastId);
                    ActionText = companion?.Singular.ToString() ?? "";
                    DamageType = DamageType.Unknown;
                    _lastUsedAction = companion;
                    break;

                default:
                    _lastUsedAction = null;
                    ActionText = "Casting...";
                    DamageType = DamageType.Unknown;
                    break;
            }
        }

        private static DamageType GetDamageType(Action? action)
        {
            if (!action.HasValue)
            {
                return DamageType.Unknown;
            }

            DamageType damageType = (DamageType)action.Value.AttackType.RowId;

            if (damageType != DamageType.Magic && damageType != DamageType.Darkness && damageType != DamageType.Unknown)
            {
                damageType = DamageType.Physical;
            }

            return damageType;
        }

        public IDalamudTextureWrap? GetIconTexture()
        {
            if (_iconId.HasValue)
            {
                return TexturesHelper.GetTexture<Action>(_iconId.Value);
            }
            else if (_lastUsedAction is Action action)
            {
                return TexturesHelper.GetTextureFromIconId(action.Icon);
            }
            else if (_lastUsedAction is Mount mount)
            {
                return TexturesHelper.GetTextureFromIconId(mount.Icon);
            }
            else if (_lastUsedAction is Item item)
            {
                return TexturesHelper.GetTextureFromIconId(item.Icon);
            }
            else if (_lastUsedAction is Companion companion)
            {
                return TexturesHelper.GetTextureFromIconId(companion.Icon);
            }

            return null;
        }
    }
}
