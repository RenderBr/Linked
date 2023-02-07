using Auxiliary;
using Auxiliary.Configuration;
using Linked.Events;
using Linked.Models;
using System.Configuration;
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
        public override string Name => "Linked";
        public override string Description => "A facilitator for connecting server ranks & player accounts together";
        public override string Author => "Average";
        public override Version Version => new Version(1, 0, 0);

        public static LinkedSettings settings;

        public Ranks ranks = new Ranks();

        public Linked(Main game) : base(game)
        {
        }

        public async override void Initialize()
        {
            // load Linked.json and store it in settings
            Configuration<LinkedSettings>.Load(nameof(Linked));
            settings = Configuration<LinkedSettings>.Settings;

            // initial sync of ranks, from DB (central) -> server (local)
            ranks.Initialize();

            // register reload event
            GeneralHooks.ReloadEvent += (x) =>
            {
                Configuration<LinkedSettings>.Load(nameof(Linked)); // load config
                x.Player.SendSuccessMessage("[Linked] Reloaded configuration.");
            };

            // register events
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
            PlayerHooks.PlayerPostLogin += PostLogin;

            // register /lsync command
            Commands.ChatCommands.Add(new Command("tbc.admin", Sync, "sync", "linkedsync", "lsync"));
        }


        #region Sync Command
        private void Sync(CommandArgs args)
        {
            ranks.Initialize(); // re-sync ranks from DB (central) -> server (local)
            args.Player.SendSuccessMessage("Ranks have been synced with the database.");
            return;
        }
        #endregion

        #region On Player Join
        //when a player joins the server
        private async void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            if(settings.IsDataCentral == false) // if the server is not the data centre
            {
                    var player = TShock.Players[args.Who]; // grab player
                    if (player == null) // if can't be found, maybe they did not connect properly, return
                        return;

                    // grab player data from central, based on UUID
                    LinkedPlayerData? data = await IModel.GetAsync(GetRequest.Bson<LinkedPlayerData>(x => x.UUID == player.UUID));

                    // if the data cannot be found, kick the player. They should have an account on data centre before they connect
                    // to survival. This essentially means, they do not have an account on the main server. (unregistered)
                    if (data == null) { player.Disconnect("Please create an account on the main TBC server first. (/hub)"); return; }

                    // attempt to grab an already created account (they have joined before, and their data is already synced
                    var account = TShock.UserAccounts.GetUserAccountByName(data.Account.Name);
                    if(account == null) // if the account cannot be found, create it
                    {
                        UserAccount temp = data.Account;
                        TShock.UserAccounts.AddUserAccount(temp);
                        account = TShock.UserAccounts.GetUserAccountByName(data.Account.Name);
                    }

                    // set the player's account and group
                    player.Account = account;
                    player.Group = TShock.Groups.GetGroupByName(account.Group);

                    // greet the player
                    player.SendSuccessMessage($"Welcome back, {player.Name}!");
            }
        }
        #endregion

        public async void PostLogin(PlayerPostLoginEventArgs e)
        {
            var player = e.Player;
            if (player.IsLoggedIn == false)
                return;

            if (settings.IsDataCentral == true)
            {
                LinkedPlayerData? data = await IModel.GetAsync(GetRequest.Bson<LinkedPlayerData>(x => x.UUID == player.UUID), x =>
                {
                    x.UUID = player.UUID;
                    x.Account = player.Account;
                });
            }
            else {
                LinkedPlayerData? data = await IModel.GetAsync(GetRequest.Bson<LinkedPlayerData>(x => x.UUID == player.UUID));
                if (data == null)
                    return;


            }

        }
    }
}