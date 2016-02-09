using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api
{
    public static class YCombinatorExtension
    {
        /// <summary>
        /// Y-Combinator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Action<T> Y<T>(this Action<T> x, IEnumerable<Action<Action<T>, T>> y)
        {
            if (y==null || y.Count() == 0)
                return x;
            return
                Y(a => y.First()(x, a), y.Skip(1).Take(y.Count() - 1));
        }
    }
}
