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
        readonly IEventSerializer _serializer;
        Stopwatch _sw = null;
        int _read, _write;

        bool _canRead, _subscribed;
        public bool CanRead 
        {
            get
            {
                return _canRead;
            }
            set
            {
                if(!value && _serializer!=null && _subscribed)
                {
                    _serializer.OnMessage -= serializer_OnMessage;
                    _subscribed = false;
                }
                else if (value && _serializer != null && !_subscribed)
                {
                    _serializer.OnMessage += serializer_OnMessage;
                }                
                _canRead = value;
            }
        }
        public bool CanWrite { get; set; }


        public EventService(IEventSerializer serializer)
        {
            _serializer = serializer;
        }

        void serializer_OnMessage(object obj)
        {
            if (obj != null)
            {
                var action = typeof(EventService).GetMethod("Publish").MakeGenericMethod(obj.GetType());
                action.Invoke(this, new []{obj});
            }
            _read++;
        }

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
            if(_serializer!=null && CanWrite)
            {
                _serializer.Serialize(@event);
                _write++;
            }
            var type = typeof(TEvent);
            if(_subscribers.ContainsKey(type))
            {
                ((Action<TEvent>)_subscribers[type])(@event);
            }
            if(EnableProgress && _sw!=null && OnProgress!=null)
            {
                OnProgress(_read, _write, _sw.Elapsed);
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


        public event Action<int, int, TimeSpan> OnProgress;

        bool _enableProgress;
        public bool EnableProgress
        {
            get
            {
                return _enableProgress;
            }
            set
            {
                if(!_enableProgress && value)
                {
                    _sw = Stopwatch.StartNew();
                    _read = 0;
                    _write = 0;
                }
                else
                {
                    _sw.Stop();
                    _sw = null;
                }
                _enableProgress = value;
            }
        }
    }
}
