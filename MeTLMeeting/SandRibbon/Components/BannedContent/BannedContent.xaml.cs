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

namespace SandRibbon.Components.BannedContent
{
    public partial class BannedContent : Window
    {
        public ObservableCollection<TargettedSubmission> submissionList { get; private set; }
        private CollectionViewSource submissionsView;
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
        public BannedContent(List<TargettedSubmission> userSubmissions):this()
        {
            submissionsView = FindResource("sortedSubmissionsView") as CollectionViewSource;
            submissionList = new ObservableCollection<TargettedSubmission>(userSubmissions);

            DataContext = this;
        }
        
        private void ReceiveSubmission(TargettedSubmission submission)
        {
            if (submission.target != "bannedcontent")
                return;

            submissionList.Add(submission);
        }

        private List<CheckBox> GetBannedUserCheckboxes()
        {
            var checkBoxes = new List<CheckBox>();
            foreach (var item in bannedUsernames.Items)
            {
                var listBoxItem = bannedUsernames.ItemContainerGenerator.ContainerFromItem(item);
                var contentPresenter = UIHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                var checkBox = contentPresenter.ContentTemplate.FindName("participantCheckbox", contentPresenter) as CheckBox;
                checkBoxes.Add(checkBox);
            }
            return checkBoxes;
        }

        private void emailReport_Click(object sender, RoutedEventArgs e)
        {
            emailReport.IsEnabled = false;
            var fileName = SaveImageTemporarilyToFile();
            ThreadPool.QueueUserWorkItem((state) =>
                {
                    SendEmail(fileName);
                    Dispatcher.adopt(() => emailReport.IsEnabled = true);
                });
        }

        private void SendEmail(string fileName)
        {
            try
            {
                var emailAddress = new MailAddress(Globals.credentials.mail);
                const string subject = "MeTL Banned Content Report";
                const string body = "Banned Content Report";

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
                var username = participant.Content as string;
                if (participant.IsChecked ?? false)
                    details.blacklist.Remove(username);
            }
            ClientFactory.Connection().UpdateConversationDetails(details);
        }
    }
}
