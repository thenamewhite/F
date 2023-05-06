using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// author  (hf) time：2023/3/8 16:27:53

namespace F
{
    public class DebugAttributeTitle : Attribute
    {

        public string Title;
        public DebugAttributeTitle(string title)
        {
            Title = title;
        }
    }
}
