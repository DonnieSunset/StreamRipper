using NAudio.Lame;
using NAudio.Wave;
using StromReisser3000.Enums;
using StromReisser3000.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StromReisser3000.Domain {
    internal class Mp3StromAufzeichner : AbstractStromAufzeichner<Mp3Frame> {
        private AbstractStromReisser<Mp3Frame> _ripper;
        private NAudio.Lame.LameMP3FileWriter _mp3writer = null;
        private Stream _fileStream = null;
        private object _lck = new object();

        public override IStromReisser CurrentRipper { get { return _ripper; } }

        public Mp3StromAufzeichner(AbstractStromReisser<Mp3Frame> ripper, string outputFileName) {
            if(ripper == null) {
                throw new ArgumentNullException(nameof(ripper));
            }

            if(string.IsNullOrWhiteSpace(outputFileName)) {
                throw new ArgumentNullException(nameof(outputFileName));
            }

            var invalidFileChars = Path.GetInvalidFileNameChars();
            var rawOutputFileName = Path.GetFileName(outputFileName);
            if(rawOutputFileName.Any(x => invalidFileChars.Contains(x))) {
                throw new InvalidDataException($"The filename '{rawOutputFileName}' contains invalid characters.");
            }

            var invalidDirChars = Path.GetInvalidPathChars();
            var outputDir = Path.GetDirectoryName(outputFileName);
            if(outputDir.Any(x => invalidDirChars.Contains(x))) {
                throw new InvalidDataException($"The directory '{outputDir}' contains invalid characters.");
            }

            OutputFileName = outputFileName;
            if(!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir)) {
                Directory.CreateDirectory(outputDir);
            }

            _ripper = ripper;
            _ripper.StreamEnded += (e) => StopRecord(e);
        }

        public override void StartRecord() {
            lock(_lck) {
                if(State == StromAufzeichnerStates.Recording) {
                    return;
                }

                if(State != StromAufzeichnerStates.Paused) {
                    _fileStream = File.Open(OutputFileName, FileMode.Create);
                }

                State = StromAufzeichnerStates.Recording;
                var backItems = _ripper.BackFrames?.Items;
                if(backItems != null) {
                    foreach(var f in backItems) {
                        WriteFrame(f);
                    }
                }

                _ripper.FrameDecompressed += OnFrameDecompressed;
            }
        }

        public override void PauseRecord() {
            lock (_lck) {
                if((State == StromAufzeichnerStates.Stopped) || (CurrentRipper == null)) {
                    return;
                }

                _ripper.FrameDecompressed -= OnFrameDecompressed;
                State = StromAufzeichnerStates.Paused;
            }
        }

        public override void StopRecord() {
            StopRecord(null);
        }

        private void StopRecord(Exception e) {
            lock (_lck) {
                _ripper.FrameDecompressed -= OnFrameDecompressed;
                _ripper.StreamEnded -= (ex) => StopRecord(ex);

                if(_mp3writer != null) {
                    _mp3writer.Dispose();
                    _mp3writer = null;
                }

                if(_fileStream != null) {
                    _fileStream.Dispose();
                    _fileStream = null;
                }

                State = StromAufzeichnerStates.Stopped;

                if(_recordingEnded != null) {
                    _recordingEnded(e);
                }
            }
        }

        public override void OnFrameDecompressed(FrameDecompressedEventArgs<Mp3Frame> e) {
            lock (_lck) {
                WriteFrame(e);
            }
        }

        private void WriteFrame(FrameDecompressedEventArgs<Mp3Frame> e) {
            if(State != StromAufzeichnerStates.Recording) {
                return;
            }

            if(e.DecompressedDataLen <= 0) {
                return;
            }

            if(_mp3writer == null) {
                _mp3writer = new LameMP3FileWriter(
                    _fileStream,
                    e.DecompressedFormat,
                    e.Frame.BitRate,
                    new ID3TagData {
                        Title = $"{e.StreamSource.DisplayName} ({DateTime.Now.ToString()})",
                        Comment = e.StreamSource.StreamUrl
                    }
                );
            }

            _mp3writer.Write(e.DecompressedData, 0, e.DecompressedDataLen);
        }
    }
}
