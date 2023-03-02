using Auxiliary;
using Auxiliary.Configuration;
using CSF;
using CSF.TShock;
using Linked.Events;
using Linked.Models;
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
        public override string Description => "A facilitator for connecting server ranks & player accounts together";
        public override string Author => "Average";
        public override Version Version => new Version(1, 0, 0);
        #endregion
        public static LinkedSettings settings;
        public static Ranks ranks = new Ranks();
        public static LocalSetup local = new LocalSetup();
        private readonly TSCommandFramework _fx;

        
        public Linked(Main game) : base(game)
        {
            _fx = new(new CommandConfiguration()
            {
                DoAsynchronousExecution = false
            });
        }

        #region Initialization
        public async override void Initialize()
        {            
            // load Linked.json and store it in settings
            Configuration<LinkedSettings>.Load(nameof(Linked));
            settings = Configuration<LinkedSettings>.Settings;

            // build command modules
            await _fx.BuildModulesAsync(typeof(Linked).Assembly);

            // initial sync of ranks, from DB (central) -> server (local)
            await ranks.Initialize();

            //disable / enable registrations
            if (settings.DisableRegistrations == true)
                TShock.Groups.DeletePermissions("default", new List<string>() { "tshock.account.register" });
            else
                TShock.Groups.AddPermissions("default", new List<string>() { "tshock.account.register" });
           
            // register reload event
            GeneralHooks.ReloadEvent += (x) =>
            {
                Reload();
                x.Player.SendSuccessMessage("[Linked] Reloaded configuration.");
            };

            // register events
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
            PlayerHooks.PlayerPostLogin += PostLogin;

            // initial local permission, addition and negation
            await local.InitLocalPermissions();
        }
        #endregion

        #region Reload Method
        private void Reload()
        {
            Configuration<LinkedSettings>.Load(nameof(Linked));
            settings = Configuration<LinkedSettings>.Settings;

            if(settings.DisableRegistrations == true)
                TShock.Groups.DeletePermissions("default", new List<string>() { "tshock.account.register" });
            else
                TShock.Groups.AddPermissions("default", new List<string>() { "tshock.account.register" });

            ranks.Initialize();
            local.InitLocalPermissions();
        }
        #endregion

        #region On Player Join
        //when a player joins the server
        private async void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            if (settings.IsDataCentral == false) // if the server is not the data centre
            {
                var player = TShock.Players[args.Who]; // grab player
                if (player == null) // if can't be found, maybe they did not connect properly, return
                    return;

                // grab player data from central, based on UUID & IP
                // note: the reason we are using IP is to prevent UUID spoofing, which would allow users to login to other accounts
                LinkedPlayerData? data = await IModel.GetAsync(GetRequest.Linked<LinkedPlayerData>(x => x.UUID == player.UUID && x.Account.KnownIps.Contains(player.IP)));

                bool shouldKick = ((await IModel.GetAsync(GetRequest.Linked<LinkedPlayerData>(x => x.Account.KnownIps.Contains(player.IP))) == null ? true : false));
                
                // if the data cannot be found, kick the player. They should have an account on data centre before they connect
                // to survival. This essentially means, they do not have an account on the main server. (unregistered)
                if (data == null && (settings.ForceAccountMadeAlready && shouldKick)) { player.Disconnect("Please create an account on the main TBC server first. (/hub)"); return; }

                // attempt to grab an already created account (they have joined before, and their data is already synced
                var account = TShock.UserAccounts.GetUserAccountByName(data.Account.Name);
                if (account == null) // if the account cannot be found, create it
                {
                    UserAccount temp = new UserAccount()
                    {
                        Name = data.Account.Name,
                        Group = data.Account.Group
                    };

                    TShock.UserAccounts.AddUserAccount(temp);
                    account = TShock.UserAccounts.GetUserAccountByName(data.Account.Name);

                    TShock.UserAccounts.SetUserAccountUUID(account, data.Account.UUID);
                    TShock.UserAccounts.SetUserGroup(account, data.Account.Group);
                    TShock.UserAccounts.SetUserAccountPassword(account, data.Account.Password);
                }
                else
                {
                    TShock.UserAccounts.SetUserGroup(account, data.Account.Group);
                }

                player.PlayerData = TShock.CharacterDB.GetPlayerData(player, account.ID);

                // set the player's account and group (if auto login enabled)
                if (settings.AutoLogin == false)
                    return;
                
                player.Account = account;
                player.Group = TShock.Groups.GetGroupByName(account.Group);
                player.tempGroup = null;
                player.IsLoggedIn = true;
                player.IsDisabledForSSC = false;

                if (Main.ServerSideCharacter)
                {
                    if (player.HasPermission(Permissions.bypassssc))
                    {
                        player.PlayerData.CopyCharacter(player);
                        TShock.CharacterDB.InsertPlayerData(player);
                    }
                    player.PlayerData.RestoreCharacter(player);
                }
                player.LoginFailsBySsi = false;

                // greet the player
#if DEBUG
                player.SendSuccessMessage($"Welcome back, {player.Name}!");
#endif
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

            if (settings.IsDataCentral == true) // if central server
            {
                // retrieve player data, OR CREATE IT
                LinkedPlayerData? data = await IModel.GetAsync(GetRequest.Linked<LinkedPlayerData>(x => x.UUID == player.UUID), x =>
                {
                    x.UUID = player.UUID;
                    x.Account = player.Account;
                });

                //update the player account
                data.Account = player.Account;
            }

        }
        #endregion
    }
}