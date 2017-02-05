using TW.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TW.View.Other
{
    public partial class WindowTemplate : Form
    {
        protected IControllable controller;
        private Object monitor = new Object();
        private HashSet<String> paramsNeedReload = new HashSet<String>();

        protected WindowTemplate()
        {
            InitializeComponent();
            prBarSolve.Maximum = 100;
        }

        private void WindowTemplate_Load(object sender, EventArgs e)
        {
            if (controller != null) controller.Reallocate(checkBox2ndEq.Checked);
            Change2ndLegendVisibility(checkBox2ndEq.Checked);
            paramsNeedReload.Add("start");
        }

        private void WindowTemplate_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerT.Enabled = false;
            controller.Dispose();
            chart.Series.Clear();
            Dispose(true);
        }

        protected virtual void btnPlot_Click(object sender, EventArgs e)
        {
            SetPlot();

            // start timer, if radio button is checked
            timerT.Enabled = rdBtnTmr.Checked;
        }

        protected virtual void timerT_Tick(object sender, EventArgs e)
        {
            controller.Plot();
        }

        protected virtual void trBarT_Scroll(object sender, EventArgs e)
        {
            controller.Plot(trBarT.Value);
        }

        protected virtual void DisablePlotBtn()
        {
            btnPlot.Enabled = false;
            btnSolve.Enabled = true;
            btnSolveFurther.Enabled = false;

            lblError.Visible = false;

            trBarT.Value = 0;
            trBarT.Enabled = false;
            timerT.Enabled = false;

            prBarSolve.Value = 0;

            controller.ToSolveFurther(false);
        }

        protected virtual void EnablePlotBtn()
        {
            btnSolve.Enabled = false;
            btnPlot.Enabled = true;
            btnSolveFurther.Enabled = true;

            trBarT.Maximum = controller.TrackBarMax();
            trBarT.Value = 0;
            trBarT.Enabled = true;
        }

        protected virtual void SetPlot()
        {
            chart.ChartAreas[0].AxisX.Minimum = controller.ChartXMin();
            chart.ChartAreas[0].AxisX.Maximum = controller.ChartXMax();

            chart.ChartAreas[0].AxisY.Minimum = Convert.ToDouble(txtBoxMinUV.Text);
            chart.ChartAreas[0].AxisY.Maximum = Convert.ToDouble(txtBoxMaxUV.Text);

            chart.ChartAreas[0].AxisX.Interval = Convert.ToInt32((controller.ChartXMax() - controller.ChartXMin()) / 10.0);
            chart.ChartAreas[0].AxisY.Interval = (chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 10.0;

            chart.Series[1].Color = Color.LimeGreen;
        }

        protected virtual void Change2ndLegendVisibility(bool isSecondEqChecked)
        {
            for (int i = 0; i < 1; i++)
                chart.Series[i + 1].IsVisibleInLegend = isSecondEqChecked;
        }

        private void checkBox2ndEq_CheckedChanged(object sender, EventArgs e)
        {
            controller.Reallocate(checkBox2ndEq.Checked);
            paramsNeedReload.Add("start");
            DisablePlotBtn();

            Change2ndLegendVisibility(checkBox2ndEq.Checked);
        }

        private async void btnSolve_Click(object sender, EventArgs e)
        {
            if (!Monitor.IsEntered(monitor))
            {
                try
                {
                    Monitor.Enter(monitor);
                    controller.CheckToLoad(paramsNeedReload);

                    bool result = false;
                    var progress = new Progress<int>(percent => prBarSolve.Value = percent);

                    result = await Task.Factory.StartNew(() => controller.Solve(progress));

                    if (result)
                        EnablePlotBtn();
                    else
                        lblError.Visible = true;
                }
                catch (NullReferenceException)
                {
                    Debug.WriteLine("Solution failed");
                }
                finally
                {
                    paramsNeedReload.Clear();
                    Monitor.Exit(monitor);
                }
            }
        }

        private void btnSolveFurther_Click(object sender, EventArgs e)
        {
            DisablePlotBtn();
            controller.ToSolveFurther(true);
            btnSolve_Click(sender, e);
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.OldValue == e.ChangedItem.Value)
                return;

            DisablePlotBtn();
            paramsNeedReload.Add(e.ChangedItem.Label);
        }

        private void btnTune_Click(object sender, EventArgs e)
        {
            chart.ChartAreas[0].AxisY.Minimum = Convert.ToDouble(txtBoxMinUV.Text);
            chart.ChartAreas[0].AxisY.Maximum = Convert.ToDouble(txtBoxMaxUV.Text);

            chart.ChartAreas[0].AxisY.Interval = (chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) / 10.0;
        }

        private void btnStopTimer_Click(object sender, EventArgs e)
        {
            if (timerT.Enabled)
            {
                rdBtnTmr.Checked = false;
                timerT.Enabled = false;
            }
        }

        protected virtual void btnAbout_Click(object sender, EventArgs e) { }
    }
}
