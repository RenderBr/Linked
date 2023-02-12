using Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linked.Models
{
    internal class LocalPermissions : BsonModel
    {
        private string _rank = string.Empty;
        public string Rank
        {
            get
              => _rank;
            set
            {
                _ = this.SaveAsync(x => x.Rank, value);
                _rank = value;
            }
        }

        private List<string> _negated = new List<string>();

        public List<string> Negated
        {
            get
              => _negated;
            set
            {
                _ = this.SaveAsync(x => x.Negated, value);
                _negated = value;
            }
        }

        private List<string> _allowed = new List<string>();

        public List<string> Allowed
        {
            get
              => _allowed;
            set
            {
                _ = this.SaveAsync(x => x.Allowed, value);
                _allowed = value;
            }
        }


    }
}
