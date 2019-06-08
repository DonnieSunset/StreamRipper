using StreamRipper.Domain;
using StreamRipper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamRipper.Interfaces {
    public delegate void RecordingEndedDelegate(Exception e);

    public interface IStreamRecorder : IDisposable {
        IStreamRipper CurrentRipper { get; }
        string OutputFileName { get; }
        StreamRecorderStates State { get; }

        void StartRecord();
        void PauseRecord();
        void StopRecord();

        event RecordingEndedDelegate RecordingEnded;
    }
}
