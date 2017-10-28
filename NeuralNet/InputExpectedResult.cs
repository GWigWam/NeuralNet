namespace NeuralNet {

    public struct InputExpectedResult {
        public readonly float[] Input;
        public readonly float[] ExpectedOutput;

        public InputExpectedResult(float[] input, float[] output) {
            Input = input;
            ExpectedOutput = output;
        }
    }
}