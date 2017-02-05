using TW.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TW.Controller
{
    public abstract class AbstractController<T> : IControllable where T : AbstractFHN, new()
    {
        protected bool allocate = true;
        protected bool solveFurther = false;
        protected static HashSet<String> paramsNeedReload;

        protected T[] fhn;
        protected ViewElements view;

        abstract public double ChartXMin();
        abstract public double ChartXMax();
        abstract public void Plot();
        abstract public void Plot(int j);
        abstract public int TrackBarMax();

        protected AbstractController(ViewElements viewElements)
        { this.view = viewElements; }

        /// <summary>
        /// if 'b' is true, then
        /// we're solving further
        /// </summary>
        public void ToSolveFurther(bool b)
        { solveFurther = b; }

        /// <summary>
        /// Makes all the data point to null
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < fhn.Length; i++)
            {
                fhn[i].Dispose();
                fhn[i] = null;
            }
            fhn = null;
        }

        /// <summary>
        /// Call when you need to reload equations
        /// or to reassign them to property grid
        /// </summary>
        public void Reallocate(bool chckd)
        {
            int size;
            if (chckd) size = 2;
            else size = 1;

            fhn = new T[size];
            for (int i = 0; i < size; i++)
                fhn[i] = new T();

            view.pg1.SelectedObject = fhn[0];

            if (size == 2) view.pg2.SelectedObject = fhn[1];
            else view.pg2.SelectedObject = null;
        }

        /// <summary>
        /// Checks whether you need to reallocate
        /// arrays or not
        /// </summary>
        public void CheckToLoad(IEnumerable<String> parameters)
        {
            // need to reallocate, if paramsNeedReload contains given label
            allocate = paramsNeedReload.Overlaps(parameters);
        }

        /// <summary>
        /// Call to solve equations
        /// <para>Returns false, if computation error occurred,
        /// true otherwise.</para>
        /// </summary>
        public virtual bool Solve(IProgress<int> progress)
        {
            Stopwatch stopwatch = Stopwatch.StartNew(); //creates and start the instance of Stopwatch
            SolveAllocate();
            progress.Report(33);

            SolveInitials();
            progress.Report(66);

            for (int i = 0; i < fhn.Length; i++)
                if (!fhn[i].Solve())
                    return false;
            progress.Report(100);

            stopwatch.Stop();
            Debug.WriteLine("Solved in " + stopwatch.ElapsedMilliseconds / 1000.0 + "sec");
            return true;
        }

        private void SolveAllocate()
        {
            for (int i = 0; i < fhn.Length; i++)
                if (allocate && !solveFurther)
                    fhn[i].Allocate();
                else
                    fhn[i].Reload();
        }

        private void SolveInitials()
        {
            for (int i = 0; i < fhn.Length; i++)
            {
                if (solveFurther)
                    fhn[i].InitialsFurther();
                else
                {
                    if (view.customInitials != null && view.customInitials.Checked)
                        fhn[i].Initials(view.ux0.Text);
                    else
                        fhn[i].Initials();
                }
            }
        }

        /// <summary>
        /// Clears all the plot data
        /// </summary>
        public virtual void ClearPlot()
        {
            for (int i = 0; i < view.chart.Series.Count(); i++)
                view.chart.Series[i].Points.Clear();
        }
    }
}
