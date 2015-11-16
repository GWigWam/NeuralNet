using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingGui.PlotModels {

    public class SSEHistModel {
        private ColumnSeries SSE;
        private List<ColumnItem> SSEData => (List<ColumnItem>)SSE.ItemsSource;
        private CategoryAxis XAxis;

        public int BarCount {
            get; set;
        } = 10;

        public SSEHistModel() {
            Model = new PlotModel();
            XAxis = new CategoryAxis() { Position = AxisPosition.Bottom, Title = "SSE" };
            Model.Axes.Add(XAxis);
            Model.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Title = "Frequency" });

            SSE = new ColumnSeries() { Title = "SSE", ColumnWidth = 1 };
            SSE.ItemsSource = new List<ColumnItem>();

            Model.Series.Add(SSE);
        }

        public void Update(double[] sses) {
            if(BarCount > 0) {
                var min = sses.Min();
                var max = sses.Max();
                var dif = max - min;
                var step = dif / (BarCount - 1);

                XAxis.Minimum = -1;
                XAxis.Maximum = BarCount + 1;

                double[] barHeighs = new double[BarCount];

                foreach(var cur in sses) {
                    var barNr = ((int)Math.Floor((cur - min) / step));
                    if(barNr >= barHeighs.Length)
                        barNr--;
                    if(barNr < 0)
                        barNr = 0;

                    barHeighs[barNr]++;
                }

                SSEData.Clear();
                for(int barNr = 0; barNr < barHeighs.Length; barNr++) {
                    SSEData.Add(new ColumnItem(barHeighs[barNr], barNr));
                }

                Model.InvalidatePlot(true);
            }
        }

        public PlotModel Model {
            get; private set;
        }
    }
}