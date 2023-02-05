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

        public Ranks ranks;

        public Linked(Main game) : base(game)
        {
        }

        public async override void Initialize()
        {
            Configuration<LinkedSettings>.Load(nameof(Linked));
            settings = Configuration<LinkedSettings>.Settings;
            
            ranks = new Ranks();
            ranks.Initialize();

            GeneralHooks.ReloadEvent += (x) =>
            {
                Configuration<LinkedSettings>.Load(nameof(Linked));
                x.Player.SendSuccessMessage("[Linked] Reloaded configuration.");
            };

            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
            PlayerHooks.PlayerPostLogin += PostLogin;
            Commands.ChatCommands.Add(new Command("tbc.admin", Sync, "sync"));
        }

        private void Sync(CommandArgs args)
        {
            ranks.Initialize();
            args.Player.SendSuccessMessage("Ranks have been synced with the database.");
        }

        private async void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            if(settings.IsDataCentral == false)
            {
                    var player = TShock.Players[args.Who];
                    if (player == null)
                        return;

                    LinkedPlayerData? data = await IModel.GetAsync(GetRequest.Bson<LinkedPlayerData>(x => x.UUID == player.UUID));
                    if (data == null) { player.Disconnect("Please create an account on the main TBC server first. (/hub)"); return; }

                    var account = TShock.UserAccounts.GetUserAccountByName(data.Account.Name);

                    account = data.Account;
                    player.Account = account;
                    player.Group = TShock.Groups.GetGroupByName(account.Group);

                    player.SendSuccessMessage($"Welcome back, {player.Name}!");
            }
        }

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