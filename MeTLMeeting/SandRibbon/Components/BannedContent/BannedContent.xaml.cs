using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Utilities;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using MeTLLib.Providers;
using System.Text;
using System.Windows.Media;
using System.Linq;

namespace SandRibbon.Components.BannedContent
{
    public class PrivateUser
    {
        public PrivateUser(string username, string displayname, Color displaycolor)
        {
            UserName = username;
            DisplayName = displayname;
            DisplayColor = displaycolor;
        }

        public string UserName { get; private set; }
        public string DisplayName { get; set; }
        public Color DisplayColor { get; private set; }
    }

    public class PrivacyWrapper 
    {
        private TargettedSubmission targettedSubmission;
        private List<PrivateUser> privateusers = new List<PrivateUser>();

        public PrivacyWrapper(TargettedSubmission sub, Dictionary<string, string> userMapping)
        {
            targettedSubmission = sub;

            var labelIndex = 1;
            foreach (var user in sub.blacklisted)
            {
                privateusers.Add(new PrivateUser(user.UserName, GenerateDisplayName(userMapping, user.UserName, ref labelIndex), MeTLStanzas.Ink.stringToColor(user.Color)));
            }
        }

        private string GenerateDisplayName(Dictionary<string, string> userMapping, string userName, ref int labelIndex)
        {
            return userName;
            var displayName = "";
            if (!userMapping.TryGetValue(userName, out displayName))
            {
                displayName = String.Format("User {0}", labelIndex++);
                userMapping.Add(userName, displayName);
            }
            return displayName;
        }

        public void UpdateDisplayNames(Dictionary<string,string> userMapping)
        {
            var labelIndex = 1;
            foreach (var user in PrivateUsers)
            {
                user.DisplayName = GenerateDisplayName(userMapping, user.UserName, ref labelIndex); 
            }
        }

        public List<PrivateUser> PrivateUsers
        {
            get { return privateusers; }
            set { privateusers = value; }
        }

        public string url 
        { 
            get { return targettedSubmission.url; } 
            set { targettedSubmission.url = value; }
        }

        public long time 
        {
            get { return targettedSubmission.time; }
            set { targettedSubmission.time = value; } 
        }

        public int slide
        {
            get { return targettedSubmission.slide; }
            set { targettedSubmission.slide = value; } 
        }

        public string author
        {
            get { return targettedSubmission.author; }
            set { targettedSubmission.author = value; } 
        }

        public string title
        {
            get { return targettedSubmission.title; }
            set { targettedSubmission.title = value; }
        }
    }

    public partial class BannedContent : Window
    {
        public ObservableCollection<PrivacyWrapper> submissionList { get; private set; }
        public ObservableCollection<PrivateUser> blackList { get; private set; }
        private CollectionViewSource submissionsView;
        private Dictionary<string, string> userMapping = new Dictionary<string,string>();
        public BannedContent()
        {
            InitializeComponent();
            Commands.ReceiveScreenshotSubmission.RegisterCommandToDispatcher<TargettedSubmission>(new DelegateCommand<TargettedSubmission>(ReceiveSubmission));
            Commands.JoinConversation.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(JoinConversation));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
        }

        private void closeMe(object obj)
        {
            Close();
        }
        private void JoinConversation(string jid)
        {
            Close();
        }

        private List<PrivacyWrapper> WrapSubmissions(List<TargettedSubmission> submissions)
        {
            var privateSubmissions = new List<PrivacyWrapper>();

            foreach (var sub in submissions)
            {
                var privSub = new PrivacyWrapper(sub, userMapping);
                privateSubmissions.Add(privSub);
            }

            return privateSubmissions;
        }

        private List<PrivateUser> WrapBlackList(List<string> blacklist)
        {
            var privateUsers = new List<PrivateUser>();
            var alphabetSeq = new EnglishAlphabetSequence();

            var labelIndex = 0;
            foreach (var user in blacklist)
            {
                var displayName = "";
                if (!userMapping.TryGetValue(user, out displayName))
                {
                    displayName = user;
                }
                var privUser = new PrivateUser(user, displayName, Colors.Black);
                privateUsers.Add(privUser);
            }
            return privateUsers;
        }

        private void submissions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                var currentSelection = e.AddedItems[0] as PrivacyWrapper;
                UpdateDisplayNames(currentSelection);
            }
        }

        public BannedContent(List<TargettedSubmission> userSubmissions) : this()
        {
            submissionsView = FindResource("sortedSubmissionsView") as CollectionViewSource;
            submissionList = new ObservableCollection<PrivacyWrapper>(WrapSubmissions(userSubmissions));
            blackList = new ObservableCollection<PrivateUser>(WrapBlackList(Globals.conversationDetails.blacklist));

            DataContext = this;
        }
        
        private void ReceiveSubmission(TargettedSubmission submission)
        {
            if (submission.target != "bannedcontent")
                return;

            submissionList.Add(new PrivacyWrapper(submission, userMapping));
        }

        private List<CheckBox> GetBannedUserCheckboxes()
        {
            var checkBoxes = new List<CheckBox>();
            foreach (var item in bannedUsernames.Items)
            {
                var listBoxItem = bannedUsernames.ItemContainerGenerator.ContainerFromItem(item);
                var contentPresenter = UIHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                var checkBox = contentPresenter.ContentTemplate.FindName("participantDisplay", contentPresenter) as CheckBox;
                checkBoxes.Add(checkBox);
            }
            return checkBoxes;
        }

        private void UpdateDisplayNames(PrivacyWrapper sub)
        {
            userMapping.Clear();
            sub.UpdateDisplayNames(userMapping);

            blackList.Clear();
            var updatedBlacklist = WrapBlackList(Globals.conversationDetails.blacklist);
            foreach (var user in updatedBlacklist)
            {
                blackList.Add(user);
            }
        }
        private class MergedUserInformation{
            public string user;
            public string email;
            public string display;
            public Color colorName;
            public MergedUserInformation(string username, string emailAddress, string displayName, Color color){
                user = username;
                email = emailAddress;
                display = displayName;
                colorName = color;
            }
        }
        /*
        private string BuildReport(List<MeTLUserInformation> users)
        {
            var currentSelection = submissions.SelectedItem as PrivacyWrapper;

            var theUsers = users.Join(currentSelection.PrivateUsers, u => u.username, cs => cs.UserName, (user, privateUser) => new MergedUserInformation(privateUser.UserName,user.emailAddress,privateUser.DisplayName,privateUser.DisplayColor));

            var report = new StringBuilder();
            
            var time = new DateTime(currentSelection.time);
            report.AppendFormat("\nDetails: \nConversation Owner: {0}\nConversation Title: {1}\nSlide Number: {2}\nTime Snapshot Taken: {3} {4}\n\n", 
                currentSelection.author,
                currentSelection.title,
                currentSelection.slide,
                time.ToShortDateString(), time.ToLongTimeString());

            report.Append("The following banned users and corresponding usernames were recorded:\n\n");

            foreach (var user in theUsers)
            {
                report.AppendFormat("{0} => Authcate {1} highlighted in {2}\n", user.email, user.user, ColorLookup.HumanReadableNameOf(user.colorName));
            }

            return report.ToString();
        }
        */

        private void SendEmail(string fileName, string report)
        {
            try
            {
                var emailAddress = new MailAddress(Globals.credentials.mail);
                const string subject = "MeTL Banned Content Report";
                string body = report;

                var smtpClient = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailAddress.Address, Globals.credentials.password)
                };

                using (var message = new MailMessage(emailAddress, emailAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                {
                    var attached = new Attachment(fileName, MediaTypeNames.Application.Octet);
                    message.Attachments.Add(attached);
                    smtpClient.Send(message);
                    MeTLMessage.Information("Banned content report sent to " + emailAddress.Address);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                } 
            }
        }

        private string SaveImageTemporarilyToFile()
        {
            var fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(previewImage.Source as BitmapSource));
                encoder.Save(fileStream);
                fileStream.Close();
            }

            return fileName;
        }

        private void selectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var bannedUser in GetBannedUserCheckboxes())
            {
                bannedUser.IsChecked = true;
            }
        }

        private void deselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var bannedUser in GetBannedUserCheckboxes())
            {
                bannedUser.IsChecked = false;
            }
        }

        private void unbanSelected_Click(object sender, RoutedEventArgs e)
        {
            var bannedUsers = GetBannedUserCheckboxes();
            var details = Globals.conversationDetails;
            foreach (var participant in bannedUsers)
            {
                var priv = participant.DataContext as PrivateUser;
                if (participant.IsChecked ?? false)
                {
                    details.blacklist.Remove(priv.UserName);
                    foreach (var usr in blackList)
                    {
                        if (usr.UserName == priv.UserName)
                        {
                            blackList.Remove(usr);
                            break;
                        }
                    }
                }
            }
            ClientFactory.Connection().UpdateConversationDetails(details);
        }
    }
}
