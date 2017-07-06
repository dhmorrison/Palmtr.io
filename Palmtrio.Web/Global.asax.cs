using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;

using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;
using SimpleInjector.Integration.WebApi;

namespace Palmtrio.Web
{
    public class Global : HttpApplication
    {
        private Container _diContainer;

        void Application_Start(object sender, EventArgs e)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;

            MvcHandler.DisableMvcResponseHeader = true;

            _diContainer = new Container();
            Framework.Injectors.SimpleInjector.Configure(ref _diContainer);
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(_diContainer));
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(_diContainer);

            
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //GlobalFilters.Filters.Add(new Palmtrio.Web.Framework.Filters.RequireHttpsAttribute());

        }


        public override void Dispose()
        {
            if (_diContainer != null)
                _diContainer.Dispose();

            base.Dispose();
        }

    }
}