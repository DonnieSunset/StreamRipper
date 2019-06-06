using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StromReisser3000.Exceptions {
    public class RipFailedException : Exception {
        public RipFailedException(string message, Exception innerException = null) : base(message, innerException) {
        }
    }
}
