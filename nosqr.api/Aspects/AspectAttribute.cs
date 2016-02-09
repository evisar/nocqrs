using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Aspects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class AspectAttribute: Attribute
    {
        public Type Type { get; private set; }

        public AspectAttribute(Type type)
        {
            if(!type.IsGenericType && type.BaseType.GetGenericTypeDefinition()!=typeof(IAspect<>))
            {
                throw new InvalidOperationException();
            }
            this.Type = type;
        }
    }
}
