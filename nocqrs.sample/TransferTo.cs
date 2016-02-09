using nosqr.api;
using nosqr.api.Aspects;
using sample.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nocqrs.sample
{
    [Aspect(typeof(LoggingAspect<>))]
    [Aspect(typeof(TransactionAspect<>))]
    public class TransferTo : Command<SaleTransfer, Sale>
    {
        public Location Location { get; private set; }

        public TransferTo(Sale sale, Location location):base(sale, location)
        {
            this.Location = location;
        }
    }
}
