﻿using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Data;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.State
{
    public class GymTeamState : IDisposable
    {
        /// <summary>
        /// Cache time in seconds
        /// </summary>
        private const long _cacheTime = 2 * 60;

        private Dictionary<string, CachedGymGetails> _gymDetails { get; set; }

        public List<MyPokemonStat> myPokemons { get; private set; }

        public List<AnyPokemonStat> otherDefenders { get; private set; }

        public List<GymPokemon> myTeam { get; private set; }

        public IEnumerable<MoveSettings> moveSettings { get; set; }

        public long timeToDodge { get; set; }

        public long lastWentDodge { get; set; }

        public SwitchPokemonData swithAttacker { get; set; }

        public int trainingRound { get; set; }

        public string trainingGymId { get; set; }

        public string capturedGymId { get; set; }

        public GymTeamState()
        {
            myTeam = new List<GymPokemon>();
            myPokemons = new List<MyPokemonStat>();
            otherDefenders = new List<AnyPokemonStat>();
            _gymDetails = new Dictionary<string, CachedGymGetails>();
            timeToDodge = 0;
            swithAttacker = null;
        }

        public void AddPokemon(ISession session, PokemonData pokemon, bool isMine = true)
        {
            if (isMine && myPokemons.Any(a => a.data.Id == pokemon.Id))
                return;

            if (!isMine && otherDefenders.Any(a => a.data.Id == pokemon.Id))
                return;

            if (isMine)
                myPokemons.Add(new MyPokemonStat(session, pokemon));
            else
                otherDefenders.Add(new AnyPokemonStat(session, pokemon));
        }

        public void AddToTeam(ISession session, PokemonData pokemon)
        {
            if (!myPokemons.Any(a => a.data.Id == pokemon.Id))
                myPokemons.Add(new MyPokemonStat(session, pokemon));

            if (!myTeam.Any(a => a.attacker.Id == pokemon.Id))
                myTeam.Add(new GymPokemon() { attacker = pokemon, hpState = pokemon.StaminaMax });
        }

        public async Task LoadMyPokemons(ISession session)
        {
            myPokemons.Clear();
            var pokemons = await session.Inventory.GetPokemons().ConfigureAwait(false);
            foreach (var pokemon in pokemons.Where(w => w.Cp >= session.LogicSettings.GymConfig.MinCpToUseInAttack))
            {
                MyPokemonStat mps = new MyPokemonStat(session, pokemon);
                myPokemons.Add(mps);
            }
        }

        public GetGymDetailsResponse getGymDetails(ISession session, FortData fort, bool force = false)
        {
            CachedGymGetails gymDetails = null;

            if (_gymDetails.Keys.Contains(fort.Id))
                gymDetails = _gymDetails[fort.Id];
            else
            {
                gymDetails = new CachedGymGetails(session, fort);
                _gymDetails.Add(fort.Id, gymDetails);
                force = false;
            }

            if (force || gymDetails.lastCall.AddSeconds(_cacheTime) < DateTime.UtcNow)
            {
                gymDetails.LoadData(session, fort);
                _gymDetails[fort.Id] = gymDetails;
            }

            return gymDetails.gymDetails;
        }

        public void Dispose()
        {
            if (myTeam != null)
                myTeam.Clear();

            if (myPokemons != null)
                myPokemons.Clear();

            if (otherDefenders != null)
                otherDefenders.Clear();
        }
    }

    public class GymPokemon : IDisposable
    {
        public PokemonData attacker { get; set; }

        public int hpState { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class AnyPokemonStat : IDisposable
    {
        public PokemonData data { get; set; }

        public MoveSettings attack { get; set; }

        public MoveSettings specialAttack { get; set; }

        public POGOProtos.Enums.PokemonType mainType { get; set; }

        public POGOProtos.Enums.PokemonType extraType { get; set; }

        public AnyPokemonStat(ISession session, PokemonData pokemon)
        {
            data = pokemon;

            var pokemonsSetting = session.Inventory.GetPokemonSettings().Result;
            
            mainType = pokemonsSetting.Where(f => f.PokemonId == data.PokemonId).Select(s => s.Type).FirstOrDefault();
            extraType = pokemonsSetting.Where(f => f.PokemonId == data.PokemonId).Select(s => s.Type2).FirstOrDefault();

            attack = session.Inventory.GetMoveSetting(data.Move1).Result;
            specialAttack = session.Inventory.GetMoveSetting(data.Move2).Result;
        }

        public void Dispose()
        {

        }
    }

    public class MyPokemonStat : AnyPokemonStat
    {

        public Dictionary<POGOProtos.Enums.PokemonType, int> typeFactor { get; private set; }

        public MyPokemonStat(ISession session, PokemonData pokemon) : base(session, pokemon)
        {
            typeFactor = new Dictionary<POGOProtos.Enums.PokemonType, int>();

            foreach (var type in Enum.GetValues(typeof(POGOProtos.Enums.PokemonType)))
            {
                GetFactorAgainst((POGOProtos.Enums.PokemonType)type);
            }
        }

        private int GetFactorAgainst(POGOProtos.Enums.PokemonType type)
        {
            if (typeFactor.Keys.Contains(type))
                return typeFactor[type];

            int factor = 0;
            if (UseGymBattleTask.GetBestTypes(type).Any(a => a == attack.PokemonType))
            {
                factor += 2;
                if (mainType == attack.PokemonType || extraType == attack.PokemonType)
                    factor += 1;
            }
            if (UseGymBattleTask.GetWorstTypes(type).Any(a => a == attack.PokemonType)) factor -= 2;

            if (UseGymBattleTask.GetBestTypes(type).Any(a => a == specialAttack.PokemonType))
            {
                factor += 2;
                if (mainType == specialAttack.PokemonType || extraType == specialAttack.PokemonType)
                    factor += 1;
            }
            if (UseGymBattleTask.GetWorstTypes(type).Any(a => a == specialAttack.PokemonType)) factor -= 2;

            typeFactor.Add(type, factor);

            return factor;
        }

        public int GetFactorAgainst(ISession session, int cp, bool isTraining)
        {
            decimal percent = 0.0M;
            if (cp > data.Cp)
                percent = (decimal)data.Cp / (decimal)cp * -100.0M;
            else
                percent = (decimal)cp / (decimal)data.Cp * 100.0M;

            int factor = (int)((100.0M - Math.Abs(percent)) / 5.0M) * Math.Sign(percent);

            if (isTraining && cp <= data.Cp)
                factor -= 100;

            if (session.LogicSettings.GymConfig.NotUsedSkills.Any(a => a.Key == data.PokemonId && a.Value == attack.MovementId))
                factor -= 20;

            if (session.LogicSettings.GymConfig.NotUsedSkills.Any(a => a.Key == data.PokemonId && a.Value == specialAttack.MovementId))
                factor -= 20;

            return factor;
        }

        private int GetFactorAgainst(PokemonSettings pokemon)
        {
            int factor = GetFactorAgainst(pokemon.Type);
            factor += GetFactorAgainst(pokemon.Type2);
            return factor;
        }

        // jjskuld - Ignore CS0108 warning for now.
#pragma warning disable 0108
        public void Dispose()
        {
            if (typeFactor != null)
                typeFactor.Clear();
        }
#pragma warning restore 0108
    }

    public class SwitchPokemonData
    {
        public ulong oldAttacker { get; private set; }
        public ulong newAttacker { get; private set; }

        public int attackDuration
        {
            get { return 1000; }
        }

        public SwitchPokemonData(ulong Old, ulong New)
        {
            oldAttacker = Old;
            newAttacker = New;
        }
    }

    public class CachedGymGetails
    {
        public DateTime lastCall { get; set; }

        public GetGymDetailsResponse gymDetails { get; set; }

        public CachedGymGetails(ISession session, FortData fort)
        {
            LoadData(session, fort);
        }

        public void LoadData(ISession session, FortData fort)
        {
            var task = session.Client.Fort.GetGymDetails(fort.Id, fort.Latitude, fort.Longitude);
            task.Wait();
            if (task.IsCompleted && task.Result.Result == GetGymDetailsResponse.Types.Result.Success)
            {
                fort = task.Result.GymState.FortData;
                gymDetails = task.Result;
                lastCall = DateTime.UtcNow;
            }
        }
    }

}