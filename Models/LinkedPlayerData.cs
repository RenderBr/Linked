using Auxiliary;
using TShockAPI.DB;

namespace Linked.Models
{
    public class LinkedPlayerData : LinkedModel
    {
        private string _uuid = string.Empty;
        public string UUID
        {
            get
              => _uuid;
            set
            {
                _ = this.SaveAsync(x => x.UUID, value);
                _uuid = value;
            }
        }

        private UserAccount _acc;

        public UserAccount Account
        {
            get
              => _acc;
            set
            {
                _ = this.SaveAsync(x => x.Account, value);
                _acc = value;
            }
        }
    }
}
