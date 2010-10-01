using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using MeTLLib.Providers.Connection;
using MeTLLib.Providers;
using MeTLLib.Providers.Structure;

namespace MeTLLib
{
    /*The properties which will remain true during both production and test*/
    public class BaseModule : NinjectModule
    {
        public override void Load()
        {
            Bind<HttpResourceProvider>().ToSelf().InSingletonScope();
            Bind<AuthorisationProvider>().ToSelf().InSingletonScope();
            Bind<ResourceUploader>().ToSelf().InSingletonScope();
            Bind<HttpHistoryProvider>().ToSelf().InSingletonScope();
            Bind<IConversationDetailsProvider>().To<FileConversationDetailsProvider>().InSingletonScope();
        }
    }
}
