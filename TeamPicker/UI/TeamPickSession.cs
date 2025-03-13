using System;
using System.Collections.Generic;
using System.Text;
using SilK.Unturned.Extras.UI;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using OpenMod.API.Plugins;
using Cysharp.Threading.Tasks;
using Steamworks;
using Microsoft.Extensions.Configuration;
using TeamPicker.Models;
using System.Linq;
using UnityEngine;
using TeamPicker.Services;

namespace TeamPicker.UI
{
    public class TeamPickSession : SingleEffectUISession
    {
        private readonly MyOpenModPlugin m_Plugin;
        private readonly IConfiguration m_Configuration;
        private readonly ITeamInfoService m_TeamInfoService;
        public TeamPickSession(UnturnedUser user, IServiceProvider serviceProvider,
            IPluginAccessor<MyOpenModPlugin> pluginAccesor, IConfiguration configuration,
            ITeamInfoService teamInfoService) : base(user, serviceProvider)
        {
            m_Plugin = pluginAccesor.Instance!;
            m_Configuration = configuration;
            m_TeamInfoService = teamInfoService;
        }

        public override ushort EffectId => 40119;
        public override string Id => "TeamPicker.UI";

        protected override async UniTask OnStartAsync()
        {
            SubscribeButtonClick("Menu_Button1", OnButtonOneClickedAsync);
            SubscribeButtonClick("Menu_Button2", OnButtonTwoClickedAsync);
            await UniTask.SwitchToMainThread();
            SendUIEffectWithKey(40119, 119);
            SendTextWithKey(119, "Canvas/Title/Text", "PICK ONE SIDE!");

            ulong firstGroupId = m_Configuration.GetSection("Active_Teams:Team_One").Get<ulong>();
            var firstGroup = m_Configuration.GetSection("Teams").Get<List<TeamModel>>()
                .Where(teamModel => (CSteamID)teamModel.ID == (CSteamID)firstGroupId).First();

            ulong secondGroupId = m_Configuration.GetSection("Active_Teams:Team_Two").Get<ulong>();
            var secondGroup = m_Configuration.GetSection("Teams").Get<List<TeamModel>>()
                .Where(teamModel => (CSteamID)teamModel.ID == (CSteamID)secondGroupId).First();

            SendTextWithKey(119, "Canvas/Buttons/Menu_Button1/Text", $"{firstGroup.Name}");
            SendTextWithKey(119, "Canvas/Buttons/Menu_Button2/Text", $"{secondGroup.Name}");

            User.Player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
        }

        private async UniTask OnButtonOneClickedAsync(string buttonName)
        {
            await UniTask.SwitchToMainThread();

            ulong groupId = m_Configuration.GetSection("Active_Teams:Team_One").Get<ulong>();
            var group = m_Configuration.GetSection("Teams").Get<List<TeamModel>>()
                .Where(teamModel => (CSteamID)teamModel.ID == (CSteamID)groupId).First();

            var groupName = group.Name;
            //Vector3 spawn = new Vector3(group.Spawn.X, group.Spawn.Y, group.Spawn.Z);
            
            var groupInfo = GroupManager.getGroupInfo((CSteamID)group.ID);
            if (groupInfo == null)
            {
                GroupManager.addGroup((CSteamID)group.ID, group.Name);
                var getInfo = GroupManager.getGroupInfo((CSteamID)group.ID);
                User.PrintMessageAsync($"[GroupManager.addGroup] {getInfo.groupID}, {getInfo.name}");
            }
            else
            {
                User.PrintMessageAsync($"[GroupManager.addGroup] {groupInfo.groupID}, {groupInfo.name}");
            }
            GroupManager.deleteGroup((CSteamID)group.ID);
            GroupManager.addGroup((CSteamID)group.ID, group.Name);

            User.Player.Player.quests.ServerAssignToGroup((CSteamID)group.ID, EPlayerGroupRank.MEMBER, true);


            User.Player.Player.ServerShowHint($"<color=white>Joined <b>{groupName}</b>", 5f);
            Vector3 spawn = m_TeamInfoService.GetTeamSpawn(User);
            User.Player.Player.teleportToLocation(spawn, 0f);

            await UniTask.SwitchToThreadPool();
            await EndAsync();
        }
        private async UniTask OnButtonTwoClickedAsync(string buttonName)
        {
            await UniTask.SwitchToMainThread();

            ulong groupId = m_Configuration.GetSection("Active_Teams:Team_Two").Get<ulong>();
            var group = m_Configuration.GetSection("Teams").Get<List<TeamModel>>()
                .Where(teamModel => (CSteamID)teamModel.ID == (CSteamID)groupId).First();

            var groupName = group.Name;
            //Vector3 spawn = new Vector3(group.Spawn.X, group.Spawn.Y, group.Spawn.Z);
            

            var groupInfo = GroupManager.getGroupInfo((CSteamID)group.ID);
            if (groupInfo == null)
            {
                GroupManager.addGroup((CSteamID)group.ID, group.Name);
                var getInfo = GroupManager.getGroupInfo((CSteamID)group.ID);
                User.PrintMessageAsync($"[GroupManager.addGroup] {getInfo.groupID}, {getInfo.name}");
            }
            else
            {
                User.PrintMessageAsync($"[GroupManager.addGroup] {groupInfo.groupID}, {groupInfo.name}");
            }
            GroupManager.deleteGroup((CSteamID)group.ID);
            GroupManager.addGroup((CSteamID)group.ID, group.Name);

            User.Player.Player.quests.ServerAssignToGroup((CSteamID)group.ID, EPlayerGroupRank.MEMBER, true);


            User.Player.Player.ServerShowHint($"<color=white>Joined <b>{groupName}</b>", 5f);
            Vector3 spawn = m_TeamInfoService.GetTeamSpawn(User);
            User.Player.Player.teleportToLocation(spawn, 0f);

            await UniTask.SwitchToThreadPool();
            await EndAsync();
        }

        protected override async UniTask OnEndAsync()
        {
            await UniTask.SwitchToMainThread();
            ClearEffect();
            User.Player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
        }
    }
}
