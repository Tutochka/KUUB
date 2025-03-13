using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API.Ioc;
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
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Transient)]
    public class teamInfoService : ITeamInfoService
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        public teamInfoService(IStringLocalizer stringLocalizer, IConfiguration configuration)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
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
    }
}
