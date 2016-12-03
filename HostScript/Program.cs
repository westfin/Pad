using Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostScript
{
    class Program
    {
        static void Main(string[] args)
        {
           ScriptHost.Run(args[0], Int32.Parse(args[1]));
        }
    }
}
