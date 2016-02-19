using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Services
{
    public interface IEventSerializer
    {
        void Serialize<TEvent>(TEvent @event)
            where TEvent : IEvent;
        
        event Action<object> OnMessage;
    }
}
