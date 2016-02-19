using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Services.Channels
{
    public class NullEventSerializer: IEventSerializer
    {
        public void Serialize<TEvent>(TEvent @event) where TEvent : IEvent
        {
            
        }

        public event Action<object> OnMessage;
    }
}
