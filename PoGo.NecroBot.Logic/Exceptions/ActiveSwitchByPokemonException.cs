﻿using System;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Model.Settings;
using POGOProtos.Enums;

namespace PoGo.NecroBot.Logic.Exceptions
{
    public class ActiveSwitchByPokemonException : Exception
    {
        public double LastLatitude { get; set; }
        public double LastLongitude { get; set; }
        public PokemonId LastEncounterPokemonId { get; set; }
        public MultiAccountManager.BotAccount Bot { get; set; }
        public bool Snipe { get; set; }
        public EncounteredEvent EncounterData { get; set; }
    }
}