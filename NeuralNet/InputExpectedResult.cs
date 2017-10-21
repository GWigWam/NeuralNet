namespace NeuralNet {

    public struct InputExpectedResult {
        public readonly double[] Input;
        public readonly double[] ExpectedOutput;

        public InputExpectedResult(double[] input, double[] output) {
            Input = input;
            ExpectedOutput = output;
        }
    }
}