using Auxiliary;
using Linked.Models;
using MongoDB.Driver;
using TShockAPI;

namespace Linked.Events
{
    public class Ranks
    {
        #region Initialize Ranks
        public async void Initialize()
        {
            if (Linked.settings.IsDataCentral == true) // if data centre
            {
                foreach (Group group in TShock.Groups.groups) // loop through each group
                {
                    LinkedRankData? data = await IModel.GetAsync(GetRequest.Linked<LinkedRankData>(x => x.Name == group.Name), x =>
                    {
                        x.Name = group.Name;
                        x.Group = new Rank
                        {
                            Name = group.Name,
                            Prefix = group.Prefix,
                            R = group.R,
                            G = group.G,
                            B = group.B,
                            Parent = group.Parent == null ? "" : group.Parent.Name,
                            Suffix = group.Suffix,
                            Permissions = group.Permissions
                        };
                        // and add them to the linked database if they don't exist

                    });
                }
                return;
            }
            else // if not data centre
            {
                // retrieve all linked rank data and store it as a list
                var temp = await StorageProvider.GetLinkedCollection<LinkedRankData>("LinkedRankDatas").FindAsync(Builders<LinkedRankData>.Filter.Empty);
                List<LinkedRankData> ranks = await temp.ToListAsync();

                foreach (LinkedRankData rank in ranks) // loop through each rank
                {
                    // store some initial variables to reduce boilerplate
                    string parent = rank.Group.Parent == null ? "" : rank.Group.Parent;
                    string chatcolor = $"{rank.Group.R},{rank.Group.B},{rank.Group.G}";

                    if (TShock.Groups.GroupExists(rank.Group.Name)) // check if exists, and if it does just modify it appropriately
                    {
                        Group group = TShock.Groups.GetGroupByName(rank.Name);
                        group.Parent = TShock.Groups.GetGroupByName(rank.Group.Parent == null ? "" : rank.Group.Parent);
                        group.Permissions = rank.Group.Permissions;
                        group.R = (byte)rank.Group.R;
                        group.B = (byte)rank.Group.B;
                        group.G = (byte)rank.Group.G;
                        group.Prefix = rank.Group.Prefix;
                        group.Suffix = rank.Group.Suffix;
                        group.ChatColor = chatcolor;
                        continue;
                    }
                    // if it does not exist, add the group
                    TShock.Groups.AddGroup(rank.Group.Name, parent, rank.Group.Permissions, chatcolor);
                    TShock.Groups.UpdateGroup(rank.Group.Name, parent, rank.Group.Permissions, chatcolor, rank.Group.Suffix, rank.Group.Prefix);
                }
            }


        }
        #endregion
    }
}
