using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Nodes {

    [DebuggerDisplay("Bias '{Name}' = [{Output}]")]
    public class Bias : Node {
        public override float Output => 1;

        public Bias(string name = "X") : base(name) {
        }
    }
}