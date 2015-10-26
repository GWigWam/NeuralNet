namespace NeuralNet {

    public struct InputExpectedResult {
        public readonly double[] Input;
        public readonly double[] Output;

        public InputExpectedResult(double[] input, double[] output) {
            Input = input;
            Output = output;
        }
    }
}