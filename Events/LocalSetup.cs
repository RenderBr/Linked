using Auxiliary;
using Linked.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Linked.Events
{
    public class LocalSetup
    {
        public async void InitLocalPermissions()
        {
            var temp = await StorageProvider.GetMongoCollection<LocalPermissions>("LocalPermissions").FindAsync(Builders<LocalPermissions>.Filter.Empty);
            List<LocalPermissions> perms = await temp.ToListAsync();

            foreach(LocalPermissions perm in perms)
            {
                Group g = TShock.Groups.GetGroupByName(perm.Rank);
                if (g != null)
                {
                    TShock.Groups.AddPermissions(g.Name, perm.Allowed);
                    TShock.Groups.DeletePermissions(g.Name, perm.Negated);
                }
            }
        }

    }
}
