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
using Autofac;
using Kits.Providers;
using OpenMod.Extensions.Games.Abstractions.Players;
using static TeamPicker.Services.TeamInfoService;

namespace TeamPicker.UI
{
    public class TeamPickSession : SingleEffectUISession
    {
        private readonly MyOpenModPlugin m_Plugin;
        private readonly IConfiguration m_Configuration;
        private readonly ITeamInfoService m_TeamInfoService;
        private readonly IPluginAccessor<Kits.Kits> m_pluginAccessorKits;
        public TeamPickSession(UnturnedUser user, IServiceProvider serviceProvider,
            IPluginAccessor<MyOpenModPlugin> pluginAccesor, IConfiguration configuration,
            ITeamInfoService teamInfoService, IPluginAccessor<Kits.Kits> pluginAccessorKits) : base(user, serviceProvider)
        {
            m_Plugin = pluginAccesor.Instance!;
            m_Configuration = configuration;
            m_TeamInfoService = teamInfoService;
            m_pluginAccessorKits = pluginAccessorKits;
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

            IsTeamFullResult isFull = m_TeamInfoService.IsTeamFull((CSteamID)groupId, User.Player.Player.quests.groupID);
            switch (isFull)
            {
                case IsTeamFullResult.TeamFull:
                    User.Player.Player.ServerShowHint($"<color=red>This group is full, pick the other one!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.TeamNotFull:
                    m_TeamInfoService.AddToTeam(User, (CSteamID)groupId);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.UnexpectedResult:
                    User.Player.Player.ServerShowHint($"<color=red>UnexpectedResult!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.TeamNotValid:
                    User.Player.Player.ServerShowHint($"<color=red>TeamNotValid!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.AlreadyInTeam:
                    User.Player.Player.ServerShowHint($"<color=red>Already in team!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.endOfCheck0:
                    User.Player.Player.ServerShowHint($"<color=red>endOfCheck0!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.endOfCheck1:
                    User.Player.Player.ServerShowHint($"<color=red>endOfCheck1!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.endOfCheck2:
                    User.Player.Player.ServerShowHint($"<color=red>endOfCheck2!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
            }
        }
        private async UniTask OnButtonTwoClickedAsync(string buttonName)
        {
            await UniTask.SwitchToMainThread();

            ulong groupId = m_Configuration.GetSection("Active_Teams:Team_Two").Get<ulong>();

            IsTeamFullResult isFull = m_TeamInfoService.IsTeamFull((CSteamID)groupId, User.Player.Player.quests.groupID);
            switch (isFull)
            {
                case IsTeamFullResult.TeamFull:
                    User.Player.Player.ServerShowHint($"<color=red>This group is full, pick the other one!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.TeamNotFull:
                    m_TeamInfoService.AddToTeam(User, (CSteamID)groupId);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.UnexpectedResult:
                    User.Player.Player.ServerShowHint($"<color=red>UnexpectedResult!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.TeamNotValid:
                    User.Player.Player.ServerShowHint($"<color=red>TeamNotValid!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.AlreadyInTeam:
                    User.Player.Player.ServerShowHint($"<color=red>Already in team!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.endOfCheck0:
                    User.Player.Player.ServerShowHint($"<color=red>endOfCheck0!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.endOfCheck1:
                    User.Player.Player.ServerShowHint($"<color=red>endOfCheck1!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
                case IsTeamFullResult.endOfCheck2:
                    User.Player.Player.ServerShowHint($"<color=red>endOfCheck2!", 5f);
                    await UniTask.SwitchToThreadPool();
                    await EndAsync();
                    return;
            }
        }

        protected override async UniTask OnEndAsync()
        {
            await UniTask.SwitchToMainThread();
            ClearEffect();
            User.Player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
        }
    }
}
