using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Nodes {

    [DebuggerDisplay("Bias '{Name}' = [{Output}]")]
    public class Bias : INode {

        public string Name {
            get;
        }

        public float Output => 1;

        public Bias(string name = "Bias") {
            Name = name;
        }
    }
}