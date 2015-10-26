using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Nodes {

    [DebuggerDisplay("Input '{Name}' = [{Output}]")]
    public class Input : Node {

        public double Value {
            get; set;
        }

        public override double Output => Value;

        public Input(double initValue = 0, string name = "X") : base(name) {
            Value = initValue;
        }
    }
}