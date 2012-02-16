using System;
using System.Windows.Automation;
using FunctionalTests.Utilities;
using Functional;

namespace FunctionalTests
{
    public interface IWindowScreenObject
    {
        IWindowScreenObject WithWindow(AutomationElement window);
        IScreenAction Ensure<T>(Func<T, bool> func) where T : IScreenObject, new();
    }
}
