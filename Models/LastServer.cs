using Auxiliary;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace Linked.Models
{
    public class LastServer : LinkedModel
    {
        public static LastServer ServerDetails(UserAccount acc)
            => new()
            {
                Account = acc,
                LastServerName = TShock.Config.Settings.ServerName,
                LastServerAddress = Main.getIP,
                LastServerPort = Netplay.ListenPort
            };


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

        private UserAccount _acc = new();

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
