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
using OpenMod.Core.Eventing;
using Steamworks;

namespace TeamPicker.Events
{
    public class UserConnectedEventListener : IEventListener<IUserConnectedEvent>
    {
        public UserConnectedEventListener()
        {
        }
        [EventListener(Priority = EventListenerPriority.Highest)]
        public async Task HandleEventAsync(object? sender, IUserConnectedEvent @event)
        {
            UnturnedUser user = (UnturnedUser) @event.User;
            if (user is UnturnedUser)
            {

                user.Player.Player.quests.leaveGroup(true);
            }

        }
    }
}