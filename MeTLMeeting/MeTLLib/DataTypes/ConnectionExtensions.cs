using System.Collections.Generic;
namespace MeTLLib.DataTypes
{
    public class Credentials
    {
        public string name;
        public string password;
        public List<AuthorizedGroup> authorizedGroups;
    }
    public class AuthorizedGroup
    {
        public AuthorizedGroup() { }
        public AuthorizedGroup(string newGroupKey)
        {
            groupType = "test only";
            groupKey = newGroupKey;
        }
        public AuthorizedGroup(string newGroupKey, string newGroupType)
        {
            groupType = newGroupType;
            groupKey = newGroupKey;
        }
        public string groupType { get; set; }
        public string groupKey { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is AuthorizedGroup)) return false;
            var otherGroup = (AuthorizedGroup)obj;
            return otherGroup.groupKey == this.groupKey;
        }
        public override int GetHashCode()
        {
            return groupKey.GetHashCode();
        }
    }
    public class Location
    {
        public string activeConversation;
        public int currentSlide;
        public List<int> availableSlides = new List<int>();
    }
    public class Policy
    {
        private bool authorProperty;
        public bool isSynced;
        public bool isAuthor
        {
            get
            {
                return authorProperty;
            }
            set
            {
                authorProperty = value;
            }
        }
        //plus any other important state information
    }
    public class UserInformation
    {
        public Credentials credentials;
        public Location location;
        public Policy policy;
    }
    public class BoardMove
    {
        public string boardUsername;
        public int roomJid;
    }
}