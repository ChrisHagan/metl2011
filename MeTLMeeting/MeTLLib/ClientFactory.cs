using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace MeTLLib
{
    public class ClientFactory
    {
        public static ClientConnection Connection() {
            var kernel = new StandardKernel(new BaseModule(), new ProductionModule());
            return kernel.Get<ClientConnection>();
        } 
    }
}
