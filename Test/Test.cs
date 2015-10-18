using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNet;
using NeuralNet.Connections;
using NeuralNet.TransferFunctions;
using System;

namespace Test {

    [TestClass]
    public class Test {

        [TestMethod]
        public void TestSigmoid() {
            SigmoidFunction sf = new SigmoidFunction();

            Assert.IsTrue(sf.Calculate(10, 20) > 0.9);
            Assert.IsTrue(sf.Calculate(-10, -20) < 0.1);
            Assert.IsTrue(sf.Calculate(-1, 1) > 0.4 && sf.Calculate(-1, 1) < 0.6);
        }

        [TestMethod]
        public void TestSetValueConnection() {
            float testval = 10.3f;
            var svc = new SetValueConnection(testval);

            Assert.AreEqual(svc.Output, testval);
        }

        [TestMethod]
        public void TestWeightedConnection() {
            float weight = 2f;
            float input = 5f;
            var wc = new WeightedConnection(weight, () => input);

            Assert.AreEqual(wc.Output, weight * input);
        }
    }
}