using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Methods
{
    public abstract class MethodCommand<TObject> : IMethodCommand<TObject>
    {
        public object[] Args { get; private set; }
        public MethodCommand(params object[] args)
        {
            this.Args = args;
        }
    }
}
