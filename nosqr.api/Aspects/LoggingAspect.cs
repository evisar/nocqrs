using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api.Aspects
{
    public class LoggingAspect<T>: IAspect<T>
        where T: ICommand
    {
        readonly ILogger _logger;

        public LoggingAspect(ILogger logger)
        {
            _logger = logger;
        }

        public Action<Action<T>, T> GetAspect()
        {
            return (x, y) =>
            {
                var type = typeof(T);
                try
                {
                    _logger.Info(string.Format("Entering: {0}", type.FullName));
                    x(y);
                    _logger.Info(string.Format("Finishing: {0}", type.FullName));
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Error: {0}", type.FullName), ex);
                    throw;
                }
            };
        }
    }
}
