﻿using Dalamud.Plugin;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace DelvUI.Interface.Jobs
{
    public class JobHud : DraggableHudElement, IHudElementWithActor
    {
        protected DalamudPluginInterface PluginInterface => Plugin.PluginInterface;

        public JobConfig Config => (JobConfig)_config;

        public GameObject? Actor { get; set; } = null;
        protected PlayerCharacter? Player => Actor is PlayerCharacter ? (PlayerCharacter)Actor : null;

        public JobHud(string id, JobConfig config, string? displayName = null) : base(id, config, displayName)
        {
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Player == null)
            {
                return;
            }

            DrawJobHud(origin, Player);
        }

        public virtual void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            // override
        }
    }
}
