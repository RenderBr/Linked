using Auxiliary;
using Linked.Models;
using MongoDB.Driver;
using TShockAPI;

namespace Linked.Events
{
    public class Ranks
    {
        #region Initialize Ranks
        public async Task Initialize()
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
                    if (rank.Group.Name == "superadmin")
                        continue;
                    // store some initial variables to reduce boilerplate
                    string parent = rank.Group.Parent == null ? "" : rank.Group.Parent;
                    string chatcolor = $"{rank.Group.R},{rank.Group.G},{rank.Group.B}";

                    if (TShock.Groups.GroupExists(rank.Group.Name)) // check if exists, and if it does just modify it appropriately
                    {
                        TShock.Groups.UpdateGroup(rank.Group.Name, "", rank.Group.Permissions, chatcolor, rank.Group.Suffix, rank.Group.Prefix);
                        continue;
                    }
                    // if it does not exist, add the group
                    TShock.Groups.AddGroup(rank.Group.Name, "", rank.Group.Permissions, chatcolor);
                    TShock.Groups.UpdateGroup(rank.Group.Name, "", rank.Group.Permissions, chatcolor, rank.Group.Suffix, rank.Group.Prefix);
                }
                Console.WriteLine("All ranks have been pre-initialized locally!");
                await SetupParenting();
            }


        }
        
        // must be done after rank initializations
        public async Task SetupParenting()
        {
            Console.WriteLine("Parents are being inherited by their respective ranks...");
            var temp = await StorageProvider.GetLinkedCollection<LinkedRankData>("LinkedRankDatas").FindAsync(Builders<LinkedRankData>.Filter.Empty);
            List<LinkedRankData> ranks = await temp.ToListAsync();

            foreach (LinkedRankData rank in ranks) // loop through each rank
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
