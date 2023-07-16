using Auxiliary;
using CSF;
using CSF.TShock;
using Linked.Models;
using TShockAPI;

namespace Linked
{
    [RequirePermission("tbc.admin")]
    public class AdminCommands : TSModuleBase<TSCommandContext>
    {

        [Command("sync", "lsync", "linkedsync")]
        [Description("Syncs the local server's ranks with the central database")]
        public async Task<IResult> SyncRanks()
        {
            // let the player know
            await RespondAsync("Syncing ranks...");
            try // it will either succeed or send a fail message if an error is found
            {
                await Linked.Manager.InitializeServer();
                await Linked.Manager.SetupLocalPerms();
                return Success("Synced ranks!");
            }
            catch
            {
                return Error("Failed to sync ranks!");
            }
        }

        [Command("pm", "perms", "lperm", "permmanager")]
        [Description("Allows the negation and allowance of certain permissions to groups. This is done locally.")]
        public async Task<IResult> PermManager(string sub = "", string group = "", string perm = "")
        {
            // retrieve group
            LocalPermissions? grp = await IModel.GetAsync(GetRequest.Bson<LocalPermissions>(x => x.Rank == group));
            if (grp is null) // if it cannot be retrieved
            {
                // check if it exists in linked DB
                var temp = await IModel.GetAsync(GetRequest.Linked<LinkedRankData>(x => x.Name == group));
                if (temp != null) // if it does, create it in linked permissions
                {
                    grp = await IModel.GetAsync(GetRequest.Bson<LocalPermissions>(x => x.Rank == group), x => { x.Rank = group; });
                }
                else return Error("That group does not exist!"); // otherwise, return an error (probably user mispelt group name)
            }

            switch (sub) // switch between various command arguments
            {
                case "addperm": // user is attempting to add a local perm
                case "add":
                case "allow":
                    {
                        if (group == "" || grp == null)
                            return Error("You must specify a valid group!");
                        if (perm == "")
                            return Error("You must specify a permission!");

                        List<string> temp = grp.Allowed;
                        temp.Add(perm);
                        grp.Allowed = temp;
                        TShock.Groups.AddPermissions(grp.Rank, new List<string>() { perm });
                        return Success("The permission was added locally!");
                    }
                case "del":
                case "delperm":
                case "delete":
                case "rem":
                case "remove":
                case "negate": // user is attempting to negate a local perm
                    {
                        if (group == "" || grp == null)
                            return Error("You must specify a valid group!");
                        if (perm == "")
                            return Error("You must specify a permission!");
                        List<string> temp = grp.Negated;
                        temp.Add(perm);
                        grp.Negated = temp;
                        TShock.Groups.DeletePermissions(grp.Rank, new List<string>() { perm });
                        return Success("The permission was negated locally!");
                    }
                default:
                case "help": // user either entered no args or wrote "help"
                    Info("All /pm commands -");
                    Info("/pm negate <group> <perm> - removes a permission locally");
                    Info("/pm allow <group> <perm> - allows a permission locally");
                    return ExecuteResult.FromSuccess();
            }
        }

    }
}
