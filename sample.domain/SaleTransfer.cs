using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample.domain
{
    public class SaleTransfer: IFeature<Sale>
    {
        public static int TimesCalled = 0;

        readonly Sale _sale;
        public SaleTransfer(Sale sale)
        {
            _sale = sale;
        }
        public void TransferTo(Location targetLocation)
        {
            _sale.Location = targetLocation;
            TimesCalled++;
        }
    }
}
