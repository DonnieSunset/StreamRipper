using StromReisser3000.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StromReisser3000.Enums;
using System.Threading;
using System.Net;
using System.IO;
using NAudio.Wave;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace StromReisser3000.Domain {
    public delegate void FrameDecompressedDelegate<TFrameType>(FrameDecompressedEventArgs<TFrameType> e);

    public abstract class AbstractStromReisser<TFrameType> : IStromReisser {
        private const int STOP_WAIT_TIMEOUT = 10000;

        private BackgroundWorker _worker = null;
        private EventWaitHandle _stopHandle = null;

        public StreamSource CurrentStreamSource { get; private set; } = null;

        public StromReisserStates State { get; private set; } = StromReisserStates.Stopped;

        public PipeQueue<FrameDecompressedEventArgs<TFrameType>> BackFrames { get; protected set; }

        public EventWaitHandle StopHandle {
            get { return _stopHandle; }
            set { _stopHandle = value; }
        }

        private object _lckEvt = new object();
        private readonly ICollection<FrameDecompressedDelegate<TFrameType>> _frameDecompressedHandlers = new Collection<FrameDecompressedDelegate<TFrameType>>();
        private readonly ICollection<StreamEndedDelegate> _streamEndedHandlers = new Collection<StreamEndedDelegate>();

        private event FrameDecompressedDelegate<TFrameType> _frameDecompressed;
        private event StreamEndedDelegate _streamEnded;

        public event FrameDecompressedDelegate<TFrameType> FrameDecompressed {
            add {
                lock (_lckEvt) if((_frameDecompressed == null) || (!_frameDecompressed.GetInvocationList().Contains(value))) {
                    _frameDecompressed += value;
                    _frameDecompressedHandlers.Add(value);
                }
            }

            remove {
                lock (_lckEvt) {
                    _frameDecompressed -= value;
                    _frameDecompressedHandlers.Remove(value);
                }
            }
        }

        public event StreamEndedDelegate StreamEnded {
            add {
                lock (_lckEvt) if((_streamEnded == null) || (!_streamEnded.GetInvocationList().Contains(value))) {
                    _streamEnded += value;
                    _streamEndedHandlers.Add(value);
                }
            }

            remove {
                lock (_lckEvt) {
                    _streamEnded -= value;
                    _streamEndedHandlers.Remove(value);
                }
            }
        }

        protected abstract void RipLoop(Domain.BufferedStream bufferedStream);
        protected abstract void RipLoopCleanup();

        protected void RaiseFrameDecompressed(TFrameType frame, WaveFormat frameFormat, WaveFormat decompressedFormat, byte[] decompressedData, int decompressedDataLen) {
            var args = new FrameDecompressedEventArgs<TFrameType> {
                StreamSource = CurrentStreamSource,
                Frame = frame,
                FrameFormat = frameFormat,
                DecompressedFormat = decompressedFormat,
                DecompressedData = decompressedData,
                DecompressedDataLen = decompressedDataLen
            };

            if(_frameDecompressed != null) {
                _frameDecompressed(args);
            }

            if(BackFrames != null) {
                BackFrames.Enqueue(args);
            }
        }

        public void StartRip(StreamSource streamSource) {
            if(State != StromReisserStates.Stopped) {
                throw new InvalidOperationException("The ripper is already running or not fully stopped yet.");
            }

            _worker = new BackgroundWorker();
            _worker.RunWorkerCompleted += (sender, e) => {
                if(_streamEnded != null) {
                    _streamEnded(e.Error);
                }
            };

            _worker.DoWork += (sender, e) => {
                var req = (HttpWebRequest)HttpWebRequest.Create(streamSource.StreamUrl);

                HttpWebResponse resp;

                try {
                    resp = (HttpWebResponse)req.GetResponse();
                } catch(WebException ex) {
                    if(ex.Status != WebExceptionStatus.RequestCanceled) {
                        throw;
                    }

                    return;
                }

                try {
                    _stopHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    State = StromReisserStates.Running;
                    CurrentStreamSource = streamSource;

                    using(var responseStream = resp.GetResponseStream()) {
                        var bufferedStream = new Domain.BufferedStream(responseStream);

                        do {
                            RipLoop(bufferedStream);
                            if(_worker.CancellationPending) {
                                e.Cancel = true;
                                break;
                            }
                        } while(State == StromReisserStates.Running);
                    }
                } catch(EndOfStreamException) {
                    // reached the end of the MP3 file / stream
                } catch(WebException) {
                    // probably we have aborted download from the GUI thread
                } finally {
                    RipLoopCleanup();
                    State = StromReisserStates.Stopped;
                    _stopHandle.Set();
                }
            };

            _worker.RunWorkerAsync();
        }

        public void StopRip() {
            if(State != StromReisserStates.Running) {
                return;
            }

            State = StromReisserStates.Stopping;
            if(_stopHandle != null) {
                if(!_stopHandle.WaitOne(STOP_WAIT_TIMEOUT)) {
                    // The dispose below might cause problems here.
                    _worker.CancelAsync();
                }

                _stopHandle.Dispose();
                _worker.Dispose();                
            }

            _stopHandle = null;
            _worker = null;
            BackFrames = null;
            CurrentStreamSource = null;

            // This is actually done in the finally block of the thread, but
            // we set it in case we have to harshly abandon a locked thread.
            State = StromReisserStates.Stopped;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing) {
            if(isDisposing) {
                StopRip();

                lock (_lckEvt) {
                    foreach(var h in _frameDecompressedHandlers) {
                        _frameDecompressed -= h;
                    }

                    foreach(var h in _streamEndedHandlers) {
                        _streamEnded -= h;
                    }

                    _streamEndedHandlers.Clear();
                }
            }
        }
    }
}
