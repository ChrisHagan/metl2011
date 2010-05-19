using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security;
using System.DirectoryServices;

namespace PowerpointJabber
{
    /// <summary>
    /// Interaction logic for loginDialogBox.xaml
    /// </summary>
    public partial class loginDialogBox : Window
    {
        public bool SuccessfulCompletionOfDialog = false;

        public loginDialogBox()
        {
            InitializeComponent();
        }
        private void login()
        {
            var username = usernameBox.Text;
            var password = passwordPasswordBox.Password;
            var securePassword = passwordPasswordBox.SecurePassword;
            if (authenticateAgainstFailoverSystem(username, password, securePassword))
            {
                var authorizedGroups = new AuthorisationProvider().getEligibleGroups(username, password);
                ThisAddIn.instance.wire.myCredentials = new Credentials
                {
                    name = username,
                    password = password,
                    authorizedGroups = authorizedGroups
                };
                this.SuccessfulCompletionOfDialog = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("login failed.  Please check that you have entered your details correctly");
            }
        }
        private bool authenticateAgainstFailoverSystem(string username, string password, SecureString securePassword)
        {
            if (isAuthenticatedAgainstLDAP(username, password))
                return true;
            else if (isAuthenticatedAgainstWebProxy(username, securePassword))
                return true;
            else
                return false;
        }
        public bool isAuthenticatedAgainstLDAP(string username, string password)
        {
            string LDAPServerURL = @"LDAP://directory.monash.edu.au:389/";
            string LDAPBaseOU = "o=Monash University,c=AU";
            try
            {
                DirectoryEntry LDAPAuthEntry = new DirectoryEntry(LDAPServerURL + LDAPBaseOU, "", "", AuthenticationTypes.Anonymous);
                DirectorySearcher LDAPDirectorySearch = new DirectorySearcher(LDAPAuthEntry);
                LDAPDirectorySearch.ClientTimeout = new System.TimeSpan(0, 0, 5);
                LDAPDirectorySearch.Filter = "uid=" + username;
                SearchResult LDAPSearchResponse = LDAPDirectorySearch.FindOne();

                string NewSearchPath = LDAPSearchResponse.Path.ToString();
                string NewUserName = NewSearchPath.Substring(NewSearchPath.LastIndexOf("/") + 1);

                DirectoryEntry AuthedLDAPAuthEntry = new DirectoryEntry(NewSearchPath, NewUserName, password, AuthenticationTypes.None);
                DirectorySearcher AuthedLDAPDirectorySearch = new DirectorySearcher(AuthedLDAPAuthEntry);
                AuthedLDAPDirectorySearch.ClientTimeout = new System.TimeSpan(0, 0, 5);
                AuthedLDAPDirectorySearch.Filter = "";
                SearchResultCollection AuthedLDAPSearchResponse = AuthedLDAPDirectorySearch.FindAll();
            }
            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(string.Format("Failed authentication against LDAP because {0}", e.Message));
                return false;
            }
            return true;
        }
        public bool isAuthenticatedAgainstWebProxy(string username, SecureString password)
        {
            try
            {
                IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(password);
                string sDecrypString = System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
                var resource = String.Format("https://my.monash.edu.au/login?username={0}&password={1}", username, sDecrypString);
                String test = HttpResourceProvider.insecureGetString(resource);
                return !test.Contains("error-text");
            }
            catch (Exception e)
            {
                //MessageBox.Show("Web proxy auth error:" + e.Message);
                return false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(usernameBox.Text) || String.IsNullOrEmpty(passwordPasswordBox.Password))
                return;
            login();
        }
    }
}
