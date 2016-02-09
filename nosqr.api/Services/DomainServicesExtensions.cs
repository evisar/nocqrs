using Autofac;
using nosqr.api.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Services
{
    public static class DomainServicesExtensions
    {
        public static Action<T> CreateHandler<T>(this IContainer container)
            where T : ICommand
        {
            var eventType = typeof(T);
            var baseType = (from @if in eventType.GetInterfaces()
                            where @if.IsGenericType && @if.GetGenericTypeDefinition() == typeof(ICommand<,>)
                            select @if).FirstOrDefault();
            var featureType = baseType.GetGenericArguments().FirstOrDefault();
            var entityType = baseType.GetGenericArguments().Skip(1).FirstOrDefault();
            var operation = featureType.GetMethod(eventType.Name);

            Action<T> handler = x =>
            {
                dynamic cmd = x;
                var feature = container.Resolve(featureType, new PositionalParameter(0, cmd.Model));
                operation.Invoke(feature, cmd.Arguments);
            };

            var aspects = from a in container.Resolve<IEnumerable<IAspect<T>>>().Distinct()
                          from t in typeof(T).GetCustomAttributes(typeof(AspectAttribute), false).OfType<AspectAttribute>()
                          where a.GetType().GetGenericTypeDefinition() == t.Type
                          select a.GetAspect();

            return handler.Y(aspects);
        }
    }
}
