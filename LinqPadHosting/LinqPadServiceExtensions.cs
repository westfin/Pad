using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPadHosting
{
    public static class LinqPadServiceExtensions
    {
        public static bool IsAlive(this Process process)
        {
            try
            {
                return !process.HasExited;
            }
            catch
            {
                return false;
            }
        }
    }
}
