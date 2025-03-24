using Autofac;
using Kits.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamPicker.Models;
using UnityEngine;

namespace TeamPicker.Services
{
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
    public class TeamInfoService : ITeamInfoService
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        private readonly IPluginAccessor<Kits.Kits> m_pluginAccessorKits;
        public TeamInfoService(IStringLocalizer stringLocalizer, IConfiguration configuration, IPluginAccessor<Kits.Kits> pluginAccessorKits)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_pluginAccessorKits = pluginAccessorKits;
        }
        public Vector3 GetTeamSpawn(UnturnedUser user)
        {
            CSteamID groupID = user.Player.Player.quests.groupID;
            var group = m_Configuration.GetSection("Teams").Get<List<TeamModel>>()
                .Where(teamModel => (CSteamID)teamModel.ID == groupID).First();

            Vector3 spawn = new Vector3(group.Spawn.X, group.Spawn.Y, group.Spawn.Z);
            return spawn;
        }

        public bool HasValidTeam(UnturnedUser user)
        {
            CSteamID groupID = user.Player.Player.quests.groupID;

            ulong groupIdOne = m_Configuration.GetSection("Active_Teams:Team_One").Get<ulong>();
            ulong groupIdTwo = m_Configuration.GetSection("Active_Teams:Team_Two").Get<ulong>();

            if (groupID != (CSteamID)groupIdOne && groupID != (CSteamID)groupIdTwo) return false;
            return true;
        }

        public CSteamID GetTeamID(UnturnedUser user)
        {
            if (!HasValidTeam(user))
            {
                return CSteamID.Nil;
            }

            CSteamID groupID = user.Player.Player.quests.groupID;
            return groupID;
        }

        public void AddToTeam(UnturnedUser user, CSteamID TeamID)
        {
            var group = m_Configuration.GetSection("Teams").Get<List<TeamModel>>()
                .Where(teamModel => (CSteamID)teamModel.ID == TeamID).First();

            var groupInfo = GroupManager.getGroupInfo((CSteamID)group.ID);

            if (groupInfo == null)
            {
                GroupManager.addGroup((CSteamID)group.ID, group.Name);
                var getInfo = GroupManager.getGroupInfo((CSteamID)group.ID);
            }

            user.Player.Player.quests.ServerAssignToGroup(TeamID, EPlayerGroupRank.MEMBER, true);

            user.Player.Player.ServerShowHint($"<color=white>Joined <b>{group.Name}</b>", 5f);

            Vector3 spawn = GetTeamSpawn(user);
            user.Player.Player.teleportToLocation(spawn, 0f);

            user.Player.Inventory.Clear();
            if (m_pluginAccessorKits.Instance?.IsComponentAlive ?? false)
            {
                var service = m_pluginAccessorKits.Instance.LifetimeScope.Resolve<KitManager>();
                service.GiveKitAsync((IPlayerUser)user, group.DefaultKit, null, forceGiveKit: true);
            }
        }

        public bool IsTeamFull(CSteamID TeamID)
        {
            int groupOneCount = 0;
            int groupTwoCount = 0;
            ulong firstGroupId = m_Configuration.GetSection("Active_Teams:Team_One").Get<ulong>();
            ulong secondGroupId = m_Configuration.GetSection("Active_Teams:Team_Two").Get<ulong>();
            foreach (var player in Provider.clients)
            {
                if (player.playerID.group == (CSteamID)firstGroupId)
                {
                    groupOneCount++;
                }
                if (player.playerID.group == (CSteamID)secondGroupId)
                {
                    groupTwoCount++;
                }
            }

            int groupAcount = 0;
            int groupBcount = 0;

            if (firstGroupId == (uint)TeamID)
            {
                groupAcount = groupOneCount;
                groupBcount = groupTwoCount;
            }
            if (secondGroupId == (uint)TeamID)
            {
                groupAcount = groupTwoCount;
                groupBcount = groupOneCount;
            }
            
            /*if (groupAcount == 0 && groupBcount == 0) return false;
            if (groupBcount == 0) return false;
            if (Math.Abs(groupAcount - groupBcount) == 0) return false;
            float div = (float)groupAcount / (float)groupBcount;
            if (div == 1.25f) return true;
            return false;*/

            if (groupAcount == groupBcount) return false;
            if (groupAcount < groupBcount) return false;
            if (groupAcount - groupBcount >= 2) return true;
            return false;
        }
    }
}
