using Autofac;
using nosqr.api;
using nosqr.api.Aspects;
using nosqr.api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nocqrs.sample
{
    public static class DomainContainerExtensions
    {
        public static Action<T> CreateHandler<T>(this IContainer container)
            where T : ICommand
        {
            var eventType = typeof(T);
            var featureType = eventType.BaseType.GetGenericArguments().FirstOrDefault();
            var entityType = eventType.BaseType.GetGenericArguments().Skip(1).FirstOrDefault();
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
