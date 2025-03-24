using Cysharp.Threading.Tasks;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using Steamworks;
using System;
using TeamPicker.Services;

namespace TeamPicker.Commands
{
    [Command("DirectChangeTeam")]
    [CommandAlias("DCTeam")]
    [CommandAlias("DCT")]
    [CommandDescription("DIRECTLY CHANGES TEAM")]
    public class DirectChangeTeamCommand : UnturnedCommand
    {
        private readonly IPluginAccessor<MyOpenModPlugin> m_pluginAccessor;
        private readonly ITeamInfoService m_teamInfoService;
        private readonly IUserManager m_userManager;

        public DirectChangeTeamCommand(IPluginAccessor<MyOpenModPlugin> pluginAccessor, IServiceProvider serviceProvider, ITeamInfoService teamInfoService, IUserManager userManager) : base(serviceProvider)
        {
            m_pluginAccessor = pluginAccessor;
            m_teamInfoService = teamInfoService;
            m_userManager = userManager;
        }
        
        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Actor.Type == KnownActorTypes.Console) return;
            var caller = Context.Actor;

            var pluginInstance = m_pluginAccessor.Instance;
            if (pluginInstance == null)
            {
                throw new InvalidOperationException("Plugin instance is null");
            }

            var playerName = await Context.Parameters.GetAsync<string>(0);
            var teamId = await Context.Parameters.GetAsync<ulong>(1);

            var player = await m_userManager.FindUserAsync(KnownActorTypes.Player, playerName, UserSearchMode.FindByNameOrId);

            if (player is UnturnedUser unturnedUser)
            {
                await UniTask.SwitchToMainThread();
                m_teamInfoService.AddToTeam(unturnedUser, (CSteamID)teamId);
            }
            else
            {
                await caller.PrintMessageAsync($"Player '{playerName}' not found.");
            }
        }
    }
}
