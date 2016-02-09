using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace nosqr.api.Aspects
{
    public class TransactionAspect<T>: IAspect<T>
        where T: ICommand
    {
        public Action<Action<T>, T> GetAspect()
        {
            return (x, y) =>
            {
                using (var tran = new TransactionScope())
                {
                    x(y);
                }
            };
        }
    }
}
