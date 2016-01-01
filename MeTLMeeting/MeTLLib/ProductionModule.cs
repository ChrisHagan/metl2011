using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Ninject.Modules;
using MeTLLib.Providers.Connection;
using System.Net;
using MeTLLib.Providers;
using System.Xml.Linq;

namespace MeTLLib
{
    public class TriedToStartMeTLWithNoInternetException : Exception
    {
        public TriedToStartMeTLWithNoInternetException(Exception inner) : base("Couldn't connect to MeTL servers", inner) { }
    }
}
