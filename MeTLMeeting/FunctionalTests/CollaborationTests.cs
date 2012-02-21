using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using Functional;
using UITestFramework;
using FunctionalTests.DSL;
using Microsoft.Test.Input;
using FunctionalTests.Utilities;
using System.Xml.Linq;
using System.IO;

namespace FunctionalTests
{
    [TestClass]
    public class CollaborationTests
    {
        private TestContext testContext;
        private static AutomationElement ownerWindow;
        private static AutomationElement participantWindow;
        private static AutomationElementCollection metlWindows;
        private static int participantRandomPage;


        public TestContext TestContext 
        {
            get
            {
                return testContext;
            }
            set
            {
                testContext = value;
            }
        }

        private void FindCurrentWindows()
        {
            metlWindows = MeTL.GetAllMainWindows(2, true);

            ownerWindow = null;
            participantWindow = null;
        }

        private AutomationElement DetermineCurrentWindow()
        {
            if (ownerWindow == null)
            {
                ownerWindow = metlWindows[0];
                return ownerWindow;
            }

            if (participantWindow == null)
            {
                participantWindow = metlWindows[1];
                return participantWindow;
            }

            return null;
        }

        private string DetermineDataDirectory()
        {
            var baseDirectory = "MeTLMeeting";
            var currentDirectory = Directory.GetCurrentDirectory();
            return currentDirectory.Remove(currentDirectory.IndexOf(baseDirectory) + baseDirectory.Length) + @"\FunctionalTests\bin\Debug";
        }

        [TestMethod]
        public void LoginOwnerAndParticipant()
        {
            FindCurrentWindows();

            var credentials = XDocument.Load(DetermineDataDirectory() + @"\MultipleUserCredentials.xml");
            foreach (var elem in credentials.Descendants("user"))
            {
                AutomationElement currentWindow = DetermineCurrentWindow();

                var user = elem.Attribute("username").Value;
                var pass = elem.Attribute("password").Value;

                var loginScreen = new Login(currentWindow).username(user).password(pass);
                loginScreen.submit();
            }
        }

        [TestMethod]
        public void JoinConversation()
        {
            var conversation = new FunctionalTests.Actions.SearchConversation();

            conversation.SearchForConversationAndJoin(new UITestHelper(UITestHelper.RootElement, ownerWindow), TestConstants.OWNER_CONVERSATION_TITLE);
            conversation.SearchForConversationAndJoin(new UITestHelper(UITestHelper.RootElement, participantWindow), TestConstants.OWNER_CONVERSATION_TITLE);
        }

        [TestMethod]
        public void OwnerAddThreePages()
        {
            ScreenActionBuilder.Create().WithWindow(ownerWindow)
                .Ensure<SlideNavigation>(nav =>
                {
                    nav.IsAddAvailable.ShouldBeTrue();
                    return nav.IsAddAvailable;
                })
                .With<SlideNavigation>( nav =>
                {
                    foreach (var i in Enumerable.Range(0, 3))
                    {
                        nav.Add();
                    }
                });
        }

        [TestMethod]
        public void ParticipantToggleSyncToOwner()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<SlideNavigation>(nav =>
                {
                    nav.IsSyncAvailable.ShouldBeTrue();
                    return nav.IsSyncAvailable;
                })
                .With<SlideNavigation>( nav =>
                {
                    nav.Sync();
                });
        }

        [TestMethod]
        public void OwnerChangePage()
        {
            ScreenActionBuilder.Create().WithWindow(ownerWindow)
                .Ensure<SlideNavigation>(nav => { return true; })
                .With<SlideNavigation>(nav =>
                {
                    var randPage = new Random();
                    var pagesToChoose = new List<int>();
                    foreach (var i in Enumerable.Range(0, nav.PagesCount))
                    {
                        if (i != nav.CurrentPage)
                            pagesToChoose.Add(i);
                    }
                    participantRandomPage = pagesToChoose[randPage.Next(pagesToChoose.Count - 1)];
                    nav.ChangePage(participantRandomPage);

                    UITestHelper.Wait(TimeSpan.FromSeconds(2));
                });
        }

        [TestMethod]
        public void ParticipantHasSyncedToOwnersPage()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<SlideNavigation>(nav => { return true; })
                .With<SlideNavigation>(nav =>
                {
                    var waitSuccess = false;
                    if (nav.CurrentPage != participantRandomPage)
                        waitSuccess = nav.WaitForPageChange(participantRandomPage);

                    UITestHelper.Wait(TimeSpan.FromSeconds(5));

                    nav.CurrentPage.ShouldEqual(participantRandomPage);
                });
        }

        [TestMethod]
        public void ParticipantHasNotSyncedToOwnersPage()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<SlideNavigation>(nav => { return true; })
                .With<SlideNavigation>(nav =>
                {
                    var waitSuccess = false;
                    if (nav.CurrentPage != participantRandomPage)
                        waitSuccess = nav.WaitForPageChange(participantRandomPage);

                    UITestHelper.Wait(TimeSpan.FromSeconds(5));

                    nav.CurrentPage.ShouldNotEqual(participantRandomPage);
                });
        }

        [TestMethod]
        public void AddTextToOwner()
        {
            ScreenActionBuilder.Create().WithWindow(ownerWindow)
                .Ensure<HomeTabScreen>( home => 
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextInsertMode();

                    return true; 
                })
                .With<CollapsedCanvasStack>( canvas =>
                {
                    var textboxCount = canvas.ChildTextboxes.Count;

                    canvas.InsertTextbox(canvas.RandomPointWithinMargin(-40, -40), "owner");

                    canvas.ChildTextboxes.Count.ShouldEqual(textboxCount + 1);
                });
        }

        [TestMethod]
        public void DeleteTextFromOwner()
        {
            ScreenActionBuilder.Create().WithWindow(ownerWindow)
                .Ensure<HomeTabScreen>( home => 
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextSelectMode();

                    return true; 
                })
                .With<CollapsedCanvasStack>( canvas =>
                {
                    var textboxCount = canvas.ChildTextboxes.Count;

                    foreach (AutomationElement textbox in canvas.ChildTextboxes)
                    {
                        canvas.SelectTextboxWithClick(textbox);
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                        canvas.DeleteSelectedContent();
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    }

                    UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    canvas.ChildTextboxes.Count.ShouldEqual(0);
                });
        }

        [TestMethod]
        public void AddTextToParticipant()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<HomeTabScreen>(home =>
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextInsertMode();
                    return true;
                })
                .With<CollapsedCanvasStack>(canvas =>
                {
                    var textboxCount = canvas.ChildTextboxes.Count;

                    canvas.InsertTextbox(canvas.RandomPointWithinMargin(-40, -40), "participant");

                    canvas.ChildTextboxes.Count.ShouldEqual(textboxCount + 1);
                });
        }

        [TestMethod]
        public void DeleteTextFromParticipant()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<HomeTabScreen>( home => 
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextSelectMode();

                    return true; 
                })
                .With<CollapsedCanvasStack>( canvas =>
                {
                    var textboxCount = canvas.ChildTextboxes.Count;

                    foreach (AutomationElement textbox in canvas.ChildTextboxes)
                    {
                        canvas.SelectTextboxWithClick(textbox);
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                        canvas.DeleteSelectedContent();
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    }

                    UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    canvas.ChildTextboxes.Count.ShouldEqual(0);
                });
        }
        [TestMethod]
        public void AddInkToOwner()
        {
            ScreenActionBuilder.Create().WithWindow(ownerWindow)
                .Ensure<HomeTabScreen>( home => 
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivatePenMode().SelectPen(0);

                    return true; 
                })
                .With<CollapsedCanvasStack>( canvas =>
                {
                    var inkStrokeCount = canvas.NumberOfInkStrokes();

                    Mouse.Down(MouseButton.Left);
                    // points need to be below 10 if we're to get a single stroke
                    MouseExtensions.AnimateThroughPoints(canvas.RandomPoints(8, -40, -40));
                    Mouse.Up(MouseButton.Left);

                    canvas.NumberOfInkStrokes().ShouldEqual(inkStrokeCount + 1);
                });
        }


        [TestMethod]
        public void DeleteInkFromOwner()
        {
            ScreenActionBuilder.Create().WithWindow(ownerWindow)
                .Ensure<HomeTabScreen>( home => 
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivatePenMode().PenSelectMode();

                    return true; 
                })
                .With<CollapsedCanvasStack>( canvas =>
                {
                    canvas.SelectAllInkStrokes();

                    UITestHelper.Wait(TimeSpan.FromMilliseconds(500));

                    canvas.DeleteSelectedContent();

                    UITestHelper.Wait(TimeSpan.FromSeconds(5));

                    canvas.NumberOfInkStrokes().ShouldEqual(0);
                });
        }

        [TestMethod]
        public void DeleteInkFromParticipant()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<HomeTabScreen>( home => 
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivatePenMode().PenSelectMode();

                    return true; 
                })
                .With<CollapsedCanvasStack>( canvas =>
                {
                    canvas.SelectAllInkStrokes();

                    UITestHelper.Wait(TimeSpan.FromMilliseconds(500));

                    canvas.DeleteSelectedContent();

                    UITestHelper.Wait(TimeSpan.FromSeconds(5));

                    canvas.NumberOfInkStrokes().ShouldEqual(0);
                });
        }

        [TestMethod]
        public void AddInkToParticipant()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<HomeTabScreen>( home => 
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivatePenMode().SelectPen(2);

                    return true; 
                })
                .With<CollapsedCanvasStack>( canvas =>
                {
                    var inkStrokeCount = canvas.NumberOfInkStrokes();

                    Mouse.Down(MouseButton.Left);
                    // points need to be below 10 if we're to get a single stroke
                    MouseExtensions.AnimateThroughPoints(canvas.RandomPoints(8, -40, -40));
                    Mouse.Up(MouseButton.Left);

                    canvas.NumberOfInkStrokes().ShouldEqual(inkStrokeCount + 1);
                });
        }

        [TestMethod]
        public void AddEpicycloidInkToPartipant()
        {
            ScreenActionBuilder.Create().WithWindow(participantWindow)
                .Ensure<HomeTabScreen>(home =>
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivatePenMode();

                    return true;
                })
                .With<CollapsedCanvasStack>(canvas =>
                {
                    canvas.DrawSpirographWaveOnCanvas();
                });
        }
    }
}
