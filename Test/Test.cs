using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNet;
using NeuralNet.Connections;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [TestMethod]
        public void TestBasicPerceptron() {
            string name = "TestName123";
            List<Connection> connections = new List<Connection>() {
                new SetValueConnection(10),
                new SetValueConnection(20)
            };

            var p = new Perceptron(new SigmoidFunction(), connections, name);
            Assert.IsTrue(p.Output > 0.9);
            Assert.AreEqual(name, p.Name);
        }

        [TestMethod]
        public void TestPerceptronCaching() {
            float local1 = -10;
            float local2 = -5;
            var c1 = new WeightedConnection(1, () => local1);
            var c2 = new WeightedConnection(1.3f, () => local2);

            var p = new Perceptron(new SigmoidFunction(), new List<Connection>() { c1, c2 });
            Assert.IsTrue(p.Output < 0.1);

            local1 = 10;
            local2 = 5;

            Assert.IsTrue(p.Output < 0.1);

            p.ResetCache();

            Assert.IsFalse(p.Output < 0.1);
        }

        [TestMethod]
        public void TestFillNetwork() {
            int inputs = 2;
            int layer1 = 3;
            int layer2 = 4;
            int outputs = 1;

            var nw = new Network(new SigmoidFunction());
            nw.FillNetwork(inputs, outputs, layer1, layer2);

            Assert.IsTrue(nw.Perceptrons[0].Length == inputs);
            Assert.IsTrue(nw.Perceptrons[1].Length == layer1);
            Assert.IsTrue(nw.Perceptrons[2].Length == layer2);
            Assert.IsTrue(nw.Perceptrons[3].Length == outputs);

            Assert.IsTrue(nw.Perceptrons[3][0].Connections.Length == layer2);
            Assert.IsTrue(nw.Perceptrons[2][3].Connections.Length == layer1);
        }

        [TestMethod]
        public void TestNetworkOutput() {
            var rand = new Random();
            float input = (float)(rand.NextDouble() * 100);
            float weight = (float)(rand.NextDouble() * 2);

            var sigmoid = new SigmoidFunction();
            var nw = new Network(sigmoid);

            var inputCon = new SetValueConnection(input);
            var inputNode = new Perceptron(sigmoid, inputCon, "Input");
            var inpToOut = new WeightedConnection(weight, () => inputNode.Output);
            var outputNode = new Perceptron(sigmoid, inpToOut, "Output");

            nw.Perceptrons = new Perceptron[][] {
                new Perceptron[] { inputNode },
                new Perceptron[] { outputNode }
            };

            var nwOut = nw.CurOutput()[0];
            Assert.AreEqual(outputNode.Output, nwOut);

            var expOut = sigmoid.Calculate(sigmoid.Calculate(input) * weight);

            Assert.AreEqual(nwOut, expOut);
        }

        [TestMethod]
        public void TestGetInputResult() {
            var rand = new Random();
            float input = (float)(rand.NextDouble() * 100 + 1);
            float weight = (float)(rand.NextDouble() * 2);

            var sigmoid = new SigmoidFunction();
            var nw = new Network(sigmoid);

            var inputCon = new SetValueConnection(0);
            var inputNode = new Perceptron(sigmoid, inputCon, "Input");
            var inpToOut = new WeightedConnection(weight, () => inputNode.Output);
            var outputNode = new Perceptron(sigmoid, inpToOut, "Output");

            nw.Perceptrons = new Perceptron[][] {
                new Perceptron[] { inputNode },
                new Perceptron[] { outputNode }
            };

            var nwOut = nw.GetInputResult(input)[0];
            var expOut = sigmoid.Calculate(sigmoid.Calculate(input) * weight);

            Assert.AreEqual(nwOut, expOut);
        }
    }
}