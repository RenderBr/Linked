using Auxiliary;

namespace Linked.Models
{
    public class LinkedRankData : LinkedModel
    {
        private string _name = string.Empty;
        public string Name
        {
            get
              => _name;
            set
            {
                _ = this.SaveAsync(x => x.Name, value);
                _name = value;
            }
        }

        private Rank _group = new();

        public Rank Group
        {
            get
             => _group;
            set
            {
                _ = this.SaveAsync(x => x.Group, value);
                _group = value;
            }
        }

    }

    public class Rank : BsonModel
    {
        private string _name = String.Empty;
        public string Name
        {
            get
              => _name;
            set
            {
                _ = this.SaveAsync(x => x.Name, value);
                _name = value;
            }
        }

        private int _r;
        public int R
        {
            get
             => _r;
            set
            {
                _ = this.SaveAsync(x => x.R, value);
                _r = value;
            }
        }

        private int _g;
        public int G
        {
            get
             => _g;
            set
            {
                _ = this.SaveAsync(x => x.G, value);
                _g = value;
            }
        }

        private int _b;
        public int B
        {
            get
            => _b;
            set
            {
                _ = this.SaveAsync(x => x.B, value);
                _b = value;
            }
        }

        private string _parent = String.Empty;
        public string Parent
        {
            get => _parent;
            set { _ = this.SaveAsync(x => x.Parent, value); _parent = value; }
        }

        private string _prefix = String.Empty;
        public string Prefix
        {
            get => _prefix;
            set { _ = this.SaveAsync(x => x.Parent, value); _prefix = value; }
        }

        private string _suffix = String.Empty;

        public string Suffix
        {
            get => _suffix;
            set { _ = this.SaveAsync(x => x.Suffix, value); _suffix = value; }
        }

        private string _perms = String.Empty;
        public string Permissions
        {
            get => _perms;
            set { _ = this.SaveAsync(x => x.Permissions, value); _perms = value; }
        }
    }
}
