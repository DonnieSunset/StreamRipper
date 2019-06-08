using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamRipper.Interfaces {
    public interface ISerializable {
        ISerializable SerializeTo(MemoryStream mem);
        ISerializable DeserializeFrom(MemoryStream mem);
    }
}
