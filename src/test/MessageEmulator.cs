using Gerakul.FastSql;
using Monik.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Monik.Service.Test
{
    public class MessageEmulator : IObject
    {
        private const int DelayOnProcess = 1500; //in ms

        private readonly IMonik _monik;

        private Task _emulatorTask;
        private readonly CancellationTokenSource _emulatorCancellationTokenSource = new CancellationTokenSource();

        public MessageEmulator(IMonik monik)
        {
            _monik = monik;
        }

        public void OnStart()
        {
            _emulatorTask = Task.Run(() => { OnProcessTask(); });
        }

        public void OnStop()
        {
            _emulatorCancellationTokenSource.Cancel();
        }

        private void OnProcessTask()
        {
            while (!_emulatorCancellationTokenSource.IsCancellationRequested)
            {
                Task.Delay(DelayOnProcess).Wait();

                _monik.ApplicationInfo("New message from emulator");
                _monik.Measure("TestAccum", AggregationType.Accumulator, 12);
            }
        }
    } //end of class
}
