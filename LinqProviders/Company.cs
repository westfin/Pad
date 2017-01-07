using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqProviders.Attributes;

namespace LinqProviders
{
    public class Company
    {
        [ExcelColumn("Name")]
        public string Name
        {
            get;
            set;
        }

        [ExcelColumn("CEO")]
        public string CEO
        {
            get;
            set;
        }
        [ExcelColumn("EmployeeCount")]
        public double EmployeeCount
        {
            get;
            set;
        }

        [ExcelColumn("StartDate")]
        public DateTime StartDate
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{Name} {CEO} {EmployeeCount} {StartDate}";
        }
    }
}
