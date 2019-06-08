using NAudio.Lame;
using NAudio.Wave;
using StreamRipper.Enums;
using StreamRipper.Exceptions;
using StreamRipper.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamRipper.Domain {
    public class Mp3StreamRipper : AbstractStreamRipperr<Mp3Frame> {
        private Mp3Frame _currentFrame;
        private IMp3FrameDecompressor _decompressor = null;
        private Mp3WaveFormat _waveFormat = null;
        private readonly byte[] _buffer = new byte[16384 * 4];

        public Mp3StreamRipper() {
        }

        protected override void RipLoop(Domain.BufferedStream bufferedStream) {
            _currentFrame = Mp3Frame.LoadFromStream(bufferedStream);

            if(_currentFrame == null) {
                throw new RipFailedException($"Failed to stream from '{CurrentStreamSource.StreamUrl}. The stream is either down or not a MP3 compatible stream.");
            }

            if(_decompressor == null) {
                // don't think these details matter too much - just help ACM select the right codec
                // however, the buffered provider doesn't know what sample rate it is working at
                // until we have a frame
                _waveFormat = new Mp3WaveFormat(
                    _currentFrame.SampleRate,
                    _currentFrame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                    _currentFrame.FrameLength,
                    _currentFrame.BitRate
                );

                _decompressor = new AcmMp3FrameDecompressor(_waveFormat);
                //var appSetting = AppSettings.Current;

                //if(appSetting.RecordBacktrackSeconds > 0) {
                //    // ms per frame = (samples per frame / sample rate(in hz)) * 1000
                //    var backFrameStackSize = (appSetting.RecordBacktrackSeconds * 1000) / (((float)_currentFrame.SampleCount / (float)_currentFrame.SampleRate) * 1000);
                //    BackFrames = new PipeQueue<FrameDecompressedEventArgs<Mp3Frame>>((int)Math.Ceiling(backFrameStackSize));
                //}
            }

            int decompressed = _decompressor.DecompressFrame(_currentFrame, _buffer, 0);
            RaiseFrameDecompressed(_currentFrame, _waveFormat, _decompressor.OutputFormat, _buffer, decompressed);
        }

        protected override void RipLoopCleanup() {
            if(_decompressor != null) {
                _decompressor.Dispose();
                _decompressor = null;
            }

            _waveFormat = null;
            _currentFrame = null;
        }

        protected override void Dispose(bool isDisposing) {
            base.Dispose(isDisposing);
            if(isDisposing) {
                RipLoopCleanup();
            }
        }
    }
}
