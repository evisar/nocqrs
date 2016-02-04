using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sample.domain
{
    public interface IFeature<in T>
        where T: IEntity
    {
    }
}
