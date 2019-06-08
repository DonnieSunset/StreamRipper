using StreamRipper.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamRipper.Enums;
using System.Collections.ObjectModel;

namespace StreamRipper.Domain {
    public abstract class AbstractStreamRecorder<TFrameType> : IStreamRecorder {
        public abstract IStreamRipper CurrentRipper { get; }
        public string OutputFileName { get; protected set; } = null;
        public StreamRecorderStates State { get; protected set; } = StreamRecorderStates.Stopped;

        private object _lckEvt = new object();
        private readonly ICollection<RecordingEndedDelegate> _recordingEndedHandlers = new Collection<RecordingEndedDelegate>();
        protected RecordingEndedDelegate _recordingEnded;
        public event RecordingEndedDelegate RecordingEnded {
            add {
                lock (_lckEvt) if((_recordingEnded == null) || (!_recordingEnded.GetInvocationList().Contains(value))) {
                    _recordingEnded += value;
                    _recordingEndedHandlers.Add(value);
                }
            }

            remove {
                lock (_lckEvt) {
                    _recordingEnded -= value;
                    _recordingEndedHandlers.Remove(value);
                }
            }
        }

        public abstract void OnFrameDecompressed(FrameDecompressedEventArgs<TFrameType> e);
        public abstract void PauseRecord();
        public abstract void StartRecord();
        public abstract void StopRecord();

        public void Dispose() {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public virtual void Dispose(bool isDisposing) {
            if(isDisposing) {
                StopRecord();

                lock(_lckEvt) {
                    foreach(var h in _recordingEndedHandlers) {
                        _recordingEnded -= h;
                    }

                    _recordingEndedHandlers.Clear();
                }
            }
        }
    }
}
