using System.Runtime.CompilerServices;
using BepInEx;
using R2API.Utils;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using static TPVoting.PluginConfig;

namespace TPVoting
{
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class TPVotingPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.2.3";
        public const string ModName = "TPVoting";
        public const string ModGuid = "com.Mordrog.TPVoting";

        public TPVotingSystem TPVotingSystem { get; private set; }

        public TPVotingPlugin()
        {
            InitConfig();
        }

        public void Awake()
        {
            On.RoR2.Run.Awake += Run_Awake;
            On.RoR2.Run.OnDestroy += Run_OnDestroy;
        }

        private void Run_Awake(On.RoR2.Run.orig_Awake orig, Run self)
        {
            orig(self);

            if (IgnoredGameModes.Value.Contains(GameModeCatalog.GetGameModeName(self.gameModeIndex)))
            {
                return;
            }

            TPVotingSystem = gameObject.AddComponent<TPVotingSystem>();

        }

        private void Run_OnDestroy(On.RoR2.Run.orig_OnDestroy orig, Run self)
        {
            orig(self);

            if (TPVotingSystem)
            {
                Destroy(TPVotingSystem);
            }
        }

        private void InitConfig()
        {
            PlayerIsReadyMessages = Config.Bind<string>(
                "Settings",
                "PlayerIsReadyMessages",
                "r,rdy,ready",
                "The message the player has to write in the chat to confirm they are ready. Values must be separated by comma."
            );

            IgnoredGameModes = Config.Bind<string>(
                "Settings",
                "IgnoredGameModes",
                "InfiniteTowerRun",
                "Gamemode in which tp voting should not work. Values must be separated by comma. Possible values: 'InfiniteTowerRun', 'EclipseRun', 'ClassicRun', 'WeeklyRun'"
            );

            MajorityVotesCountdownTime = Config.Bind<int>(
                "Settings",
                "MajorityVotesCountdownTime",
                30,
                "Countdown in seconds to unlock the teleporter when 'majority' of the players are ready."
            );

            PercentageOfTotal = Config.Bind<float>(
                "Settings",
                "PercentageOfTotal",
                50f,
                "Percentage of total players that need to be ready to start the countdown. Value must be between 0 and 1."
            );

            UserAutoVoteOnDeath = Config.Bind<bool>(
                "Settings",
                "UserAutoVoteOnDeath",
                true,
                "Should players auto vote tp when they die."
            );

            VoteAfterTPEvent = Config.Bind<bool>(
                "Settings",
                "VoteAfterTPEvent",
                false,
                "Should tp voting also be activated after tp event."
            );

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Mordrog.TPVoting"))
            {
                AddConfigOptions();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AddConfigOptions()
        {
            ModSettingsManager.AddOption(new StringInputFieldOption(PlayerIsReadyMessages));
            ModSettingsManager.AddOption(new StringInputFieldOption(IgnoredGameModes));
            ModSettingsManager.AddOption(new IntSliderOption(MajorityVotesCountdownTime, new IntSliderConfig() { min = 0, max = 100}));
            ModSettingsManager.AddOption(new SliderOption(PercentageOfTotal, new SliderConfig() { min = 0, max = 100}));
            ModSettingsManager.AddOption(new CheckBoxOption(UserAutoVoteOnDeath));
            ModSettingsManager.AddOption(new CheckBoxOption(VoteAfterTPEvent));
        }

    }
}
