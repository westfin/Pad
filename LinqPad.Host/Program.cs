using LinqPadHosting;
using System;

namespace LinqPad.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("starting service");
            LinqPadHost.RunHost(args[0], Int32.Parse(args[1]));
        }
    }
}
