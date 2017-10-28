using NeuralNet.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet {

    public static class NetworkValidation {

        /// <param name="checkSuccess">Funcion takes 'expected', 'actual' returns bool wether or not actual can be considerd a success</param>
        public static ValidationResult Validate(IGetOutput network, IEnumerable<InputExpectedResult> inputResults, Func<float[], float[], bool> checkSuccess) {
            ValidationResult totalResult = null;
            foreach(var cur in inputResults) {
                float[] actual = network.GetOutputForInput(cur.Input);

                float sse = SumSquaredError(cur.ExpectedOutput, actual);
                bool success = checkSuccess(cur.ExpectedOutput, actual);

                var res = new ValidationResult(sse, success);

                totalResult += res;
            }

            return totalResult;
        }

        public static float SquaredError(float target, float actual) {
            var val = Math.Pow(target - actual, 2);
            return (float)val;
        }

        public static float SumSquaredError(float[] target, float[] actual) {
            float sum = 0;
            for(int i = 0; i < target.Length; i++) {
                var curErr = SquaredError(target[i], actual[i]);
                sum += curErr;
            }

            return sum;
        }
    }

    public class ValidationResult {
        private List<float> SSEs;

        public float AvgSSE => SSEs.Count > 0 ? SSEs.Average() : -1;

        public int EntryCount => SSEs.Count;

        public int Successes {
            get; private set;
        }

        public double SuccessPercentage => 100 * Successes / EntryCount;

        private ValidationResult() {
            SSEs = new List<float>();
        }

        public ValidationResult(float sse, bool isSuccess) : this() {
            SSEs.Add(sse);

            if(isSuccess) {
                Successes++;
            }
        }

        public float[] GetSSEs() {
            return SSEs.ToArray();
        }

        public static ValidationResult operator +(ValidationResult v1, ValidationResult v2) {
            var retVal = new ValidationResult();

            if(v1 != null) {
                retVal.SSEs.AddRange(v1.SSEs);
                retVal.Successes += v1.Successes;
            }
            if(v2 != null) {
                retVal.SSEs.AddRange(v2.SSEs);
                retVal.Successes += v2.Successes;
            }

            return retVal;
        }

        public override string ToString() {
            return $"Avg SSE: {AvgSSE:N5} | {SuccessPercentage:N1}% Correct";
        }
    }
}