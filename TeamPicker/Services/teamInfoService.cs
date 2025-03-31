using Autofac;
using Kits.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Serilog.Core;
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
        private readonly ILogger<MyOpenModPlugin> m_Logger;
        public TeamInfoService(IStringLocalizer stringLocalizer, IConfiguration configuration, IPluginAccessor<Kits.Kits> pluginAccessorKits, ILogger<MyOpenModPlugin> logger)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_pluginAccessorKits = pluginAccessorKits;
            m_Logger = logger;
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

        public IsTeamFullResult IsTeamFull(CSteamID selectedTeamID, CSteamID currentTeamId)
        {
            if (selectedTeamID == currentTeamId) return IsTeamFullResult.AlreadyInTeam; // selected the same team

            CSteamID teamAId = (CSteamID)m_Configuration.GetSection("Active_Teams:Team_One").Get<ulong>();
            CSteamID teamBId = (CSteamID)m_Configuration.GetSection("Active_Teams:Team_Two").Get<ulong>();
            int countA = 0, countB = 0;
            bool isPlayerInGroup;
            if (currentTeamId == teamAId || currentTeamId == teamBId) isPlayerInGroup = true;
            else
                isPlayerInGroup = false;
            if (selectedTeamID != teamAId && selectedTeamID != teamBId) return IsTeamFullResult.TeamNotValid; // Selection is none of the configured teams.
            foreach (var player in Provider.clients)
            {
                if (player.player.quests.groupID == teamAId)
                    countA++;
                if (player.player.quests.groupID == teamBId)
                    countB++;
            }

            if (!isPlayerInGroup)
            {
                if (countA == 0 && countB == 0) return IsTeamFullResult.TeamNotFull;
                if (countA == 0 && countB > 0 && selectedTeamID == teamBId) return IsTeamFullResult.TeamFull; // Selected team B is higher than 0, but A isn't
                if (countB == 0 && countA > 0 && selectedTeamID == teamAId) return IsTeamFullResult.TeamFull; // Selected team A is higher than 0, but B isn't 
                if (countA == 0 && selectedTeamID == teamAId) return IsTeamFullResult.TeamNotFull;
                if (countB == 0 && selectedTeamID == teamBId) return IsTeamFullResult.TeamNotFull;
                if (countA == countB) return IsTeamFullResult.TeamNotFull;
                if (countB > countA && Math.Abs(countB - countA) >= 2 && selectedTeamID == teamAId) return IsTeamFullResult.TeamNotFull;
                if (countA > countB && Math.Abs(countA - countB) >= 2 && selectedTeamID == teamBId) return IsTeamFullResult.TeamNotFull;

                if (countB > countA && Math.Abs(countB - countA) >= 2 && selectedTeamID == teamBId) return IsTeamFullResult.TeamFull;
                if (countA > countB && Math.Abs(countA - countB) >= 2 && selectedTeamID == teamAId) return IsTeamFullResult.TeamFull;

                return IsTeamFullResult.TeamNotFull;
            }

            if (isPlayerInGroup)
            {
                if (countA == 0 && countB == 0) return IsTeamFullResult.UnexpectedResult; // can't have both groups empty, if player is in a group.
                if (countA == 0 && countB == 1 && selectedTeamID == teamBId) return IsTeamFullResult.UnexpectedResult; // can't be because condition on top should filter same group
                if (countA == 1 && countB == 0 && selectedTeamID == teamAId) return IsTeamFullResult.UnexpectedResult; // can't be because condition on top should filter same group
                if (countA == 1 && countB == 0 && selectedTeamID == teamBId) return IsTeamFullResult.TeamNotFull; // other group is empty
                if (countB == 1 && countA == 0 && selectedTeamID == teamAId) return IsTeamFullResult.TeamNotFull; // other group is empty
                if (countA == 1 && countB == 1) return IsTeamFullResult.TeamFull; // can't, switching teams would leave any group empty.
                if (countA == 1 && countB > 1 && selectedTeamID == teamAId) return IsTeamFullResult.TeamNotFull; // returns everytime your team has more people than the opposite
                if (countB == 1 && countA > 1 && selectedTeamID == teamBId) return IsTeamFullResult.TeamNotFull; // returns everytime your team has more people than the opposite

                if (countA > countB && Math.Abs(countA - countB) >= 1 && selectedTeamID == teamAId && countA <= 10) return IsTeamFullResult.TeamFull; // diff greater than one but below 10 
                if (countB > countA && Math.Abs(countB - countA) >= 1 && selectedTeamID == teamBId && countB <= 10) return IsTeamFullResult.TeamFull; // diff greater than one but below 10.
                if (countA > countB && Math.Abs(countA - countB) >= 2 && selectedTeamID == teamAId && countA > 10) return IsTeamFullResult.TeamFull; // diff greater than one but above 10.
                if (countB > countA && Math.Abs(countB - countA) >= 2 && selectedTeamID == teamBId && countB > 10) return IsTeamFullResult.TeamFull; // diff greater than one but above 10.

                if (countA > countB && Math.Abs(countA - countB) >= 1 && selectedTeamID == teamBId && countA <= 10) return IsTeamFullResult.TeamNotFull; // diff greater than one but below 10 
                if (countB > countA && Math.Abs(countB - countA) >= 1 && selectedTeamID == teamAId && countB <= 10) return IsTeamFullResult.TeamNotFull; // diff greater than one but below 10.
                if (countA > countB && Math.Abs(countA - countB) >= 2 && selectedTeamID == teamBId && countA > 10) return IsTeamFullResult.TeamNotFull; // diff greater than one but above 10.
                if (countB > countA && Math.Abs(countB - countA) >= 2 && selectedTeamID == teamAId && countB > 10) return IsTeamFullResult.TeamNotFull; // diff greater than one but above 10.

                if (countA == countB) return IsTeamFullResult.TeamNotFull;
                return IsTeamFullResult.TeamNotFull;
            }

            return IsTeamFullResult.TeamNotFull;
        }

        public enum IsTeamFullResult
        {
            TeamFull,
            TeamNotFull,
            TeamNotValid,
            AlreadyInTeam,
            UnexpectedResult,
            endOfCheck0,
            endOfCheck1,
            endOfCheck2
        }
    }
}
