using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamRipper.Domain {
    public class FrameDecompressedEventArgs<T> {
        public StreamSource StreamSource { get; set; }
        public T Frame { get; set; }
        public WaveFormat FrameFormat { get; set; }
        public WaveFormat DecompressedFormat { get; set; }
        public byte[] DecompressedData { get; set; }
        public int DecompressedDataLen { get; set; }
    }
}
