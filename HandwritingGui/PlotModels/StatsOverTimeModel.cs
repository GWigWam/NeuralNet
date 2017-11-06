using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingGui.PlotModels {

    public class StatsOverTimeModel {
        private LineSeries SSE;
        private List<DataPoint> SSEData => (List<DataPoint>)SSE.ItemsSource;

        private LineSeries PercentSuccess;
        private List<DataPoint> PercentSuccessData => (List<DataPoint>)PercentSuccess.ItemsSource;

        public StatsOverTimeModel() {
            Model = new PlotModel();
            Model.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, Title = "Batch", MinorStep = 1 });
            Model.Axes.Add(new LinearAxis() { Key = "SSE", Position = AxisPosition.Left, Title = "SSE", Minimum = 0, MajorStep = 1 });
            Model.Axes.Add(new LinearAxis() { Key = "Suc", Position = AxisPosition.Right, Title = "Success %", Minimum = -1, Maximum = 101, MinorStep = 1 });

            SSE = new LineSeries() { YAxisKey = "SSE", Title = "SSE" };
            SSE.ItemsSource = new List<DataPoint>();

            PercentSuccess = new LineSeries() { YAxisKey = "Suc", Title = "Success %" };
            PercentSuccess.ItemsSource = new List<DataPoint>();

            Model.Series.Add(SSE);
            Model.Series.Add(PercentSuccess);

            Model.LegendOrientation = LegendOrientation.Vertical;
            Model.LegendPlacement = LegendPlacement.Inside;
            Model.LegendPosition = LegendPosition.TopRight;
            Model.LegendBorder = OxyColors.Black;
            Model.LegendTitle = "Over time";
        }

        public void AddSSE(double sse, bool update = true) {
            SSEData.Add(new DataPoint(SSEData.Count - 1, sse));
            if(update) {
                Model.InvalidatePlot(true);
            }
        }

        public void AddSuccessPercent(double perc, bool update = true) {
            PercentSuccessData.Add(new DataPoint(PercentSuccessData.Count - 1, perc));
            if(update) {
                Model.InvalidatePlot(true);
            }
        }

        public void AddBoth(double sse, double perc) {
            AddSSE(sse, false);
            AddSuccessPercent(perc, false);
            Model.InvalidatePlot(true);
        }

        public PlotModel Model {
            get; private set;
        }
    }
}