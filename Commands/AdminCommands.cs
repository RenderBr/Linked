using Auxiliary;
using CSF;
using CSF.TShock;
using Linked.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            await RespondAsync("Syncing ranks...");
            try
            {
                Linked.ranks.Initialize();
                Linked.local.InitLocalPermissions();
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
            if (group == "" && (sub != "" || sub != "help"))
                return Error("Please enter a group name to modify!");

            if (perm == "" && (sub != "" || sub != "help"))
                return Error("Please enter a permission node!");
            
            LocalPermissions? grp = await IModel.GetAsync(GetRequest.Bson<LocalPermissions>(x=>x.Rank==group));
            if(grp is null)
            {
                var temp = await IModel.GetAsync(GetRequest.Linked<LinkedRankData>(x => x.Name == group));
                if (temp != null)
                {
                    grp = await IModel.GetAsync(GetRequest.Bson<LocalPermissions>(x => x.Rank == group), x => { x.Rank = group; }) ;
                }
                else return Error("That group does not exist!");
            }
            
            switch (sub)
            {
                case "addperm":
                case "add":
                case "allow":
                    {
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
                case "negate":
                    {
                        List<string> temp = grp.Negated;
                        temp.Add(perm);
                        grp.Negated = temp;
                        TShock.Groups.DeletePermissions(grp.Rank, new List<string>() { perm });
                        return Success("The permission was negated locally!");
                    }
                default:
                case "help":
                    Info("All /pm commands -");
                    Info("/pm negate <group> <perm> - removes a permission locally");
                    Info("/pm allow <group> <perm> - allows a permission locally");
                    return ExecuteResult.FromSuccess();
            }
        }

    }
}
