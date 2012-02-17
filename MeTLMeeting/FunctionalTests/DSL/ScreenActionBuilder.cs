using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Functional;

namespace FunctionalTests.DSL
{
    public class ScreenActionBuilder 
    {
        private ScreenActionBuilder()
        {
        }

        public static IWindowScreenObject Create()
        {
            return new WindowScreen(); 
        }
    }
}
