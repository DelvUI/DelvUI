using Dalamud.Game.ClientState.Objects.Enums;
using DelvUI.Enums;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiScene;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Companion = Lumina.Excel.GeneratedSheets.Companion;

namespace DelvUI.Helpers
{
    public class LastUsedCast
    {
        private ExcelRow? _lastUsedAction;

        public readonly bool Interruptible;
        public readonly ActionType ActionType;
        public readonly uint CastId;
        public string ActionText { get; private set; } = "";
        public DamageType DamageType { get; private set; } = DamageType.Unknown;
        public TextureWrap? IconTexture { get; private set; } = null;

        public LastUsedCast(uint castId, ActionType actionType, bool interruptible)
        {
            CastId = castId;
            ActionType = actionType;
            Interruptible = interruptible;

            SetCastProperties();
        }

        private void SetCastProperties()
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            var targetKind = target?.ObjectKind;

            switch (targetKind)
            {
                case null:
                    break;

                case ObjectKind.Aetheryte:
                    ActionText = "Attuning...";
                    IconTexture = TexturesCache.Instance.GetTextureFromIconId<Action>(112);

                    return;

                case ObjectKind.EventObj:
                case ObjectKind.EventNpc:
                    ActionText = "Interacting...";
                    IconTexture = null;

                    return;
            }

            _lastUsedAction = null;
            if (CastId == 1 && ActionType != ActionType.Mount)
            {
                ActionText = "Interacting...";

                return;
            }

            ActionText = "Casting";

            switch (ActionType)
            {
                case ActionType.PetAction:
                case ActionType.Spell:
                case ActionType.SquadronAction:
                case ActionType.PvPAction:
                case ActionType.CraftAction:
                case ActionType.Ability:
                    var action = Plugin.DataManager.GetExcelSheet<Action>()?.GetRow(CastId);
                    ActionText = action?.Name.ToString() ?? "";
                    IconTexture = TexturesCache.Instance.GetTexture<Action>(action);
                    DamageType = GetDamageType(action);

                    _lastUsedAction = action;

                    break;

                case ActionType.Mount:
                    var mount = Plugin.DataManager.GetExcelSheet<Mount>()?.GetRow(CastId);
                    ActionText = mount?.Singular.ToString() ?? "";
                    IconTexture = TexturesCache.Instance.GetTexture<Mount>(mount);
                    DamageType = DamageType.Unknown;

                    _lastUsedAction = mount;

                    break;

                case ActionType.KeyItem:
                case ActionType.Item:
                    var item = Plugin.DataManager.GetExcelSheet<Item>()?.GetRow(CastId);
                    ActionText = item?.Name.ToString() ?? "Using item...";
                    IconTexture = TexturesCache.Instance.GetTexture<Item>(item);
                    DamageType = DamageType.Unknown;

                    _lastUsedAction = item;

                    break;

                case ActionType.Companion:
                    var companion = Plugin.DataManager.GetExcelSheet<Companion>()?.GetRow(CastId);
                    ActionText = companion?.Singular.ToString() ?? "";
                    IconTexture = TexturesCache.Instance.GetTexture<Companion>(companion);
                    DamageType = DamageType.Unknown;

                    _lastUsedAction = companion;

                    break;

                case ActionType.None:
                case ActionType.General:
                case ActionType.Unk_7:
                case ActionType.Unk_8:
                case ActionType.MainCommand:
                case ActionType.Waymark:
                case ActionType.ChocoboRaceAbility:
                case ActionType.ChocoboRaceItem:
                case ActionType.Unk_12:
                case ActionType.Unk_18:
                case ActionType.Accessory:
                    _lastUsedAction = null;
                    ActionText = "Casting...";
                    IconTexture = null;
                    DamageType = DamageType.Unknown;

                    break;

                default:
                    _lastUsedAction = null;
                    ActionText = "Casting...";
                    IconTexture = null;
                    DamageType = DamageType.Unknown;

                    break;
            }
        }

        private static DamageType GetDamageType(Action? action)
        {
            if (action == null)
            {
                return DamageType.Unknown;
            }

            var damageType = (DamageType)action.AttackType.Row;

            if (damageType != DamageType.Magic && damageType != DamageType.Darkness && damageType != DamageType.Unknown)
            {
                damageType = DamageType.Physical;
            }

            return damageType;
        }
    }
}
