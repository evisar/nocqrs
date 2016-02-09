using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample.domain
{
    public class SaleTransfer: IFeature<Sale>
    {
        readonly Sale _sale;
        public SaleTransfer(Sale sale)
        {
            _sale = sale;
        }
        public void TransferTo(Location targetLocation)
        {
            _sale.Location = targetLocation;
            //System.Console.WriteLine("Sale {0} has been transfered to location {1}", _sale.GetHashCode(), targetLocation.GetHashCode());
        }   
    }
}
