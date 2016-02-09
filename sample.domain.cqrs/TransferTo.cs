using nosqr.api;
using nosqr.api.Aspects;
using sample.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample.domain.cqrs
{
    [Aspect(typeof(LoggingAspect<>))]
    [Aspect(typeof(TransactionAspect<>))]
    public class TransferTo : ICommand<SaleTransfer, Sale>
    {
        public Sale Model { get; set; }
        public Location Location { get; set; }

        public object[] Arguments
        {
            get { return new []{ this.Location}; }
        }
    }
}
