using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Data.Files;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Companion = Lumina.Excel.GeneratedSheets.Companion;
using DelvUI.Enums;

namespace DelvUI.Helpers
{
    public class LastUsedCast
    {
        private readonly uint _castId;
        private dynamic _lastUsedAction;
        private readonly BattleChara.CastInfo _castInfo;
        private readonly ActionType _actionType;
        private readonly DalamudPluginInterface _pluginInterface;
        public string ActionText;
        public TexFile Icon;
        public DamageType DamageType;
        public bool Interruptable;

        public LastUsedCast(uint castId, ActionType actionType, BattleChara.CastInfo castInfo, DalamudPluginInterface pluginInterface)
        {
            _castId = castId;
            _actionType = actionType;
            _castInfo = castInfo;
            _pluginInterface = pluginInterface;
            SetCastProperties();
        }

        private void SetCastProperties()
        {
            _lastUsedAction = null;
            Interruptable = _castInfo.Interruptible > 0;
            if (_castId == 1)
            {
                ActionText = "Interacting...";
                Icon = _pluginInterface.Data.GetIcon(0);
                return;
            }
            ActionText = "Casting";
            Icon = _pluginInterface.Data.GetIcon(0);
            
            switch (_actionType)
            {
                case ActionType.PetAction:
                case ActionType.Spell:
                case ActionType.SquadronAction:
                case ActionType.PvPAction:
                case ActionType.CraftAction:
                case ActionType.Ability:
                    _lastUsedAction = _pluginInterface.Data.GetExcelSheet<Action>()?.GetRow(_castId);
                    ActionText = _lastUsedAction?.Name.ToString();
                    Icon = _pluginInterface.Data.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = GetDamageType(_lastUsedAction);
                    break;
                case ActionType.Mount:
                    _lastUsedAction = _pluginInterface.Data.GetExcelSheet<Mount>()?.GetRow(_castId);
                    ActionText = _lastUsedAction?.Singular.ToString();
                    Icon = _pluginInterface.Data.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
                case ActionType.KeyItem:
                case ActionType.Item:
                    _lastUsedAction = _pluginInterface.Data.GetExcelSheet<Item>()?.GetRow(_castId);
                    ActionText = ActionText.ToString() != "" ? _lastUsedAction?.Name.ToString() : "Using item...";
                    Icon = _pluginInterface.Data.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
                case ActionType.Companion:
                    _lastUsedAction = _pluginInterface.Data.GetExcelSheet<Companion>()?.GetRow(_castId);
                    ActionText = _lastUsedAction?.Singular.ToString();
                    Icon = _pluginInterface.Data.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
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
                    Icon = _pluginInterface.Data.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
                default:
                    _lastUsedAction = null;
                    ActionText = "Casting...";
                    Icon = _pluginInterface.Data.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
            }
        }

        private static DamageType GetDamageType(Action action)
        {
            var damageType = (DamageType) action.AttackType.Row;
            if (damageType != DamageType.Magic && damageType != DamageType.Darkness && damageType != DamageType.Unknown)
                damageType = DamageType.Physical;
            return damageType;
        }
    }
}