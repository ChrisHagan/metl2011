using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Functional;
using UITestFramework;

namespace FunctionalTests.Utilities
{
    public interface IScreenAction
    {
        IScreenAction With<T>(Action<T> action) where T : IScreenObject, new();
    }

    public class ScreenAction : IScreenAction
    {
        private UITestHelper screenParent;

        public ScreenAction(UITestHelper parent)
        {
            screenParent = parent;
        }

        public IScreenAction With<T>(Action<T> action) where T : IScreenObject, new()
        {
            var screenObject = new T();
            screenObject.Parent = screenParent;

            action(screenObject);

            return this;
        }
    }

    public class NullScreenAction : IScreenAction
    {
        public IScreenAction With<T>(Action<T> action) where T : IScreenObject, new()
        {
            return this; 
        }
    }
}
