using BepInEx.Configuration;
using System.Collections.Generic;

namespace TPVoting
{
    class PluginConfig
    {
        public static ConfigEntry<string>
            PlayerIsReadyMessages,
            IgnoredGameModes;

        public static ConfigEntry<int>
            MajorityVotesCountdownTime;

        public static ConfigEntry<float>
            PercentageOfTotal;

        public static ConfigEntry<bool>
            UserAutoVoteOnDeath,
            VoteAfterTPEvent;
    }
}
