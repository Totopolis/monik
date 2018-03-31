using System;
using System.Runtime.CompilerServices;

namespace Monik.Client
{
    public class TimingHelper
    {
        private DateTime _from;
        private readonly IClientControl _control;

        private TimingHelper(IClientControl aControl)
        {
            _from = DateTime.Now;
            _control = aControl;
        }

        public static TimingHelper Create(IClientControl aControl)
        {
            return new TimingHelper(aControl);
        }

        public void Begin()
        {
            _from = DateTime.Now;
        }

        public void EndAndLog([CallerMemberName] string aSource = "")
        {
            var delta = DateTime.Now - _from;
            _control.ApplicationInfo("{0} execution time: {1}ms", aSource, delta.TotalMilliseconds);
        }
    }
}
