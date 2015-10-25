namespace NeuralNet {

    public struct InputExpectedResult {
        public readonly float[] Input;
        public readonly float[] Output;

        public InputExpectedResult(float[] input, float[] output) {
            Input = input;
            Output = output;
        }
    }
}