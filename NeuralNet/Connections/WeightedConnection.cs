using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Connections {

    [DebuggerDisplay("WeightedConnection out: {Output}")]
    public class WeightedConnection : Connection {

        public float Weight {
            get; set;
        }

        public Func<float> GetInput {
            get;
        }

        public float Output => Weight * GetInput();

        public WeightedConnection(float weight, Func<float> getInput) {
            if(getInput == null) {
                throw new ArgumentException($"{nameof(getInput)} must not be null", nameof(getInput));
            }
            GetInput = getInput;
            Weight = weight;
        }
    }
}