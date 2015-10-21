using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Nodes {

    [DebuggerDisplay("Input '{Name}' = [{Output}]")]
    public class Input : INode {

        public float Value {
            get; set;
        }

        public string Name {
            get;
        }

        public float Output => Value;

        public Input(float initValue = 0, string name = "Input") {
            Name = name;
            Value = initValue;
        }
    }
}