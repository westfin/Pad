using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad.ViewModels
{
    public sealed class DocumentViewModel
    {
        private string name;
        public  string Name { get { return name; } }

        public Document Document
        {
            get;
            set;
        }
    }
}
