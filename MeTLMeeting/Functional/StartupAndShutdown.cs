using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Windows.Automation;
using System.Threading;

namespace Functional
{
    [TestClass]
    public class StartupAndShutdown
    {
        private static Process metlProcess;

        private AutomationElement metlWindow;
        private const string workingDirectory = @"C:\specialMeTL\MeTLMeeting\SandRibbon\bin\Debug\";

        [ClassInitialize]
        public static void StartProcess(TestContext context)
        {
            metlProcess = new Process();
            metlProcess.StartInfo.UseShellExecute = false;
            metlProcess.StartInfo.WorkingDirectory = workingDirectory; 
            metlProcess.StartInfo.FileName = workingDirectory + "MeTL Presenter.exe";
            metlProcess.Start();
        }

        [ClassCleanup]
        public static void EndProcess()
        {
            if (!metlProcess.HasExited)
            {
                metlProcess.CloseMainWindow();
                metlProcess.Close();
            }
        }
        
        [TestInitialize]
        public void Setup()
        {
            var control = new UITestControl();
            control.WaitForControlEnabled(Constants.ID_METL_MAIN_WINDOW);

            if (metlWindow == null)
                metlWindow = MeTL.GetMainWindow(); 
        }

        [TestMethod]
        public void Quit()
        {
            new ApplicationPopup(metlWindow).Quit();
        }

        [TestMethod]
        public void LogoutAndQuit()
        {
            new ApplicationPopup(metlWindow).LogoutAndQuit();
        }
    }
}
