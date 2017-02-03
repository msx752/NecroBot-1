﻿using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using PoGo.NecroBot.Logic.Event;
using POGOProtos.Networking.Responses;
using POGOProtos.Inventory;

namespace PoGo.Necrobot.Window.Model
{
    public class PlayerInfoModel : ViewModelBase
    {
        public PokemonId BuddyPokemonId { get; set; }
        public string Name { get; set; }
        private double exp;
        public double Exp
        {
            get { return exp; }
            set
            {
                exp = value;
                RaisePropertyChanged("Exp");
                RaisePropertyChanged("PercentComplete");
            }
        }

        private double levelExp;
        public double LevelExp
        {
            get { return levelExp; }
            set
            {
                levelExp = value;
                RaisePropertyChanged("LevelExp");
                RaisePropertyChanged("PercentComplete");
            }
        }

        public int PercentComplete
        {
            get
            {
                if (LevelExp > 0)
                    return (int)Math.Floor(Exp / LevelExp * 100);
                return 0;
            }
        }

        private int expH;
        public int EXPPerHour
        {
            get { return expH; }
            set
            {
                expH = value;
                RaisePropertyChanged("EXPPerHour");
            }
        }

        private int pkmH;
        public int PKMPerHour
        {
            get { return pkmH; }
            set
            {
                pkmH = value;
                RaisePropertyChanged("PKMPerHour");

            }
        }

        private string runtime;
        public string Runtime
        {
            get { return runtime; }
            set
            {
                runtime = value;
                RaisePropertyChanged("Runtime");

            }
        }

        private string levelupTime;
        public string TimeToLevelUp
        {
            get { return levelupTime; }
            set
            {
                levelupTime = value;
                RaisePropertyChanged("TimeToLevelUp");

            }
        }
        private int stardust;
        public int Stardust
        {
            get { return stardust; }
            set
            {
                stardust = value;
                RaisePropertyChanged("Stardust");

            }
        }
        private int level;
        private GetPlayerResponse playerProfile;

        public int Level
        {
            get { return level; }
            set
            {
                level = value;
                RaisePropertyChanged("Level");

            }
        }

        public double BuddyTotalKM { get; set; }
        public double BuddyCurrentKM { get; set; }

        internal void OnProfileUpdate(ProfileEvent profile)
        {
            var stats = profile.Stats;
            Exp = stats.FirstOrDefault(x => x.Experience > 0).Experience;
            LevelExp = stats.FirstOrDefault(x => x.NextLevelXp > 0).NextLevelXp;

            this.playerProfile = profile.Profile;
        }

        public void OnInventoryRefreshed(IEnumerable<InventoryItem> inventory)
        {
            if (this.playerProfile == null || this.playerProfile.PlayerData.BuddyPokemon == null || this.playerProfile.PlayerData.BuddyPokemon.Id == 0) return;

            var budyData = this.playerProfile.PlayerData.BuddyPokemon;

            if (budyData == null) return;
             
            var buddy = inventory
                .Select(x => x.InventoryItemData?.PokemonData)
                .Where(x => x != null && x.Id == this.playerProfile.PlayerData.BuddyPokemon.Id)
                .FirstOrDefault();

            this.BuddyPokemonId = buddy.PokemonId;
            this.BuddyCurrentKM = budyData.LastKmAwarded;
            this.BuddyTotalKM = buddy.BuddyTotalKmWalked;

            this.RaisePropertyChanged("BuddyPokemonId");
            this.RaisePropertyChanged("BuddyCurrentKM");
            this.RaisePropertyChanged("BuddyTotalKM");
        }
    }
}
