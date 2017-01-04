using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad.Editor
{
    public sealed class LinqPadOut : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF32;
    }
}
