using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using MeTLLib.Providers.Connection;
using MeTLLib;
using System.DirectoryServices;
using MeTLLib.DataTypes;
using System.Diagnostics;
//using Ninject;
using System.Text;
using System.Xml.Linq;
using System.Threading;

namespace MeTLLib.Providers
{
    public class AuthorisationProvider : HttpResourceProvider
    {
        private MetlConfiguration server;
        private IWebClientFactory webclientFactory;
        public AuthorisationProvider(IWebClientFactory factory, MetlConfiguration server, IAuditor auditor)
            : base(factory,auditor)
        {
            this.webclientFactory = factory;
            this.server = server;
        }
        public Credentials attemptAuthentication(string username, string password)
        {
            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                string AuthcateUsername = username;
                string AuthcatePassword = password;
                var token = login(AuthcateUsername, AuthcatePassword);
                if (token.authenticated)
                {
                    var eligibleGroups = token.groups;
                    var credentials = new Credentials(AuthcateUsername, AuthcatePassword, eligibleGroups, token.mail);
                    //Globals.credentials = credentials;
                    return credentials;
                }
            }

            _auditor.log("attemptAuthentication - failed to login","AuthorizationProvider");
            return Credentials.Empty;
        }
        public class AuthToken
        {
            public AuthToken(string Username)
            {
                username = Username;
            }
            public string username { get; private set; }
            public List<AuthorizedGroup> groups = new List<AuthorizedGroup>();
            public bool authenticated = false;
            public List<String> errors = new List<String>();
            public string mail { get; set; }
        }
        public AuthToken login(string AuthcateName, string AuthcatePassword)
        {
            var token = new AuthToken(AuthcateName);
            token.authenticated = false;
            return token;
        }
    }
}