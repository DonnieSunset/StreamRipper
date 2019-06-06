using StromReisser3000.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StromReisser3000.Domain {
    public class StreamSource : ISerializable {
        private string _filePrefix = null;

        public Guid StreamSourceId { get; set; } = Guid.NewGuid();
        public string StreamUrl { get; set; } = null;
        public string DisplayName { get; set; } = null;

        public string FilePrefix {
            get { return _filePrefix; }
            set {
                if(value != null) {
                    var invalidChars = Path.GetInvalidFileNameChars();
                    if(value.Any(x => invalidChars.Contains(x))) {
                        throw new InvalidDataException($"Invalid file character in string '{value}'.");
                    }
                }

                _filePrefix = value;
            }
        }

        public StreamSource() {
        }

        public ISerializable SerializeTo(MemoryStream mem) {
            throw new NotImplementedException();
        }

        public ISerializable DeserializeFrom(MemoryStream mem) {
            throw new NotImplementedException();
        }
    }
}
