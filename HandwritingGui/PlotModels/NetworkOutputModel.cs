using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandwritingGui.PlotModels {

    public class NetworkOutputModel {

        public PlotModel Model {
            get; private set;
        }

        public NetworkOutputModel(double[] output, double min, double max) {
            Model = new PlotModel();
            var catAxis = new CategoryAxis() { Position = AxisPosition.Bottom };
            catAxis.LabelField = "Name";
            catAxis.ItemsSource = Enumerable.Range(0, output.Length).Select(i => new { Name = i.ToString() });
            Model.Axes.Add(catAxis);
            Model.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Minimum = min, Maximum = max });

            var highest = output.Max();
            var data = new List<ColumnItem>();
            for(int i = 0; i < output.Length; i++) {
                var val = output[i];
                var colm = new ColumnItem(val);
                if(val == highest) {
                    colm.Color = OxyColors.Green;
                } else {
                    colm.Color = OxyColors.Red;
                }
                data.Add(colm);
            }

            Model.Series.Add(new ColumnSeries() { ItemsSource = data });
        }
    }
}