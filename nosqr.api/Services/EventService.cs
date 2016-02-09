using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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

        IDictionary<Type, Delegate> _subscribers = new SortedDictionary<Type, Delegate>();
        public virtual void Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var type = typeof(TEvent);
            if(_subscribers.ContainsKey(type))
            {
                ((Action<TEvent>)_subscribers[type])(@event);
            }
        }

        public virtual IDisposable Subscribe<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            var type = typeof(TEvent);
            if(!_subscribers.ContainsKey(type))
            {
                _subscribers.Add(type, action);
            }
            var @delegate = _subscribers[type] as Action<TEvent>;
            _subscribers[type] = (Action<TEvent>)Delegate.Combine(@delegate, action);                                    
            return new EventSubscription(() => _subscribers[type] = Delegate.Remove(@delegate, action));
        }
    }
}
