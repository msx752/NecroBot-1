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

        [ExcelConfig(Description = "Allow bot go to gym for training, defense or battle with other team.", Position = 1)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Enable { get; set; }

        [ExcelConfig(Description = "Turn this on bot will select gym go go instead of poekstop if gym distance is valid", Position = 2)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool PrioritizeGymOverPokestop { get; set; }

        [ExcelConfig(Description = "Max distance bot allow wak to gym.", Position = 3)]
        [DefaultValue(500.0)]
        [Range(0, 9999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double MaxDistance { get; set; }

        [ExcelConfig(Description = "Default team color bot will join.", Position = 4)]
        [DefaultValue("Yellow")]
        [EnumDataType(typeof(TeamColor))]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public string DefaultTeam { get; set; }

        [ExcelConfig(Description = "Max CP that pokemmon will be select for defense gym.", Position = 5)]
        [DefaultValue(1800)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxCPToDeploy { get; set; }

        [ExcelConfig(Description = "Max level of pokemon can be put in gym.", Position = 6)]
        [DefaultValue(16)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxLevelToDeploy { get; set; }

        [ExcelConfig(Description = "Time in minute to visit come back and check gym, depend on distance setting", Position = 7)]
        [DefaultValue(60)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int VisitTimeout { get; set; }

        [ExcelConfig(Description = "Use randome pokemon for gym.", Position = 8)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseRandomPokemon { get; set; }

        [ExcelConfig(Description = "Top N pokemon by CP to exluse from pokemon deploy.", Position = 9)]
        [DefaultValue(10)]
        [Range(0, 999)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int NumberOfTopPokemonToBeExcluded { get; set; }

        [ExcelConfig(Description = "Collect coin after N gym deployed", Position = 10)]
        [DefaultValue(1)]
        [Range(1, 20)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int CollectCoinAfterDeployed { get; set; }

        [ExcelConfig(Description = "Enable attack gym", Position = 11)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnableAttackGym { get; set; }

        [ExcelConfig(Description = "Max Gym Level to Attack", Position = 12)]
        [DefaultValue(3)]
        [Range(1, 10)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxGymLevelToAttack { get; set; }

        [ExcelConfig(Description = "Max gym defenders to attack", Position = 13)]
        [DefaultValue(3)]
        [Range(1, 10)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxDefendersToAttack { get; set; }

        [ExcelConfig(Description = "Heal fefenders before apply to gym", Position = 14)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool HealDefendersBeforeApplyToGym { get; set; }

        [ExcelConfig(Description = "Don't Attack After Coins Limit is Reached", Position = 15)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DontAttackAfterCoinsLimitReached { get; set; }

        [ExcelConfig(Description = "Enable gym training", Position = 16)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnableGymTraining { get; set; }

        [ExcelConfig(Description = "Train gym when missing max points", Position = 17)]
        [DefaultValue(1000)]
        [Range(1, 50000)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int TrainGymWhenMissingMaxPoints { get; set; }

        [ExcelConfig(Description = "Max gym lvl to train", Position = 18)]
        [DefaultValue(4)]
        [Range(1, 10)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxGymLvlToTrain { get; set; }

        [ExcelConfig(Description = "Train already defended gym", Position = 19)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool TrainAlreadyDefendedGym { get; set; }

        [ExcelConfig(Description = "Min CP to use in attack", Position = 20)]
        [DefaultValue(1000)]
        [Range(1, 3500)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MinCpToUseInAttack { get; set; }

        [ExcelConfig(Description = "But not less than defender's percent", Position = 21)]
        [DefaultValue(0.75)]
        [Range(0.01, 1)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public double ButNotLessThanDefenderPercent { get; set; }

        [ExcelConfig(Description = "Don't use this skills in fight", Position = 22)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ICollection<KeyValuePair<PokemonId, PokemonMove>> NotUsedSkills = GetDefaults();

        [ExcelConfig(Description = "Use Pokemon to attack only by CP", Position = 23)]
        [DefaultValue(true)]
        [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsePokemonToAttackOnlyByCp { get; set; }

        private static ICollection<KeyValuePair<PokemonId, PokemonMove>> GetDefaults()
        {
            return new List<KeyValuePair<PokemonId, PokemonMove>>()
            {
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Snorlax, PokemonMove.HyperBeam ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Dragonite, PokemonMove.HyperBeam ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Lapras, PokemonMove.Blizzard ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Cloyster, PokemonMove.Blizzard ),
                new KeyValuePair<PokemonId, PokemonMove>( PokemonId.Flareon, PokemonMove.FireBlast ),
            };
        }
    }
}