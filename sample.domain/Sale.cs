using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sample.domain
{
    public class Sale  : IEntity
    {
        public Location Location { get; set; }
    }
}
