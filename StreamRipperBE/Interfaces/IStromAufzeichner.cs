using StromReisser3000.Domain;
using StromReisser3000.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StromReisser3000.Interfaces {
    public delegate void RecordingEndedDelegate(Exception e);

    public interface IStromAufzeichner : IDisposable {
        IStromReisser CurrentRipper { get; }
        string OutputFileName { get; }
        StromAufzeichnerStates State { get; }

        void StartRecord();
        void PauseRecord();
        void StopRecord();

        event RecordingEndedDelegate RecordingEnded;
    }
}
