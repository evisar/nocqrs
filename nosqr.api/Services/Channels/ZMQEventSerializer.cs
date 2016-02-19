using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Services.Channels
{
    public class ZMQEventSerializer: IEventSerializer
    {
        public void Serialize<TEvent>(TEvent @event) where TEvent : IEvent
        {
            throw new NotImplementedException();
        }

        public event Action<object> OnMessage;
    }
}
