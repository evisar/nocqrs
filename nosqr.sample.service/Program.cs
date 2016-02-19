using Autofac;
using Castle.Core.Logging;
using nosqr.api;
using nosqr.api.Aspects;
using nosqr.api.Services;
using nosqr.api.Services.Channels;
using sample.domain;
using sample.domain.cqrs;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace nosqr.sample.service
{
    class Program
    {

        static IEventService _bus;

        #region Bootstrap Bus, container, commands and handlers
        static Program()
        {
            //discover types
            new DirectoryCatalog(".");

            //bootstrap everything
            var builder = new ContainerBuilder();
            builder.RegisterType<FileEventSerializer>().As<IEventSerializer>();
            builder.RegisterType<EventService>().As<IEventService>();
            builder.RegisterType<ConsoleLogger>().As<ILogger>();
            builder.RegisterGeneric(typeof(LoggingAspect<>)).As(typeof(IAspect<>));
            builder.RegisterGeneric(typeof(TransactionAspect<>)).As(typeof(IAspect<>));

            //find and register all features
            var features = from feat in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                           where !feat.IsInterface && !feat.IsAbstract
                           where (from @if in feat.GetInterfaces() where @if.IsGenericType select @if.GetGenericTypeDefinition()).FirstOrDefault() == typeof(IFeature<>)
                           select feat;
            builder.RegisterTypes(features.ToArray());

            var container = builder.Build();

            _bus = container.Resolve<IEventService>();
            _bus.CanRead = true;
            _bus.CanWrite = false;

            var logger = container.Resolve<ILogger>();

            //find all commands
            var commands = from cmd in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                           where !cmd.IsInterface && !cmd.IsAbstract && typeof(ICommand).IsAssignableFrom(cmd)
                           select cmd;

            //register/subscribe all commands with dmain handlers
            foreach (var cmd in commands)
            {
                var createHandler = typeof(DomainServicesExtensions).GetMethod("CreateHandler", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(cmd);
                var handler = createHandler.Invoke(null, new[] { container });

                var subscribe = typeof(EventService).GetMethod("Subscribe").MakeGenericMethod(cmd);
                subscribe.Invoke(_bus, new[] { handler });
            }
        }
        #endregion


        static TimeSpan _elapsed;
        static int _read, _write;

        static void Main(string[] args)
        {
            var timer = new Timer(1000);
            timer.Enabled = true;
            timer.Elapsed += timer_Elapsed;

            _bus.EnableProgress = true;
            _bus.OnProgress += _bus_OnProgress;
            System.Console.ReadLine();

            
        }

        static void _bus_OnProgress(int arg1, int arg2, TimeSpan arg3)
        {
            _read = arg1;
            _write = arg2;
            _elapsed = arg3;
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Func<int, decimal> calc = x => _elapsed.TotalSeconds == 0 ? 0 :  (decimal)x / (decimal)_elapsed.TotalSeconds;
            Console.Title = string.Format("Read: {0:#,#}/s = {1:#,#}, Write: {2:#,#}/s = {3:#,#}", calc(_read), _read, calc(_write), _write);
        }
    }
}
