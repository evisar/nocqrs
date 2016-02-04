using nosqr.api.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Services
{
    public static class EventServiceMethodExtensions
    {
        public static IDisposable SubscribeType(this IEventService bus, Type eventType, Type targetType)
        {
            var subscribe = typeof(IEventService).GetMethod("Subscribe").MakeGenericMethod(eventType);
            var handlerType = typeof(MethodHandler<,>).MakeGenericType(eventType, targetType);
            var handler = Activator.CreateInstance(handlerType);
            var actionType = typeof(Action<>).MakeGenericType(eventType);
            var action = handlerType.GetMethod("Handle").CreateDelegate(actionType, handler);

            return subscribe.Invoke(bus, new object[] { action }) as IDisposable;
        }
    }
}
