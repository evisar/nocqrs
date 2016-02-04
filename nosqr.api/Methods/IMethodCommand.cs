using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Methods
{
    public interface IMethodCommand<TObject> : ICommand
    {
        //object[] Args { get; }
    }
}
