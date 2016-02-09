using Autofac;
using Castle.Core.Logging;
using nosqr.api;
using nosqr.api.Aspects;
using nosqr.api.Services;
using sample.domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace nocqrs.sample
{
    public class Program
    {
        static IEventService _bus;

        #region Bootstrap Bus, container, commands and handlers
        static Program()
        {
            //discover types
            new DirectoryCatalog(".");

            //bootstrap everything
            var builder = new ContainerBuilder();
            builder.RegisterType<EventService>().As<IEventService>();
            builder.RegisterType<ConsoleLogger>().As<ILogger>();
            builder.RegisterGeneric(typeof(LoggingAspect<>)).As(typeof(IAspect<>));
            builder.RegisterGeneric(typeof(TransactionAspect<>)).As(typeof(IAspect<>));
            
            //find and register all features
            var features = from feat in AppDomain.CurrentDomain.GetAssemblies().SelectMany( a=> a.GetTypes())
                          where !feat.IsInterface && !feat.IsAbstract
                          where (from @if in feat.GetInterfaces() where @if.IsGenericType select @if.GetGenericTypeDefinition()).FirstOrDefault() == typeof(IFeature<>)
                          select feat;
            builder.RegisterTypes(features.ToArray());

            var container = builder.Build();

            _bus = container.Resolve<IEventService>();
            var logger = container.Resolve<ILogger>();

            //find all commands
            var commands = from cmd in AppDomain.CurrentDomain.GetAssemblies().SelectMany( a=> a.GetTypes())
                          where !cmd.IsInterface && !cmd.IsAbstract && typeof(ICommand).IsAssignableFrom(cmd)
                          select cmd;

            //register/subscribe all commands with dmain handlers
            foreach(var cmd in commands)
            {
                var createHandler = typeof(DomainContainerExtensions).GetMethod("CreateHandler", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(cmd);
                var handler = createHandler.Invoke(null, new[] { container });

                var subscribe = typeof(EventService).GetMethod("Subscribe").MakeGenericMethod(cmd);
                subscribe.Invoke(_bus, new []{handler});
            }
        }
        #endregion
        
        /// <summary>
        /// Pub/Sub example of a CQRS command
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var sale = new Sale();
            var locationTo = new Location();

            //cqrs method publish 
            var command = new TransferTo(sale, locationTo);
            _bus.Publish(command);

            System.Console.ReadLine();
        }
    }
}
