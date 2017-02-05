using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TW.Controller
{
    public class ViewElements
    {
        public Chart chart;
        public PropertyGrid pg1, pg2;
        public TrackBar trackBar;
        public TextBox ux0;
        public CheckBox customInitials;

        private ViewElements(Chart chart, PropertyGrid pg1, PropertyGrid pg2, TrackBar trackBar, TextBox ux0, CheckBox customInitials)
        {
            this.chart = chart;
            this.pg1 = pg1;
            this.pg2 = pg2;
            this.trackBar = trackBar;
            this.ux0 = ux0;
            this.customInitials = customInitials;
        }

        public static ViewElements PDEViewElements(Chart chart, PropertyGrid pg1, PropertyGrid pg2, TrackBar trackBar, TextBox ux0, CheckBox customInitials)
        {
            return new ViewElements(chart, pg1, pg2, trackBar, ux0, customInitials);
        }
    }
}
