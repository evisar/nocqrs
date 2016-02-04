using Autofac;
using nosqr.api;
using nosqr.api.Services;
using sample.domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nocqrs.sample
{
    public static class Program
    {
        static readonly IEventService _bus = new EventService();

        public class TransferTo : Command<SaleTransfer, Sale>
        {
            public Location Location { get; private set; }
            public TransferTo(Sale sale, Location location):base(sale, location)
            {
                Location = location;
            }
        }

        public static IDisposable Subscribe<TEvent>(this IEventService bus)
            where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            var featureType = eventType.BaseType.GetGenericArguments().FirstOrDefault();
            var entityType = eventType.BaseType.GetGenericArguments().Skip(1).FirstOrDefault();

            //cqrs method dyanmic subscribe 
            return
                bus.Subscribe<TEvent>(x =>
                {
                    var operation = featureType.GetMethod(eventType.Name);
                    dynamic cmd = x;
                    var feature = Activator.CreateInstance(featureType, cmd.Model);
                    operation.Invoke(feature, cmd.Arguments);
                });
        }

        static Program()
        {
            //subscribe explicitly with an action
            _bus.Subscribe<TransferTo>(x =>
                {
                    var transfer = new SaleTransfer(x.Model);
                    transfer.TransferTo(x.Location);
                });


            //subscribe implicitely with reflection
            //assuming a feature is created from transfer base type
            //passing model and arguments with reflection invoke
            _bus.Subscribe<TransferTo>();
        }

        static void Main(string[] args)
        {
            var sale = new Sale();
            var locationTo = new Location();

            //direct method execute - 1 call
            var transfer = new SaleTransfer(sale);
            transfer.TransferTo(locationTo);
            
            //cqrs method publish - 2 calls     
            var command = new TransferTo(sale, locationTo);            
            _bus.Publish(command);

            //assert that transfer was called 3 times
            Debug.Assert(SaleTransfer.TimesCalled == 3);
        }
    }
}
