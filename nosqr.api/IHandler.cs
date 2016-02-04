using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api
{
    public interface IHandler<TCommand>
        where TCommand: IEvent
    {
        void Handle(TCommand command);
    }
}
