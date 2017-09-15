using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trace.Model
{
    class SearchParam
    {
        public string Field { set; get; }
        public string Value { set; get; }
        public string Compare { set; get; }
        public string LogicOperation { set; get; }

    }
}
