using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using POGOProtos.Enums;
using System;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    [JsonObject(Title = "Gym Config", Description = "This config to setup rules for bot doing gym", ItemRequired = Required.DisallowNull)]
    public class GymConfig  : BaseConfig
    {
        public GymConfig() : base()
        {
        }

        internal enum TeamColor
        {
            Yellow,
            Red,
            Blue
        }

        [NecrobotConfig(Description = "Allow bot go to gym for training, defense or battle with other team.", Position = 1)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Enable { get; set; }

        [NecrobotConfig(Description = "Turn this on bot will select gym go go instead of poekstop if gym distance is valid", Position = 2)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool PrioritizeGymOverPokestop { get; set; }

        [NecrobotConfig(Description = "Max distance bot allow wak to gym.", Position = 3)]
        [DefaultValue(500.0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double MaxDistance { get; set; }

        [NecrobotConfig(Description = "Default team color bot will join.", Position = 4)]
        [DefaultValue("Yellow")]
        [EnumDataType(typeof(TeamColor))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public string DefaultTeam { get; set; }

        [NecrobotConfig(Description = "Max CP that pokemmon will be select for defense gym.", Position = 5)]
        [DefaultValue(1800)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxCPToDeploy { get; set; }

        [NecrobotConfig(Description = "Max level of pokemon can be put in gym.", Position = 6)]
        [DefaultValue(16)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxLevelToDeploy { get; set; }

        [NecrobotConfig(Description = "Time in minute to visit come back and check gym, depend on distance setting", Position = 7)]
        [DefaultValue(60)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int VisitTimeout { get; set; }

        [NecrobotConfig(Description = "Use randome pokemon for gym.", Position = 8)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseRandomPokemon { get; set; }

        [NecrobotConfig(Description = "Top N pokemon by CP to exluse from pokemon deploy.", Position = 9)]
        [DefaultValue(10)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int NumberOfTopPokemonToBeExcluded { get; set; }

        [NecrobotConfig(Description = "Collect coin after N gym deployed", Position = 10)]
        [DefaultValue(1)]
        [Range(1, 20)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int CollectCoinAfterDeployed { get; set; }

        [NecrobotConfig(Description = "Enable attack gym", Position = 11)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnableAttackGym { get; set; }

        [NecrobotConfig(Description = "Max Gym Level to Attack", Position = 12)]
        [DefaultValue(3)]
        [Range(1, 10)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxGymLevelToAttack { get; set; }

        [NecrobotConfig(Description = "Max gym defenders to attack", Position = 13)]
        [DefaultValue(3)]
        [Range(1, 10)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxDefendersToAttack { get; set; }

        [NecrobotConfig(Description = "Heal fefenders before apply to gym", Position = 14)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool HealDefendersBeforeApplyToGym { get; set; }

        [NecrobotConfig(Description = "Don't Attack After Coins Limit is Reached", Position = 15)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DontAttackAfterCoinsLimitReached { get; set; }

        [NecrobotConfig(Description = "Enable gym training", Position = 16)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnableGymTraining { get; set; }

        [NecrobotConfig(Description = "Train gym when missing max points", Position = 17)]
        [DefaultValue(1000)]
        [Range(1, 50000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int TrainGymWhenMissingMaxPoints { get; set; }

        [NecrobotConfig(Description = "Max gym lvl to train", Position = 18)]
        [DefaultValue(4)]
        [Range(1, 10)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxGymLvlToTrain { get; set; }

        [NecrobotConfig(Description = "Train already defended gym", Position = 19)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool TrainAlreadyDefendedGym { get; set; }

        [NecrobotConfig(Description = "Min CP to use in attack", Position = 20)]
        [DefaultValue(1000)]
        [Range(1, 3500)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MinCpToUseInAttack { get; set; }

        [NecrobotConfig(Description = "But not less than defender's percent", Position = 21)]
        [DefaultValue(0.75)]
        [Range(0.01, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double ButNotLessThanDefenderPercent { get; set; }

        [NecrobotConfig(Description = "Don't use this skills in fight", Position = 22)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ICollection<KeyValuePair<PokemonId, PokemonMove>> NotUsedSkills = GetDefaults();

        [NecrobotConfig(Description = "Use Pokemon to attack only by CP", Position = 23)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokemonToAttackOnlyByCp { get; set; }

        [NecrobotConfig(Description = "List of pokemon bot won't use in gym batle, deploy", Position = 24)]
        [DefaultValue("Kangaskhan;Tauros;MrMime;Farfetchd")]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<PokemonId> ExcludeForGyms { get; set; }

        [NecrobotConfig(Description = "Do not use dodge", Position = 25)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DontUseDodge { get; set; }

        [NecrobotConfig(Description = "Minimum revive potions to use gym module", Position = 26)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MinRevivePotions { get; set; }

        [NecrobotConfig(Description = "Prioritize Gym with free slot", Position = 27)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool PrioritizeGymWithFreeSlot { get; set; }

        [NecrobotConfig(Description = "Save Max Revives", Position = 28)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool SaveMaxRevives { get; set; }

        [NecrobotConfig(Description = "MaxTrainingRounds", Position = 29)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxTrainingRoundsOnOneGym { get; set; }

        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [NecrobotConfig(Description = "Defenders from list", Position = 30)]
        public List<TeamMemberConfig> Defenders { get; set; } = TeamMemberConfig.GetDefaultDefenders();

        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [NecrobotConfig(Description = "Attackers from list", Position = 31)]
        public List<TeamMemberConfig> Attackers { get; set; } = TeamMemberConfig.GetDefaultAttackers();

        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [NecrobotConfig(Description = "Trainers from list", Position = 32)]
        public List<TeamMemberConfig> Trainers { get; set; } = TeamMemberConfig.GetDefaultTrainers();

        private static ICollection<KeyValuePair<PokemonId, PokemonMove>> GetDefaults()
        {
            return new List<KeyValuePair<PokemonId, PokemonMove>>()
            {
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Snorlax, PokemonMove.HyperBeam ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Dragonite, PokemonMove.HyperBeam ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Lapras, PokemonMove.Blizzard ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Cloyster, PokemonMove.Blizzard ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Flareon, PokemonMove.FireBlast ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Gyarados, PokemonMove.HydroPump ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Exeggutor, PokemonMove.SolarBeam ),
            };
        }
    }

    [JsonObject(Description = "", ItemRequired = Required.DisallowNull)]
    public class TeamMemberConfig : BaseConfig
    {
        public TeamMemberConfig() : base()
        {
        }

        [NecrobotConfig(Description = "Pokemon to use", Position = 1)]
        [JsonProperty(Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Populate)]
        public PokemonId Pokemon { get; set; }

        [NecrobotConfig(Description = "Min CP to use in team", Position = 2)]
        [Range(1, 5000)]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int? MinCP { get; set; }

        [NecrobotConfig(Description = "Max CP to use in team", Position = 3)]
        [Range(1, 5000)]
        [DefaultValue(null)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int? MaxCP { get; set; }

        [NecrobotConfig(Description = "Priority", Position = 4)]
        [Range(1, 100)]
        [DefaultValue(5)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Priority { get; set; }

        [NecrobotConfig(Key = "Moves", Description = "Defined wanted moves to use", Position = 5)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        [DefaultValue(null)]
        public List<PokemonMove[]> Moves { get; set; }

        internal static List<TeamMemberConfig> GetDefaultDefenders()
        {
            return new List<TeamMemberConfig>()
            {
                { new TeamMemberConfig() { Pokemon=PokemonId.Lapras, MinCP=1000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Snorlax,  MinCP=1000, MaxCP=2400 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Vaporeon,  MinCP=2000, Moves = new List<PokemonMove[]>() { new PokemonMove[2] { PokemonMove.MoveUnset, PokemonMove.HydroPump } } } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Dragonite,  MinCP=2000, MaxCP=2499 } }
            };
        }

        internal static List<TeamMemberConfig> GetDefaultAttackers()
        {
            return new List<TeamMemberConfig>()
            {
                { new TeamMemberConfig() { Pokemon=PokemonId.Dragonite, MinCP=2500 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Vaporeon, MinCP=2000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Gyarados, MinCP=2000 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Snorlax, MinCP=2401 } },
            };
        }

        internal static List<TeamMemberConfig> GetDefaultTrainers()
        {
            return new List<TeamMemberConfig>()
            {
                { new TeamMemberConfig() { Pokemon=PokemonId.Dragonite, MaxCP=2000, Priority=100 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Vaporeon, MaxCP=2000, Priority=25 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Gyarados, MaxCP=2000, Priority=50 } },
                { new TeamMemberConfig() { Pokemon=PokemonId.Snorlax, MaxCP=2000, Priority=10 } },
            };
        }

        internal bool IsMoveMatch(PokemonMove move1, PokemonMove move2)
        {
            if(Moves!=null && Moves.Count > 0)
            {
                return Moves.Find(f => (f[0] == move1 || f[0] == PokemonMove.MoveUnset) && (f[1] == move2 || f[1] == PokemonMove.MoveUnset)) != null;
            }
            return true;
        }
    }
}