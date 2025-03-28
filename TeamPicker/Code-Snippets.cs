/*using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Snippets
{
    class Code_Snippets
    {
        public bool TeamFull(CSteamID selectedTeamID, CSteamID currentTeamId)
        {
            
            // Get team IDs from configuration.
            CSteamID teamA = (CSteamID)m_Configuration.GetSection("Active_Teams:Team_One").Get<ulong>();
            CSteamID teamB = (CSteamID)m_Configuration.GetSection("Active_Teams:Team_Two").Get<ulong>();
            m_Logger.LogInformation($"CurrentTeam: {currentTeamId}");

            // Count players in each team.
            int countA = 0, countB = 0;
            foreach (var player in Provider.clients)
            {
                if (player.player.quests.groupID == teamA)
                    countA++;
                else if (player.player.quests.groupID == teamB)
                    countB++;
            }

            // Validate that the selected team is one of the valid teams.
            if (selectedTeamID != teamA && selectedTeamID != teamB)
                return false;

            // Block a "rejoin" on the same team (to prevent loadout refresh).
            if (currentTeamId == selectedTeamID)
                return true;

            // Compute the new counts if the player were to switch.
            int newCountA = countA, newCountB = countB;
            if (currentTeamId == teamA && selectedTeamID == teamB)
            {
                newCountA = countA - 1;
                newCountB = countB + 1;
            }
            else if (currentTeamId == teamB && selectedTeamID == teamA)
            {
                newCountB = countB - 1;
                newCountA = countA + 1;
            }
            else
            {
                // If currentTeamId is not one of our teams, allow the join.
                return false;
            }

            // Prevent leaving a team that would then be left empty when both teams have just one player.
            if (countA == 1 && countB == 1)
                return true;

            // If your current team is underpopulated (<2 players), don't allow leaving it.
            if ((currentTeamId == teamA && countA < 2) || (currentTeamId == teamB && countB < 2))
                return true;

            // If one team is underpopulated (<2 players), force the player to join that team.
            if (countA < 2 || countB < 2)
            {
                if (countA < 2 && selectedTeamID != teamA)
                    return true;
                if (countB < 2 && selectedTeamID != teamB)
                    return true;
            }

            // Once both teams have at least 2 players, allow any switch provided that after switching 
            // the team difference does not exceed 2 players.
            if (countA >= 2 && countB >= 2)
            {
                if (Math.Abs(newCountA - newCountB) > 2)
                    return true;
            }

            return false;
        }
    }
}
*/