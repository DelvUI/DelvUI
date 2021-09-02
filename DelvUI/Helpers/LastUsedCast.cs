using Dalamud.Data.LuminaExtensions;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Data.Files;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Companion = Lumina.Excel.GeneratedSheets.Companion;
using DelvUI.Enums;
using ImGuiScene;
using Dalamud.Game.ClientState.Actors;

namespace DelvUI.Helpers
{
    public class LastUsedCast
    {
        private dynamic _lastUsedAction;
        private readonly BattleChara.CastInfo _castInfo;
        private readonly DalamudPluginInterface _pluginInterface;
        public readonly uint CastId;
        public readonly ActionType ActionType;
        public string ActionText;
        public DamageType DamageType;
        public TextureWrap IconTexture = null;
        public bool Interruptable;

        public LastUsedCast(uint castId, ActionType actionType, BattleChara.CastInfo castInfo, DalamudPluginInterface pluginInterface)
        {
            CastId = castId;
            ActionType = actionType;
            _castInfo = castInfo;
            _pluginInterface = pluginInterface;
            SetCastProperties();
            PluginLog.Log("Loaded new icon");
        }

        private void SetCastProperties()
        {
            var target = _pluginInterface.ClientState.Targets.SoftTarget ?? _pluginInterface.ClientState.Targets.CurrentTarget;
            var targetKind = target?.ObjectKind;

            switch (targetKind)
            {
                case null: break;
                case ObjectKind.Aetheryte:
                    ActionText = "Attuning...";
                    IconTexture = TexturesCache.Instance.GetTextureFromIconId<Action>(112, true);
                    return;
                case ObjectKind.EventObj:
                case ObjectKind.EventNpc:
                    ActionText = "Interacting...";
                    IconTexture = null;
                    return;
            }

            _lastUsedAction = null;
            Interruptable = _castInfo.Interruptible > 0;
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
                    _lastUsedAction = _pluginInterface.Data.GetExcelSheet<Action>()?.GetRow(CastId);
                    ActionText = _lastUsedAction?.Name.ToString();
                    IconTexture = TexturesCache.Instance.GetTexture<Action>(_lastUsedAction, true);
                    DamageType = GetDamageType(_lastUsedAction);
                    break;
                case ActionType.Mount:
                    _lastUsedAction = _pluginInterface.Data.GetExcelSheet<Mount>()?.GetRow(CastId);
                    ActionText = _lastUsedAction?.Singular.ToString();
                    IconTexture = TexturesCache.Instance.GetTexture<Mount>(_lastUsedAction, true);
                    DamageType = DamageType.Unknown;
                    break;
                case ActionType.KeyItem:
                case ActionType.Item:
                    _lastUsedAction = _pluginInterface.Data.GetExcelSheet<Item>()?.GetRow(CastId);
                    ActionText = _lastUsedAction?.Name.ToString() ?? "Using item...";
                    IconTexture = TexturesCache.Instance.GetTexture<Item>(_lastUsedAction, true);
                    DamageType = DamageType.Unknown;
                    break;
                case ActionType.Companion:
                    _lastUsedAction = _pluginInterface.Data.GetExcelSheet<Companion>()?.GetRow(CastId);
                    ActionText = _lastUsedAction?.Singular.ToString();
                    IconTexture = TexturesCache.Instance.GetTexture<Companion>(_lastUsedAction, true);
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

        private static DamageType GetDamageType(Action action)
        {
            var damageType = (DamageType) action.AttackType.Row;
            if (damageType != DamageType.Magic && damageType != DamageType.Darkness && damageType != DamageType.Unknown)
                damageType = DamageType.Physical;
            return damageType;
        }
    }
}