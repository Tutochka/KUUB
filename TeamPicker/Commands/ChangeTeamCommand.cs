using Cysharp.Threading.Tasks;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using SilK.Unturned.Extras.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeamPicker.UI;


namespace TeamPicker.Commands
{
    [Command("ChangeTeam")] // The primary name for the command. Usually, it is defined as lowercase. 
    [CommandAlias("CTeam")] // Add "awsm" as alias.
    [CommandAlias("CT")] // Add "aw" as alias.
    [CommandDescription("CHANGES TEAM")] // Description. Try to keep it short and simple.
    public class ChangeTeamCommand : UnturnedCommand
    {
        private readonly IPluginAccessor<MyOpenModPlugin> m_pluginAccessor;
        private readonly IUIManager m_uIManager;
        public ChangeTeamCommand(IPluginAccessor<MyOpenModPlugin> pluginAccessor, IServiceProvider serviceProvider, IUIManager uIManager) : base(serviceProvider)
        {
            m_pluginAccessor = pluginAccessor;
            m_uIManager = uIManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Actor.Type == KnownActorTypes.Console) return;

            var options = new UISessionOptions
            {
                EndOnArenaClear = false,
                EndOnDeath = true
            };

            var pluginInstance = m_pluginAccessor.Instance;
            if (pluginInstance == null)
            {
                throw new InvalidOperationException("Plugin instance is null");
            }

            var LifeTimeScope = pluginInstance.LifetimeScope;

            var player = @Context.Actor;

            if (player is UnturnedUser)
            {
                await m_uIManager.StartSession<TeamPickSession>((UnturnedUser)player, options, LifeTimeScope);
            }
        }
    }
}