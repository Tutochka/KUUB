using OpenMod.API.Eventing;
using OpenMod.Unturned.Users.Events;
using OpenMod.Core.Users.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenMod.Unturned.Players.Connections.Events;
using SilK.Unturned.Extras.UI;
using OpenMod.API.Plugins;
using OpenMod.Core.Plugins;
using SDG.Unturned;
using TeamPicker.UI;
using OpenMod.Unturned.Users;
using Microsoft.Extensions.Configuration;
using UnityEngine;
using TeamPicker.Models;

namespace TeamPicker.Events
{
    public class UserConnectedEventListener : IEventListener<IUserConnectedEvent>
    {
        private readonly IPluginAccessor<MyOpenModPlugin> m_pluginAccessor;
        private readonly IUIManager m_uIManager;
        private readonly IConfiguration m_Configuration;
        public UserConnectedEventListener(IPluginAccessor<MyOpenModPlugin> pluginAccessor, IUIManager uIManager, IConfiguration configuration)
        {
            m_pluginAccessor = pluginAccessor;
            m_uIManager = uIManager;
            m_Configuration = configuration;
        }
        public async Task HandleEventAsync(object? sender, IUserConnectedEvent @event)
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
            var user = @event.User;
            if (user is UnturnedUser)
            {
                await m_uIManager.StartSession<TeamPickSession>((UnturnedUser)@event.User, options, LifeTimeScope);

                m_Configuration.GetSection("Active_Teams:Team_Two").Get<ulong>();
                var lobbySpawn = m_Configuration.GetSection("Lobby_Spawn").Get<SpawnCoordinates>();
                UnturnedUser player = (UnturnedUser)user;
                if (lobbySpawn == null)
                {
                    Vector3 coords = new Vector3(0f, 0f, 0f);
                    player.Player.Player.teleportToLocation(coords, 0f);
                    await player.PrintMessageAsync("sent to default coords");
                    return;
                }
                Vector3 spawn = new Vector3(lobbySpawn.X, lobbySpawn.Y, lobbySpawn.Z);
                await player.PrintMessageAsync($"Coords: {lobbySpawn.X}, {lobbySpawn.Y}, {lobbySpawn.Z}");
                player.Player.Player.teleportToLocation(spawn, 0f);
            }

        }
    }
}
