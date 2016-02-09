using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api
{
    public interface IAspect<T>
        where T: ICommand
    {
        Action<Action<T>, T> GetAspect(); 
    }
}
