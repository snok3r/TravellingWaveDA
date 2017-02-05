using TW.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TW.Controller
{
    public class PDEController : AbstractController<PDE>
    {
        public PDEController(ViewElements viewElements)
            : base(viewElements)
        {
            if (paramsNeedReload == null)
                paramsNeedReload = new HashSet<String>(new String[] { "N", "M", "T", "L", "start" });
        }

        /// <summary>
        /// Returns chart's minimum X bound
        /// </summary>
        public override double ChartXMin()
        { return -fhn[0].L; }

        /// <summary>
        /// Returns chart's maximum X bound
        /// </summary>
        public override double ChartXMax()
        { return fhn[0].L; }

        /// <summary>
        /// Returns maximum value for trackBar
        /// </summary>
        public override int TrackBarMax()
        { return fhn[0].M - 1; }

        /// <summary>
        /// Plots layer 'tj'
        /// </summary>
        public override void Plot(int tj)
        {
            ClearPlot();

            for (int i = 0; i < fhn.Length; i++)
                Plot(tj, fhn[i], i);
        }

        /// <summary>
        /// Plots layer tj with variable trackbar value
        /// </summary>
        public override void Plot()
        {
            if (view.trackBar.Value < TrackBarMax())
                Plot(view.trackBar.Value++);
            else
                view.trackBar.Value = 0;
        }

        /// <summary>
        /// Plots full j segment
        /// </summary>
        private void Plot(int j, PDE obj, int numEq)
        {
            for (int i = 0; i < obj.N; i++)
            {
                double x = obj.GetX(i);

                view.chart.Series[numEq].Points.AddXY(x, obj.GetU(j, i));
            }
        }

        /// <summary>
        /// Returns velocity at trackBarValue point
        /// </summary>
        public List<double> GetVelocity(int trackBarValue)
        {
            List<double> result = new List<double>();

            for (int i = 0; i < fhn.Length; i++)
                result.Add(fhn[i].GetVelocity(trackBarValue));

            return result;
        }
    }
}
