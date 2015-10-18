using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    internal abstract class TransferFunction {

        public float Calculate(params float[] input) => Calculate((IEnumerable<float>)input);

        public abstract float Calculate(IEnumerable<float> input);
    }
}