using Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.DB;

namespace Linked.Models
{
    public class LinkedPlayerData : BsonModel
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
