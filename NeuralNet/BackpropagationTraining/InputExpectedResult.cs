using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.BackpropagationTraining {

    public struct InputExpectedResult {
        public readonly float[] Input;
        public readonly float[] Output;

        public InputExpectedResult(float[] input, float[] output) {
            Input = input;
            Output = output;
        }
    }
}