using Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.DB;

namespace Linked.Models
{
    public class LastServer : LinkedModel
    {
        private int _lastServerPort;
        public int LastServerPort
        {
            get
              => _lastServerPort;
            set
            {
                _ = this.SaveAsync(x => x.LastServerPort, value);
                _lastServerPort = value;
            }
        }

        private string _lastServerName = string.Empty;
        public string LastServerName
        {
            get => _lastServerName;
            set
            {
                _ = this.SaveAsync(x => x.LastServerName, value);
                _lastServerName = value;
            }
        }

        private string _lastServerAddress = string.Empty;
        public string LastServerAddress
        {
            get => _lastServerAddress;
            set
            {
                _ = this.SaveAsync(x => x.LastServerAddress, value);
                _lastServerAddress = value;
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
