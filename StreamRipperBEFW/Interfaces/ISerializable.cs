using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StromReisser3000.Interfaces {
    public interface ISerializable {
        ISerializable SerializeTo(MemoryStream mem);
        ISerializable DeserializeFrom(MemoryStream mem);
    }
}
