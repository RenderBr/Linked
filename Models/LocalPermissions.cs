using Auxiliary;

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

        private List<string> _negated = new();

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

        private List<string> _allowed = new();

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
