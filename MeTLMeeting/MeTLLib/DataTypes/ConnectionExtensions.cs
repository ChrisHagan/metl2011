using System.Collections.Generic;
namespace MeTLLib.DataTypes
{
    public class Credentials
    {
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is Credentials)) return false;
            var foreignCredentials = ((Credentials)obj);
            return ((foreignCredentials.name == name)
                && (foreignCredentials.password == password)
                && (foreignCredentials.authorizedGroups.TrueForAll(s => s.ValueEquals(authorizedGroups[foreignCredentials.authorizedGroups.IndexOf(s)]))));
        }
        public string name;
        public string password;
        public List<AuthorizedGroup> authorizedGroups;
    }
    public class AuthorizedGroup
    {
        public AuthorizedGroup() {}
        //What's this method here for?  Seriously?
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
            if (obj == null || !(obj is AuthorizedGroup)) return false;
            var otherGroup = (AuthorizedGroup)obj;
            return otherGroup.groupKey == this.groupKey;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is AuthorizedGroup)) return false;
            var otherGroup = (AuthorizedGroup)obj;
            return (otherGroup.groupKey == this.groupKey && otherGroup.groupType == this.groupType);
        }
        public override int GetHashCode()
        {
            return groupKey.GetHashCode();
        }
    }
    public class Location
    {
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is Location)) return false;
            var foreignLocation = ((Location)obj);
            return ((foreignLocation.activeConversation == activeConversation)
                &&(foreignLocation.currentSlide == currentSlide)
                &&(foreignLocation.availableSlides.TrueForAll(s=>s.Equals(availableSlides[foreignLocation.availableSlides.IndexOf(s)]))));
        }
        public string activeConversation;
        public int currentSlide;
        public List<int> availableSlides = new List<int>();
    }
    public class Policy
    {
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is Policy)) return false;
            return ((((Policy)obj).isAuthor == isAuthor) && (((Policy)obj).isSynced == isSynced));
        }
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
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is UserInformation)) return false;
            var foreignUserInformation = ((UserInformation)obj);
            return ((foreignUserInformation.credentials.ValueEquals(credentials))
                && (foreignUserInformation.location.ValueEquals(location))
                && (foreignUserInformation.policy.ValueEquals(policy)));
        }
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