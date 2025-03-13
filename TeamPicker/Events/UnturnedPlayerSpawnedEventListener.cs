using Microsoft.Extensions.Configuration;
using OpenMod.API.Eventing;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Core.Plugins;
using OpenMod.Core.Users;
using OpenMod.Core.Users.Events;
using OpenMod.Unturned.Players.Life.Events;
using OpenMod.Unturned.Users;
using SilK.Unturned.Extras.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeamPicker.Models;
using TeamPicker.Services;
using TeamPicker.UI;
using UnityEngine;
using OpenMod.Extensions.Games.Abstractions.Players;
using Autofac;
using Kits.Providers;

namespace TeamPicker.Events
{
    class UnturnedPlayerSpawnedEventListener : IEventListener<UnturnedPlayerSpawnedEvent>
    {
        private readonly IPluginAccessor<MyOpenModPlugin> m_pluginAccessor;
        private readonly IPluginAccessor<Kits.Kits> m_pluginAccessorKits;
        private readonly IUIManager m_uIManager;
        private readonly IConfiguration m_Configuration;
        private readonly IUserManager m_UserManager;
        private readonly ITeamInfoService m_TeamInfoService;
        public UnturnedPlayerSpawnedEventListener(IPluginAccessor<MyOpenModPlugin> pluginAccessor, IPluginAccessor<Kits.Kits> pluginAccessorKits,
            IUIManager uIManager, IConfiguration configuration,
            IUserManager userManager, ITeamInfoService teamInfoService)
        {
            m_pluginAccessor = pluginAccessor;
            m_pluginAccessorKits = pluginAccessorKits;
            m_uIManager = uIManager;
            m_Configuration = configuration;
            m_UserManager = userManager;
            m_TeamInfoService = teamInfoService;
        }
        public async Task HandleEventAsync(object? sender, UnturnedPlayerSpawnedEvent @event)
        {
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

            var user = await m_UserManager.FindUserAsync(KnownActorTypes.Player, @event.Player.SteamId.ToString(), UserSearchMode.FindById);
            if (user == null) return;

            var unturnedUser = (UnturnedUser)user;

            bool hasValidTeam = m_TeamInfoService.HasValidTeam(unturnedUser);
            if (!hasValidTeam)
            {
                await m_uIManager.StartSession<TeamPickSession>(unturnedUser, options, LifeTimeScope);
                var lobbySpawn = m_Configuration.GetSection("Lobby_Spawn").Get<SpawnCoordinates>();
                if (lobbySpawn == null)
                {
                    Vector3 coords = new Vector3(0f, 0f, 0f);
                    unturnedUser.Player.Player.teleportToLocation(coords, 0f);
                    await unturnedUser.PrintMessageAsync("sent to default coords");
                    return;
                }
                Vector3 spawn = new Vector3(lobbySpawn.X, lobbySpawn.Y, lobbySpawn.Z);
                await unturnedUser.PrintMessageAsync($"Coords: {lobbySpawn.X}, {lobbySpawn.Y}, {lobbySpawn.Z}");
                unturnedUser.Player.Player.teleportToLocation(spawn, 0f);
                return;
            }

            Vector3 teamSpawn = m_TeamInfoService.GetTeamSpawn(unturnedUser);
            unturnedUser.Player.Player.teleportToLocation(teamSpawn, 0f);
            string kitName = "starter";
            if (m_pluginAccessorKits.Instance?.IsComponentAlive ?? false)
            {
                var service = m_pluginAccessorKits.Instance.LifetimeScope.Resolve<KitManager>();
                await service.GiveKitAsync((IPlayerUser)user, kitName,null , forceGiveKit: true);
            }
            return;
        }
    }
}
