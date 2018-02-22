using System;
using System.Collections.Generic;
using System.Text;

namespace ImageProcessing.Contracts
{
    public interface IMetricReporter
    {
        IDisposable StartScope();

        void Hit();
    }
}
