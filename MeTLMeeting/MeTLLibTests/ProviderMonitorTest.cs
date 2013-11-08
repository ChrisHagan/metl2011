using MeTLLib.Providers.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ninject.Modules;
using Ninject;
using MeTLLib;
using MeTLLibTests;
using System.Timers;
using System.Diagnostics;

namespace MeTLLibTests
{
    [TestClass()]
    public class ProviderMonitorTest
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            MeTLConfiguration.Load();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        [TestMethod()]
        public void ProviderMonitorConstructorTest()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            IProviderMonitor providerMonitor = kernel.Get<IProviderMonitor>();
            Assert.IsInstanceOfType(providerMonitor, typeof(ProductionProviderMonitor));
        }
        [TestMethod()]
        public void TestHealthCheckReturnsInLessThanOneSecond()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            IProviderMonitor providerMonitor = kernel.Get<IProviderMonitor>();
            bool hasPassed = false;
            providerMonitor.HealthCheck(() =>
            {
                hasPassed = true;
            });
            var TimeTaken = TestExtensions.ConditionallyDelayFor(6000, hasPassed);
            Assert.IsTrue(hasPassed && TimeTaken < new TimeSpan(0, 0, 1));
        }
        [TestMethod()]
        public void ProductionHealthCheckReturnsInLessThanThreeSeconds()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<ProductionTimerFactory>().InSingletonScope();
            IProviderMonitor providerMonitor = kernel.Get<IProviderMonitor>();
            bool hasPassed = false;
            providerMonitor.HealthCheck(() =>
            {
                hasPassed = true;
            });
            var TimeTaken = TestExtensions.ConditionallyDelayFor(6000, hasPassed);
            Assert.IsTrue(hasPassed && TimeTaken < new TimeSpan(0, 0, 1));        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestHealthCheckTestActionFailsWhenPassedNullAction()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            //Don't bind to the testProviderMonitor here - this is the class for testing the real one.
            kernel.Bind<IProviderMonitor>().To<TestProviderMonitor>().InSingletonScope();
            IProviderMonitor providerMonitor = kernel.Get<IProviderMonitor>();
            providerMonitor.HealthCheck(null);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProductionHealthCheckTestActionFailsWhenPassedNullAction()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            //Don't bind to the testProviderMonitor here - this is the class for testing the real one.
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            IProviderMonitor providerMonitor = kernel.Get<IProviderMonitor>();
            providerMonitor.HealthCheck(null);
        }
        [TestMethod()]
        [DeploymentItem("MeTLLib.dll")]
        public void checkServersTest()
        {
            IKernel kernel = new StandardKernel(new BaseModule());
            //Don't bind to the testProviderMonitor here - this is the class for testing the real one.
            kernel.Bind<IProviderMonitor>().To<TestProviderMonitor>().InSingletonScope();
            IProviderMonitor target = kernel.Get<IProviderMonitor>();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
    public class TestProviderMonitor : IProviderMonitor
    {
        public void HealthCheck(Action healthyBehaviour)
        {
            if (healthyBehaviour == null) throw new ArgumentNullException("healthyBehaviour", "Argument cannot be null");
            healthyBehaviour.Invoke();
        }
    }
    public class TestTimer : ITimer
    {
        private Action elapsedEvent;
        public TestTimer(Action elapsed)
        {
            elapsedEvent = elapsed;
        }
        public void Stop()
        {
        }
        public void Start()
        {
            if (elapsedEvent != null)
                elapsedEvent.Invoke();
        }
        public void Dispose()
        {
        }
    }
    public class TestTimerFactory : ITimerFactory
    {

        public ITimer getTimer(int time, Action elapsed)
        {
            return new TestTimer(elapsed);
        }
    }
}
