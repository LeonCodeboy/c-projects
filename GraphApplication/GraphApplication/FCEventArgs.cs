using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystAnalys_lr1
{
    class FCEventArgs : EventArgs
    {
        private String cParameter;
        public String cParam
        {
            get
            {
                return cParameter;
            }
            set
            {
                cParameter = value;
            }
        }
    }
}
