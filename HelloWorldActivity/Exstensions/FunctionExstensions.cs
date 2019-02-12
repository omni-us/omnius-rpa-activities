using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnius
{
    public static class FunctionExstensions
    {
        public static Func<In1, Out2> Then<In1, Out1, Out2>(this Func<In1, Out1> f1, Func<Out1, Out2> f2)
        {
            return x => f2(f1(x));
        }

        public static Func<In1, Out2> Bind<In1, Out1, Out2>(this Func<Out1, Out2> f1, Func<In1, Out1> f2)
        {
            return x => f1(f2(x));
        }

        public static Func<In1, Func<In2, Out>> Curr<In1, In2, Out>(this Func<In1, In2, Out> f)
        {
            return p1 => p2 => f(p1, p2);
        }

        public static Func<In2, Out> Apply<In1, In2, Out>(this Func<In1, In2, Out> f, In1 param)
        {
            return x => f(param, x);
        }

        public static Func<In2, In3, Out> Apply<In1, In2, In3, Out>(this Func<In1, In2, In3, Out> f, In1 param)
        {
            return (p2, p3) => f(param, p2, p3);
        }

        public static T Identity<T>(T input)
        {
            return input;
        }
    }
}
