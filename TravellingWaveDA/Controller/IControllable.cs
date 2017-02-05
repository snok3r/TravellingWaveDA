using System;
using System.Collections.Generic;

namespace TW.Controller
{
    public interface IControllable
    {
        void ToSolveFurther(bool b);
        void Dispose();
        void Reallocate(bool chckd);
        void CheckToLoad(IEnumerable<String> paramsNeedReload);
        bool Solve(IProgress<int> progress);
        void ClearPlot();

        int TrackBarMax();
        double ChartXMin();
        double ChartXMax();
        void Plot();
        void Plot(int j);
    }
}
