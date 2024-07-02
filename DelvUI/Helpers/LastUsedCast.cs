using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures.TextureWraps;
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
        public IDalamudTextureWrap? IconTexture { get; private set; } = null;

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
                    IconTexture = TexturesHelper.GetTexture<Action>(112);

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
                case ActionType.Action:
                case ActionType.BgcArmyAction:
                case ActionType.PvPAction:
                case ActionType.CraftAction:
                case ActionType.Ability:
                    var action = Plugin.DataManager.GetExcelSheet<Action>()?.GetRow(CastId);
                    ActionText = action?.Name.ToString() ?? "";
                    IconTexture = TexturesHelper.GetTexture<Action>(action);
                    DamageType = GetDamageType(action);

                    _lastUsedAction = action;

                    break;

                case ActionType.Mount:
                    var mount = Plugin.DataManager.GetExcelSheet<Mount>()?.GetRow(CastId);
                    ActionText = mount?.Singular.ToString() ?? "";
                    IconTexture = TexturesHelper.GetTexture<Mount>(mount);
                    DamageType = DamageType.Unknown;

                    _lastUsedAction = mount;

                    break;

                case ActionType.KeyItem:
                case ActionType.Item:
                    var item = Plugin.DataManager.GetExcelSheet<Item>()?.GetRow(CastId);
                    ActionText = item?.Name.ToString() ?? "Using item...";
                    IconTexture = TexturesHelper.GetTexture<Item>(item);
                    DamageType = DamageType.Unknown;

                    _lastUsedAction = item;

                    break;

                case ActionType.Companion:
                    var companion = Plugin.DataManager.GetExcelSheet<Companion>()?.GetRow(CastId);
                    ActionText = companion?.Singular.ToString() ?? "";
                    IconTexture = TexturesHelper.GetTexture<Companion>(companion);
                    DamageType = DamageType.Unknown;

                    _lastUsedAction = companion;

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
