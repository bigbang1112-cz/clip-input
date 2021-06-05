using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipInput
{
    public class TMTurboNotSupportedException : Exception
    {
        public TMTurboNotSupportedException() : base("Trackmania Turbo replays are not supported.")
        {

        }
    }
}
