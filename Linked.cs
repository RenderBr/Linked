using Auxiliary.Configuration;
using CSF;
using CSF.TShock;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace Linked
{
    [ApiVersion(2, 1)]
    public class Linked : TerrariaPlugin
    {
        #region Plugin metadata
        public override string Name => "Linked";
        public override string Description => "A facilitator for connecting multi-server ranks & player accounts together";
        public override string Author => "Average";
        public override Version Version => new(1, 1, 0);
        #endregion
        #region Linked Management Properties
        public static LinkedManager Manager { get; set; } = new();
        public static LinkedSettings Settings { get; set; } = new();
        #endregion

        private readonly TSCommandFramework _fx;
        public Linked(Main game) : base(game)
        {
            _fx = new(new CommandConfiguration()
            {
                DoAsynchronousExecution = false
            });
        }

        #region Initialization & Hooks
        public async override void Initialize()
        {
            // load Linked.json and store it in settings
            Configuration<LinkedSettings>.Load(nameof(Linked));
            Settings = Configuration<LinkedSettings>.Settings;

            // build command modules
            await _fx.BuildModulesAsync(typeof(Linked).Assembly);

            // initial sync of ranks, from DB (central) -> server (local)
            await Manager.InitializeServer();

            //disable / enable registrations
            Manager.InitializeRegistrations();

            // register events
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            PlayerHooks.PlayerPostLogin += PostLogin;

            // initial local permission, addition and negation
            await Manager.SetupLocalPerms();
        }
        #endregion

        #region Reload Method
        private async void Reload()
        {
            Configuration<LinkedSettings>.Load(nameof(Linked));
            Settings = Configuration<LinkedSettings>.Settings;

            Manager.InitializeRegistrations();

            await Manager.InitializeServer();
            await Manager.SetupLocalPerms();
        }
        #endregion

        #region On Player Join
        //when a player joins the server
        private async void OnServerJoin(JoinEventArgs args)
        {
            TShock.Groups.LoadPermisions();
            if (Settings.IsDataCentral == false) // if the server is not the data centre
            {
                var player = TShock.Players[args.Who]; // grab player
                if (player == null) // if can't be found, maybe they did not connect properly, return
                    return;

                // grab player data from central, based on UUID & IP
                var data = await Manager.SafeGet(player);

                // if player is not registered, handle it based on cfg values
                if (await Manager.HandleUnregistered(player, data) == true)
                    return;

                // preventing null references
                if (data == null)
                    return;

                // attempt to grab an already created account (they have joined before, and their data is already synced)
                UserAccount account = TShock.UserAccounts.GetUserAccountByName(data.Account.Name);

                // if the account is null, create a new one
                account = account == null ? Manager.CreateLocalUser(data) : account;

                // set the user's group appropriately to the respective account
                TShock.UserAccounts.SetUserGroup(account, data.Account.Group);

                // set the player's SSC data
                player.PlayerData = TShock.CharacterDB.GetPlayerData(player, account.ID);

                // if auto login is disabled, return
                // let the user login themselves?
                if (Settings.AutoLogin == false)
                    return;

                // set the player's account and group (if auto login enabled)
                // AKA: actually log them in
                Manager.SyncPlayerToAccount(player, account);

                // greet the player
                if (Settings.DoGreetPlayer)
                    player.SendSuccessMessage($"Welcome back, {player.Name}!");

            }
        }
        #endregion

        #region On Player Login
        public async void PostLogin(PlayerPostLoginEventArgs e)
        {
            // retrieve the player
            var player = e.Player;
            if (player.IsLoggedIn == false) // if they are for some reason not logged in anymore, return
                return;

            await Manager.CreateOrUpdate(e.Player);

        }
        #endregion

        #region On Player Leave
        private async void OnLeave(LeaveEventArgs args)
        {
            TSPlayer player = TShock.Players[args.Who];
            if (player == null)
                return;
            if (player.IsLoggedIn == false)
                return;

            //update the player account
            await Manager.HandleLastServer(player);
        }
        #endregion
    }
}