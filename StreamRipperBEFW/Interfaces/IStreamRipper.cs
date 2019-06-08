using NAudio.Wave;
using StreamRipper.Domain;
using StreamRipper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamRipper.Interfaces {
    public delegate void StreamEndedDelegate(Exception e);

    public interface IStreamRipper : IDisposable {
        StreamSource CurrentStreamSource { get; }
        StreamRipperStates State { get; }

        void StartRip(StreamSource streamSource);
        void StopRip();
        event StreamEndedDelegate StreamEnded;
    }
}
