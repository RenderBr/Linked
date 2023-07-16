using Auxiliary;
using Linked.Models;
using MongoDB.Driver;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace Linked
{
    public class LinkedManager
    {
        public LinkedSettings Settings => Linked.Settings;

        // note: the reason we are using IP is to prevent UUID spoofing, which would allow users to login to other accounts
        public async Task<LinkedPlayerData?> SafeGet(TSPlayer player) => await IModel.GetAsync(GetRequest.Linked<LinkedPlayerData>(x => x.UUID == player.UUID && x.Account.KnownIps.Contains(player.IP))) ?? null;
        public void InitializeRegistrations()
        {
            if (Settings.DisableRegistrations == true)
                TShock.Groups.DeletePermissions("default", new List<string>() { "tshock.account.register" });
            else
                TShock.Groups.AddPermissions("default", new List<string>() { "tshock.account.register" });
        }

        public async Task CreateOrUpdate(TSPlayer player)
        {
            if (Settings.IsDataCentral == true) // if central server
            {
                // retrieve player data, OR CREATE IT
                LinkedPlayerData? data = await IModel.GetAsync(GetRequest.Linked<LinkedPlayerData>(x => x.UUID == player.UUID), x =>
                {
                    x.UUID = player.UUID;
                    x.Account = player.Account;
                });

                //update the player account to data central
                if (data != null)
                    data.Account = player.Account;
            }
        }

        public async Task SetupLocalPerms()
        {
            var localPermissionsCollection = StorageProvider.GetMongoCollection<LocalPermissions>("LocalPermissions");
            var perms = await localPermissionsCollection.Find(Builders<LocalPermissions>.Filter.Empty).ToListAsync();

            foreach (var perm in perms)
            {
                var group = TShock.Groups.GetGroupByName(perm.Rank);
                if (group != null)
                {
                    TShock.Groups.AddPermissions(group.Name, perm.Allowed);
                    TShock.Groups.DeletePermissions(group.Name, perm.Negated);
                }
            }
        }

        public void SyncPlayerToAccount(TSPlayer player, UserAccount account)
        {
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
        }

        public UserAccount CreateLocalUser(LinkedPlayerData data)
        {
            UserAccount temp = new()
            {
                Name = data.Account.Name,
                Group = data.Account.Group
            };

            TShock.UserAccounts.AddUserAccount(temp);
            var account = TShock.UserAccounts.GetUserAccountByName(data.Account.Name);

            TShock.UserAccounts.SetUserAccountUUID(account, data.Account.UUID);
            TShock.UserAccounts.SetUserGroup(account, data.Account.Group);
            TShock.UserAccounts.SetUserAccountPassword(account, data.Account.Password);
            return account;
        }

        public async Task<bool> HandleUnregistered(TSPlayer player, LinkedPlayerData? data)
        {
            // do they have a pre-existing account?
            bool shouldKick = await IModel.GetAsync(GetRequest.Linked<LinkedPlayerData>(x => x.Account.KnownIps.Contains(player.IP))) == null ? true : false;

            // depending on config, if the data cannot be found, kick the player. They should have an account on data centre before they connect
            // to subsequent servers. This essentially means, they do not have an account on the main server. (unregistered)
            if (data == null && Settings.ForceAccountMadeAlready && shouldKick) { player.Disconnect("Please create an account on the main server first. (/lobby)"); }
            return shouldKick;

        }

        public async Task HandleLastServer(TSPlayer player)
        {
            LastServer details = LastServer.ServerDetails(player.Account);

            var lastServer = await IModel.GetAsync(GetRequest.Linked<LastServer>(x => x.Account.ID == player.Account.ID), x => { x = details; });
            if (lastServer != details)
                lastServer = details;
        }

        #region Initialize Server
        public async Task InitializeServer()
        {
            if (Linked.Settings.IsDataCentral)
            {
                foreach (Group group in TShock.Groups.groups)
                {
                    await IModel.GetAsync(GetRequest.Linked<LinkedRankData>(x => x.Name == group.Name), x =>
                    {
                        x.Name = group.Name;
                        x.Group = new Rank
                        {
                            Name = group.Name,
                            Prefix = group.Prefix,
                            R = group.R,
                            G = group.G,
                            B = group.B,
                            Parent = group.Parent?.Name ?? "",
                            Suffix = group.Suffix,
                            Permissions = group.Permissions
                        };
                    });
                }
                return;
            }
            else
            {
                var ranks = await StorageProvider.GetLinkedCollection<LinkedRankData>("LinkedRankDatas").Find(Builders<LinkedRankData>.Filter.Empty).ToListAsync();

                foreach (LinkedRankData rank in ranks)
                {
                    if (rank.Group.Name == "superadmin")
                        continue;

                    string parent = rank.Group.Parent?.ToString() ?? "";
                    string chatcolor = $"{rank.Group.R},{rank.Group.G},{rank.Group.B}";

                    if (TShock.Groups.GroupExists(rank.Group.Name))
                    {
                        TShock.Groups.UpdateGroup(rank.Group.Name, "", rank.Group.Permissions, chatcolor, rank.Group.Suffix, rank.Group.Prefix);
                        continue;
                    }

                    TShock.Groups.AddGroup(rank.Group.Name, "", rank.Group.Permissions, chatcolor);
                    TShock.Groups.UpdateGroup(rank.Group.Name, "", rank.Group.Permissions, chatcolor, rank.Group.Suffix, rank.Group.Prefix);
                }

                Console.WriteLine("All ranks have been pre-initialized locally!");
                await SetupParenting();
            }
        }

        public async Task SetupParenting()
        {
            Console.WriteLine("Parents are being inherited by their respective ranks...");
            var ranks = await StorageProvider.GetLinkedCollection<LinkedRankData>("LinkedRankDatas").Find(Builders<LinkedRankData>.Filter.Empty).ToListAsync();

            foreach (LinkedRankData rank in ranks)
            {
                if (rank.Group.Name == "superadmin")
                    continue;

                string chatcolor = $"{rank.Group.R},{rank.Group.G},{rank.Group.B}";
                TShock.Groups.UpdateGroup(rank.Group.Name, rank.Group.Parent, rank.Group.Permissions, chatcolor, rank.Group.Suffix, rank.Group.Prefix);
            }

            Console.WriteLine("Ranks have been fully initialized locally!");
        }
        #endregion
    }
}
