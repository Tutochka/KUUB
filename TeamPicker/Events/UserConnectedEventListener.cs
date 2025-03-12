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
            }
            
        }
    }
}
