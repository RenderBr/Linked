using Auxiliary;
using Linked.Models;
using MongoDB.Driver;
using TShockAPI;

namespace Linked.Events
{
    public class LocalSetup
    {
        #region Initialize Local Permissions
        public async void InitLocalPermissions()
        {
            // retrieve all local permissions and store as a list
            var temp = await StorageProvider.GetMongoCollection<LocalPermissions>("LocalPermissions").FindAsync(Builders<LocalPermissions>.Filter.Empty);
            List<LocalPermissions> perms = await temp.ToListAsync();

            // loop through each perm
            foreach (LocalPermissions perm in perms)
            {
                // if group exists, add/negate permissions, otherwise, don't and move onto next group
                Group g = TShock.Groups.GetGroupByName(perm.Rank);
                if (g != null)
                {
                    TShock.Groups.AddPermissions(g.Name, perm.Allowed);
                    TShock.Groups.DeletePermissions(g.Name, perm.Negated);
                }
                else continue;
            }
        }
        #endregion
    }
}
