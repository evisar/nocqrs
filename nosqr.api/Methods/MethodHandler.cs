using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Methods
{
    public class MethodHandler<TCommand, TObject> : IMethodHandler<TCommand, TObject>
    where TCommand : IMethodCommand<TObject>
    {
        public void Handle(TCommand command)
        {
            var entity = command.Args.FirstOrDefault();
            var args = command.Args.Skip(1).Take(command.Args.Length-1).ToArray();
            var commandType = typeof(TObject);
            var target = Activator.CreateInstance(commandType, entity);
            var mi = commandType.GetMethod(typeof(TCommand).Name);
            mi.Invoke(target, args);
        }
    }
}
