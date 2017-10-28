using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    public abstract class TransferFunction {

        public abstract float ExtremeMax {
            get;
        }

        public abstract float ExtremeMin {
            get;
        }

        public float Calculate(params float[] input) => Calculate((IEnumerable<float>)input);

        public float Calculate(IEnumerable<float> input) => Calculate(input.Sum());

        public abstract float Calculate(float input);

        public abstract float Derivative(float input);
    }
}