using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipInput
{
    public class NoInputsException : Exception
    {
        public NoInputsException() : base("No inputs are available in this GBX.")
        {

        }
    }
}
