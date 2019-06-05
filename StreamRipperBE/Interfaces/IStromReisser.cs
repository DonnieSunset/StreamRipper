using NAudio.Wave;
using StromReisser3000.Domain;
using StromReisser3000.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StromReisser3000.Interfaces {
    public delegate void StreamEndedDelegate(Exception e);

    public interface IStromReisser : IDisposable {
        StreamSource CurrentStreamSource { get; }
        StromReisserStates State { get; }

        void StartRip(StreamSource streamSource);
        void StopRip();
        event StreamEndedDelegate StreamEnded;
    }
}
