using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mercury.business;
using mercury.controller;
using mercury.model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;
namespace mercury
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Console.Clear();
            dbc_mercury.init();
            var host = new WebHostBuilder()
                .UseStartup<startup>()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureKestrel((context, options) =>
                {
                    options.Listen(IPAddress.Any, int.Parse(_io._config_value("port")), listenOptions =>
                   {
                       listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                   });
                }).Build();
            host.Run();
        }
    }
}
