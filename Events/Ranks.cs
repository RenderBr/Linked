using Auxiliary;
using Linked.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Linked.Events
{
    public class Ranks
    {
        public async void Initialize()
        {
            if(Linked.settings.IsDataCentral == true)
            {
                foreach (Group group in TShock.Groups.groups)
                {
                    LinkedRankData? data = await IModel.GetAsync(GetRequest.Linked<LinkedRankData>(x => x.Name == group.Name), x =>
                    {
                        x.Name = group.Name;
                        x.Group = new Rank {
                            Name = group.Name,
                            Prefix = group.Prefix,
                            R = group.R,
                            G = group.G,
                            B = group.B,
                            Parent = (group.Parent == null ? "" : group.Parent.Name),
                            Suffix = group.Suffix,
                            Permissions = group.Permissions
                        };
                       

                    });
                }
                return;
            }
            else
            {
                var temp = await StorageProvider.GetLinkedCollection<LinkedRankData>("LinkedRankDatas").FindAsync(Builders<LinkedRankData>.Filter.Empty);

                List<LinkedRankData> ranks = await temp.ToListAsync();

                foreach(LinkedRankData rank in ranks)
                {
                    string parent = (rank.Group.Parent == null ? "" : rank.Group.Parent);
                    string chatcolor = $"{rank.Group.R},{rank.Group.B},{rank.Group.G}";

                    if (TShock.Groups.GroupExists(rank.Group.Name))
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
                    TShock.Groups.AddGroup(rank.Group.Name, parent, rank.Group.Permissions, chatcolor);
                    TShock.Groups.UpdateGroup(rank.Group.Name, parent, rank.Group.Permissions, chatcolor, rank.Group.Suffix, rank.Group.Prefix);
                }
            }

         
        }
    }
}
