using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Autofac.Integration.SignalR;
using Caribbean.DataAccessLayer.Database;
using Microsoft.AspNet.SignalR;

namespace Caribbean.Aruba.Web
{
    public static class IoCConfig
    {
        public static void RegisterResolver()
        {
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly(),
                Assembly.Load("Caribbean.DataAccessLayer.Database"),
                Assembly.Load("Caribbean.DataAccessLayer.PrintTemplates"),
                Assembly.Load("Caribbean.DataAccessLayer.RealEstateObjects"),
            };

            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("Service")).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("Factory")).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("Repository")).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("Parser")).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(assemblies).Where(t => t.Name.EndsWith("Broadcaster")).AsImplementedInterfaces();
            var container = builder.Build();
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);
        }
    }
}