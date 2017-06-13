﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Websdepot
{
    internal static class RebootConfigService
    {
        internal static void Configure()
        {
            HostFactory.Run(configure =>
            {
                configure.Service<RebootService>(service =>
                {
                    service.ConstructUsing(s => new RebootService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem();
                configure.SetServiceName("MyWindowServiceWithTopshelf");
                configure.SetDisplayName("MyWindowServiceWithTopshelf");
                configure.SetDescription("My .Net windows service with Topshelf");
            });
        }
    }
}
