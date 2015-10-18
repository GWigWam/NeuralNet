using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.TransferFunctions {

    internal abstract class TransferFunction {

        public void Calculate(params float[] input) => Calculate((IEnumerable<float>)input);

        public abstract void Calculate(IEnumerable<float> input);
    }
}