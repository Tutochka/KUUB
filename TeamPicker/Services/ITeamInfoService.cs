using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TeamPicker.Services
{
    [Service]
    public interface ITeamInfoService
    {
        Vector3 GetTeamSpawn(UnturnedUser user);
        bool HasValidTeam(UnturnedUser user);
        CSteamID GetTeamID(UnturnedUser user);
        void AddToTeam(UnturnedUser user, CSteamID TeamID);
    }
}
