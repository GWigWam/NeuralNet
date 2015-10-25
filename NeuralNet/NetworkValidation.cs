using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class NetworkValidation {

        /// <param name="checkSuccess">Funcion takes 'expected', 'actual' returns bool wether or not actual can be considerd a success</param>
        public static ValidationResult Validate(Network network, IEnumerable<InputExpectedResult> inputResults, Func<float[], float[], bool> checkSuccess) {
            ValidationResult totalResult = null;
            foreach(var cur in inputResults) {
                float[] actual = network.GetInputResult(cur.Input);

                float sse = SumSquaredError(cur.Output, actual);
                float certainty = Certainty(cur.Output, actual);
                bool success = checkSuccess(cur.Output, actual);

                var res = new ValidationResult(sse, certainty, success);

                totalResult += res;
            }

            return totalResult;
        }

        public static float Certainty(float[] target, float[] actual) {
            double dif = 0;
            for(int i = 0; i < target.Length; i++) {
                dif += Math.Abs(target[i] - actual[i]);
            }
            return 1f - (float)(dif / target.Length);
        }

        public static float SquaredError(float target, float actual) {
            var val = Math.Pow(target - actual, 2);
            return (float)val;
        }

        public static float SumSquaredError(float[] target, float[] actual) {
            float sum = 0;
            for(int i = 0; i < target.Length; i++) {
                float curErr = SquaredError(target[i], actual[i]);
                sum += curErr;
            }

            return sum;
        }
    }

    public class ValidationResult {
        private List<float> SSEs;
        private List<float> Certainties;

        public float AvgSSE => SSEs.Count > 0 ? SSEs.Average() : -1;
        public float AvgCertainty => Certainties.Count > 0 ? Certainties.Average() : -1;

        public int EntryCount => SSEs.Count;

        public int Successes {
            get; private set;
        }

        public float SuccessPercentage => 100f * Successes / EntryCount;

        private ValidationResult() {
            SSEs = new List<float>();
            Certainties = new List<float>();
        }

        public ValidationResult(float sse, float certainty, bool isSuccess) : this() {
            SSEs.Add(sse);
            Certainties.Add(certainty);

            if(isSuccess) {
                Successes++;
            }
        }

        public float[] GetSSEs() {
            return SSEs.ToArray();
        }

        public float[] GetCertainties() {
            return Certainties.ToArray();
        }

        public static ValidationResult operator +(ValidationResult v1, ValidationResult v2) {
            var retVal = new ValidationResult();

            if(v1 != null) {
                retVal.SSEs.AddRange(v1.SSEs);
                retVal.Certainties.AddRange(v1.Certainties);
                retVal.Successes += v1.Successes;
            }
            if(v2 != null) {
                retVal.SSEs.AddRange(v2.SSEs);
                retVal.Certainties.AddRange(v2.Certainties);
                retVal.Successes += v2.Successes;
            }

            return retVal;
        }

        public override string ToString() {
            return $"Avg SSE: {AvgSSE:N3} | Avg certainty: {AvgCertainty:N3} | {SuccessPercentage:N1}% Correct";
        }
    }
}