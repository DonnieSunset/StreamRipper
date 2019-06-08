using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamRipper.Domain {
    public class BufferedStream : Stream {
        // The logic was taken from the streaming code sample by NAudio and adapted
        // and commented where necessary.
        private const int READ_AHEAD_BUFFER_SIZE = 4096;

        private long _pos = 0;
        private int _readAheadLen = 0;
        private int _readAheadOff = 0;
        private readonly byte[] _readAheadBuf = new byte[READ_AHEAD_BUFFER_SIZE];
        private readonly Stream _src;

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite { get; } = false;
        public override long Length { get { return _pos; } }

        public BufferedStream(Stream src) {
            _src = src;
        }

        public override long Position {
            get { return _pos; }
            set { throw new NotImplementedException(); }
        }

        public override void Flush() { throw new NotImplementedException(); }

        public override int Read(byte[] buffer, int offset, int count) {
            int read = 0;

            // Read until we reached the 'count' requested, or until we encounter EOS.
            while(read < count) {
                int readAheadAvail = _readAheadLen - _readAheadOff;
                int bytesRequired = count - read;

                // Check if our read-ahead-buffer has some data available. If not, fill the
                // buffer, otherwise copy it to the output array piece by piece.
                if(readAheadAvail > 0) {
                    int copyCount = Math.Min(readAheadAvail, bytesRequired);
                    Array.Copy(_readAheadBuf, _readAheadOff, buffer, offset + read, copyCount);
                    read += copyCount;
                    _readAheadOff += copyCount; // For when bytesRequired < readAheadAvail.
                } else {
                    _readAheadOff = 0;
                    _readAheadLen = _src.Read(_readAheadBuf, 0, READ_AHEAD_BUFFER_SIZE);
                    if(_readAheadLen == 0) {
                        // EOS
                        break;
                    }
                }
            }

            _pos += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
        public override void SetLength(long value) { throw new NotImplementedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }
    }
}
