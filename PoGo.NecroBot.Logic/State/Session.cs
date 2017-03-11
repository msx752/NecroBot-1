#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.Inventory;
using PoGo.NecroBot.Logic.Interfaces.Configuration;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.Model.Settings;
using PoGo.NecroBot.Logic.Service;
using PoGo.NecroBot.Logic.Service.Elevation;
using PoGo.NecroBot.Logic.Tasks;
using PoGo.NecroBot.Logic.Utils;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using static PoGo.NecroBot.Logic.Utils.PushNotificationClient;
using TinyIoC;

#endregion

namespace PoGo.NecroBot.Logic.State
{
    public interface ISession
    {
        ISettings Settings { get; set; }
        Inventory Inventory { get; }
        Client Client { get; }
        GetPlayerResponse Profile { get; set; }
        Navigation Navigation { get; }
        ILogicSettings LogicSettings { get; set; }
        ITranslation Translation { get; }
        IEventDispatcher EventDispatcher { get; }
        TelegramService Telegram { get; set; }
        SessionStats Stats { get; }
        IElevationService ElevationService { get; set; }
        List<FortData> Forts { get; set; }
        List<FortData> VisibleForts { get; set; }
        bool ReInitSessionWithNextBot(MultiAccountManager.BotAccount authConfig = null, double lat = 0, double lng = 0, double att = 0);
        void AddForts(List<FortData> mapObjects);
        void AddVisibleForts(List<FortData> mapObjects);
        Task<bool> WaitUntilActionAccept(BotActions action, int timeout = 30000);
        List<BotActions> Actions { get; }
        CancellationTokenSource CancellationTokenSource { get; set; }
        MemoryCache Cache { get; set; }
        DateTime LoggedTime { get; set; }
        DateTime CatchBlockTime { get; set; }
        Statistics RuntimeStatistics { get; }
        GymTeamState GymState { get; set; }
        double KnownLatitudeBeforeSnipe { get; set; }
        double KnownLongitudeBeforeSnipe { get; set; }
        bool SaveBallForByPassCatchFlee { set; get; }

    }

    public class Session : ISession
    {
        public Session(GlobalSettings globalSettings,ISettings settings, ILogicSettings logicSettings, IElevationService elevationService) : this(
           globalSettings, settings, logicSettings, elevationService, Common.Translation.Load(logicSettings))
        {
            LoggedTime = DateTime.Now;
        }

        public bool SaveBallForByPassCatchFlee { get; set; }

        public DateTime LoggedTime { get; set; }
        private List<AuthConfig> accounts;

        public List<BotActions> Actions
        {
            get { return this.botActions; }
        }

        public Session(GlobalSettings globalSettings, ISettings settings, ILogicSettings logicSettings,
            IElevationService elevationService, ITranslation translation)
        {
            this.GlobalSettings = globalSettings;
            this.CancellationTokenSource = new CancellationTokenSource();
            this.Forts = new List<FortData>();
            this.VisibleForts = new List<FortData>();
            this.Cache = new MemoryCache("Necrobot2");
            this.accounts = new List<AuthConfig>();
            this.EventDispatcher = new EventDispatcher();
            this.LogicSettings = logicSettings;
            this.RuntimeStatistics = new Statistics();

            this.ElevationService = elevationService;

            this.Settings = settings;

            this.Translation = translation;
            this.Reset(settings, LogicSettings);
            this.Stats = new SessionStats(this);
            this.accounts.AddRange(logicSettings.Bots);
            if (!this.accounts.Any(x => x.AuthType == settings.AuthType && x.Username == settings.Username))
            {
                this.accounts.Add(new AuthConfig()
                {
                    AuthType = settings.AuthType,
                    Password = settings.Password,
                    Username = settings.Username
                });
            }
            if (File.Exists("runtime.log"))
            {
                var lines = File.ReadAllLines("runtime.log");
                foreach (var item in lines)
                {
                    var arr = item.Split(';');
                    var acc = this.accounts.FirstOrDefault(p => p.Username == arr[0]);
                    if (acc != null)
                    {
                        acc.RuntimeTotal = Convert.ToDouble(arr[1]);
                    }
                }
            }

            GymState = new GymTeamState();
        }

        public List<FortData> Forts { get; set; }
        public List<FortData> VisibleForts { get; set; }
        public GlobalSettings GlobalSettings { get; set; }

        public ISettings Settings { get; set; }

        public Inventory Inventory { get; private set; }

        public Client Client { get; private set; }

        public GetPlayerResponse Profile { get; set; }
        public Navigation Navigation { get; private set; }

        public ILogicSettings LogicSettings { get; set; }

        public ITranslation Translation { get; }

        public IEventDispatcher EventDispatcher { get; }

        public TelegramService Telegram { get; set; }

        public SessionStats Stats { get; set; }

        public IElevationService ElevationService { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public MemoryCache Cache { get; set; }

        public List<AuthConfig> Accounts
        {
            get { return this.accounts; }
        }

        public DateTime CatchBlockTime { get; set; }
        public Statistics RuntimeStatistics { get; }
        private List<BotActions> botActions = new List<BotActions>();

        public void Reset(ISettings settings, ILogicSettings logicSettings)
        {
            this.KnownLatitudeBeforeSnipe = 0; 
            this.KnownLongitudeBeforeSnipe = 0;
            if(this.GlobalSettings.Auth.DeviceConfig.UseRandomDeviceId)
            {
                settings.DeviceId = DeviceConfig.GetDeviceId(settings.Username);
                Logger.Debug($"Username : {Settings.Username} , Device ID :{Settings.DeviceId}");
            }
            Client = new Client(settings);
            // ferox wants us to set this manually
            Inventory = new Inventory(this, Client, logicSettings, () =>
            {
                var candy = this.Inventory.GetPokemonFamilies().Result.ToList();
                var pokemonSettings = this.Inventory.GetPokemonSettings().Result.ToList();
                this.EventDispatcher.Send(new InventoryRefreshedEvent(null, pokemonSettings, candy));
            });
            Navigation = new Navigation(Client, logicSettings);
            Navigation.WalkStrategy.UpdatePositionEvent +=
                (session, lat, lng,s) => this.EventDispatcher.Send(new UpdatePositionEvent {Latitude = lat, Longitude = lng, Speed = s});

            Navigation.WalkStrategy.UpdatePositionEvent += LoadSaveState.SaveLocationToDisk;
        }

        //TODO : Need add BotManager to manage all feature related to multibot, 
        public bool ReInitSessionWithNextBot(MultiAccountManager.BotAccount bot = null, double lat = 0, double lng = 0, double att = 0)
        {
            this.CatchBlockTime = DateTime.Now; //remove any block
            MSniperServiceTask.BlockSnipe();
            this.VisibleForts.Clear();
            this.Forts.Clear();

            var manager = TinyIoCContainer.Current.Resolve<MultiAccountManager>();

            var nextBot = manager.GetSwitchableAccount(bot);

            this.Settings.AuthType = nextBot.AuthType;
            this.Settings.Password = nextBot.Password;
            this.Settings.Username = nextBot.Username;
            this.Settings.DefaultAltitude = att == 0 ? this.Client.CurrentAltitude : att;
            this.Settings.DefaultLatitude = lat == 0 ? this.Client.CurrentLatitude : lat;
            this.Settings.DefaultLongitude = lng == 0 ? this.Client.CurrentLongitude : lng;
            this.Stats = new SessionStats(this);
            this.Reset(this.Settings, this.LogicSettings);
            //CancellationTokenSource.Cancel();
            this.CancellationTokenSource = new CancellationTokenSource();

            this.EventDispatcher.Send(new BotSwitchedEvent(nextBot)
            {
            });

            if (this.LogicSettings.MultipleBotConfig.DisplayList)
            {
                manager.DumpAccountList();
            }
            return true;
        }

        public void AddForts(List<FortData> data)
        {
            data.RemoveAll(x => LocationUtils.CalculateDistanceInMeters(x.Latitude, x.Longitude, this.Settings.DefaultLatitude, this.Settings.DefaultLongitude) > 10000);

            this.Forts.RemoveAll(p => data.Any(x => x.Id == p.Id && x.Type == FortType.Checkpoint));
            this.Forts.AddRange(data.Where(x => x.Type == FortType.Checkpoint));
            foreach (var item in data.Where(p => p.Type == FortType.Gym))
            {
                var exist = this.Forts.FirstOrDefault(x => x.Id == item.Id);
                if (exist != null && exist.CooldownCompleteTimestampMs > DateTime.UtcNow.ToUnixTime())
                {
                    continue;
                }
                else
                {
                    this.Forts.RemoveAll(x => x.Id == item.Id);
                    this.Forts.Add(item);
                }
            }
        }

        public void AddVisibleForts(List<FortData> mapObjects)
        {
            var notexist = mapObjects.Where(p => !this.VisibleForts.Any(x => x.Id == p.Id));
            this.VisibleForts.AddRange(notexist);
        }

        public async Task<bool> WaitUntilActionAccept(BotActions action, int timeout = 30000)
        {
            if (botActions.Count == 0) return true;
            var waitTimes = 0;
            while (true && waitTimes < timeout)
            {
                if (botActions.Count == 0) return true;
                ///implement logic of action dependent
                waitTimes += 1000;
                await Task.Delay(1000);
            }
            return false; //timedout
        }
        public GymTeamState GymState { get; set; }

        public double KnownLatitudeBeforeSnipe { get; set; }
        public double KnownLongitudeBeforeSnipe { get; set; }
    }
}