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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            kernel = new StandardKernel(new BaseModule());
            kernel.Bind<IProviderMonitor>().To<ProductionProviderMonitor>().InSingletonScope();
            kernel.Bind<ITimerFactory>().To<TestTimerFactory>().InSingletonScope();
            kernel.Bind<MeTLServerAddress>().To<ProductionServerAddress>().InSingletonScope();
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        IKernel kernel;

        [TestMethod()]
        public void ProviderMonitorConstructorTest()
        {

            IProviderMonitor providerMonitor = kernel.Get<IProviderMonitor>();
            Assert.IsInstanceOfType(providerMonitor, typeof(ProductionProviderMonitor));
        }
        [TestMethod()]
        public void TestHealthCheckReturnsInLessThanOneSecond()
        {
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
            IProviderMonitor providerMonitor = kernel.Get<IProviderMonitor>();
            bool hasPassed = false;
            providerMonitor.HealthCheck(() =>
            {
                hasPassed = true;
            });
            var TimeTaken = TestExtensions.ConditionallyDelayFor(6000, hasPassed);
            Assert.IsTrue(hasPassed && TimeTaken < new TimeSpan(0, 0, 1));
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
