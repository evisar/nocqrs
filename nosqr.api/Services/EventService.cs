using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Services
{
    public class EventService : IEventService
    {
        public class EventSubscription : IDisposable
        {
            readonly Action _handle;
            public EventSubscription(Action handle)
            {
                _handle = handle;
            }
            public void Dispose()
            {
                if (_handle != null)
                    _handle();
            }
        }

        List<Delegate> _subscribers = new List<Delegate>();
        public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            foreach (var action in _subscribers)
            {
                if (action.GetType().GetGenericArguments().FirstOrDefault() == typeof(TEvent))
                {
                    action.DynamicInvoke(@event);
                }
            }
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            _subscribers.Add(action);
            return new EventSubscription(() => _subscribers.Remove(action));
        }
    }
}
