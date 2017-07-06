using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using System.IO;
using Palmtrio.Web.Framework.Session;
using Palmtrio.Domain.Interfaces;
using  Palmtrio.Instrument.Services;

namespace Palmtrio.Web.Framework.Injectors
{
    public static class SimpleInjector
    {
        private static readonly string _baseDir;
        private static readonly string _binDir;

        static SimpleInjector()
        {
            _baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _binDir = Path.Combine(_baseDir, "bin");
        }

        public static void Configure(ref Container DiContainer)
        {

            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(Assembly.LoadFrom(Path.Combine(_binDir, "Palmtrio.Instrument.dll")));

            var registrations = from type in assemblies.SelectMany(asm => asm.GetExportedTypes())
                                where type.GetInterfaces().Any() && !type.Name.EndsWith("Singleton")
                                select new { Service = type.GetInterfaces().Single(), Implementation = type };

            foreach (var reg in registrations)
            {
                DiContainer.Register(reg.Service, reg.Implementation, Lifestyle.Transient);
            }


            DiContainer.Register<IHttpSessionStateWrapped, DefaultSessionState>(Lifestyle.Transient);

            DiContainer.Register<IApiResourceManager, ApiResourceManagerSingleton>(Lifestyle.Singleton);
            DiContainer.Register<IPageHitCounter, DomainPageHitCounterSingleton>(Lifestyle.Singleton);

            DiContainer.RegisterInitializer<DomainPageHitCounterSingleton>(instance => instance.InjLogPath = "C:\\EXTERNAL\\LOGS\\");


            DiContainer.Verify();
        }

    }
}