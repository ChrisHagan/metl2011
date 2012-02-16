using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Functional;

namespace FunctionalTests.DSL
{
    /*
                var builder = ScreenActionBuilder.Create().WithWindow(ownerWindow)
                    .Ensure<HomeTabScreen>( x => x.OpenTab(); x.InTextInsertMode(); )
                        .With<CollapsedCanvasStack>( x =>
     */

    public class ScreenActionBuilder /*: IWindowScreenObject*/
    {
        //private IWindowScreenObject windowObject;
        private ScreenActionBuilder()
        {
        }

        public static IWindowScreenObject Create()
        {
            return new WindowScreen(); 
        }
    }
}
