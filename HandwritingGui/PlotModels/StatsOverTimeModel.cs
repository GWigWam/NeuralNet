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
        private LineSeries PercentSuccess;

        public StatsOverTimeModel() {
            Model = new PlotModel();
            Model.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, Title = "Batch", MinorStep = 1 });
            Model.Axes.Add(new LinearAxis() { Key = "SSE", Position = AxisPosition.Left, Title = "SSE" });
            Model.Axes.Add(new LinearAxis() { Key = "Suc", Position = AxisPosition.Right, Title = "Success %", Minimum = 0, Maximum = 100, MinorStep = 1 });

            SSE = new LineSeries() { YAxisKey = "SSE" };
            SSE.ItemsSource = new List<DataPoint>();

            PercentSuccess = new LineSeries() { YAxisKey = "Suc" };
            PercentSuccess.ItemsSource = new List<DataPoint>();

            Model.Series.Add(SSE);
            Model.Series.Add(PercentSuccess);
        }

        public PlotModel Model {
            get; private set;
        }
    }
}