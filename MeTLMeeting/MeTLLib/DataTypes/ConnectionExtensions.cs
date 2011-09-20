using System.Collections.Generic;
namespace MeTLLib.DataTypes
{
    public class Credentials
    {
        public Credentials(string Name, string Password, List<AuthorizedGroup> AuthorizedGroups)
        {
            name = Name;
            password = Password;
            authorizedGroups = AuthorizedGroups;
        }
        public bool ValueEquals(object obj)
        {
            if (obj == null || !(obj is Credentials)) return false;
            var foreignCredentials = ((Credentials)obj);
            return ((foreignCredentials.name == name)
                && (foreignCredentials.password == password)
                && (foreignCredentials.authorizedGroups.TrueForAll(s => s.ValueEquals(authorizedGroups[foreignCredentials.authorizedGroups.IndexOf(s)]))));
        }
        public bool isValid {
            get {
                return authorizedGroups != null && authorizedGroups.Count > 0;
            }
        }
        public string name;
        public string password;
        public List<AuthorizedGroup> authorizedGroups;
        public static Credentials Empty
        {
            get
            {
                return new Credentials("", "", new List<AuthorizedGroup>());
            }
        }
    }
    public class AuthorizedGroup
    {
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
        public Location(string ActiveConversation, int CurrentSlide, List<int> AvailableSlides)
        {
            activeConversation = ActiveConversation;
            currentSlide = CurrentSlide;
            availableSlides = AvailableSlides;
        }
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
        public static Location Empty {
            get { return new Location("0", 1, new List<int> { 1 }); }
        }
    }
    public class Policy
    {
        public Policy(bool IsAuthor, bool IsSynced)
        {
            isAuthor = IsAuthor;
            isSynced = IsSynced;
        }
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
        public UserInformation(Credentials Credentials, Location Location, Policy Policy)
        {
            credentials = Credentials;
            location = Location;
            policy = Policy;
        }
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