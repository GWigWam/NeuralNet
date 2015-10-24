using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNet;
using NeuralNet.Connections;
using NeuralNet.Nodes;
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
        public void TestConnection() {
            float weight = 2f;
            float input = 5f;
            var inp = new Input(input);
            var outp = new Perceptron(new SigmoidFunction(), "Output");
            var wc = Connection.Create(weight, inp, outp);

            Assert.AreEqual(wc.Output, weight * input);
            Assert.AreEqual(inp.GetOutgoingConnections()[0], outp.GetIncommingConnections()[0]);
        }

        [TestMethod]
        public void TestPerceptronCaching() {
            float local1 = -10;
            float local2 = -5;

            var in1 = new Input(local1, "Input1");
            var in2 = new Input(local2, "Input2");
            var p = new Perceptron(new SigmoidFunction());

            Connection.Create(0.5f, in1, p);
            Connection.Create(1f, in2, p);

            Assert.IsTrue(p.Output < 0.1);

            in1.Value = 10;
            in2.Value = 5;

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

            var nw = new Network(new SigmoidFunction(), true);
            nw.FillNetwork(inputs, outputs, layer1, layer2);

            Assert.IsTrue(nw.Nodes[0].Length == inputs);
            Assert.IsTrue(nw.Nodes[1].Length == layer1);
            Assert.IsTrue(nw.Nodes[2].Length == layer2);
            Assert.IsTrue(nw.Nodes[3].Length == outputs);

            Assert.IsTrue(((Perceptron)nw.Nodes[3][0]).GetIncommingConnections().Length == layer2 + 1); // +1 for bias node
            Assert.IsTrue(((Perceptron)nw.Nodes[2][3]).GetIncommingConnections().Length == layer1 + 1); // +1 for bias node
        }

        [TestMethod]
        public void TestNetworkOutput() {
            var rand = new Random();
            float input = (float)(rand.NextDouble());
            float weight = (float)(rand.NextDouble());

            var sigmoid = new SigmoidFunction();
            var nw = new Network(sigmoid, false);

            var inputNode = new Input(input);
            var outputNode = new Perceptron(sigmoid, "Output");
            Connection.Create(weight, inputNode, outputNode);

            nw.Nodes = new Node[][] {
                new Node[] { inputNode },
                new Node[] { outputNode }
            };

            var nwOut = nw.CurOutput()[0];
            Assert.AreEqual(outputNode.Output, nwOut);

            var expOut = sigmoid.Calculate(input * weight);

            Assert.AreEqual(nwOut, expOut);
        }

        [TestMethod]
        public void TestGetInputResult() {
            var rand = new Random();
            float input = (float)(rand.NextDouble() * 100 + 1);
            float weight = (float)(rand.NextDouble() * 2);

            var sigmoid = new SigmoidFunction();
            var nw = new Network(sigmoid, false);

            var inputNode = new Input(input);
            var outputNode = new Perceptron(sigmoid, "Output");
            Connection.Create(weight, inputNode, outputNode);

            nw.Nodes = new Node[][] {
                new Node[] { inputNode },
                new Node[] { outputNode }
            };

            var nwOut = nw.GetInputResult(input)[0];

            //Output
            // = Sig(inpToOut.Output)
            // = Sig(inpToOut.Weight * inpToOut.GetInput())
            // = Sig(inpToOut.Weight * inputNode.Output)
            // = Sig(this.weight * this.input)
            var expOut = sigmoid.Calculate(weight * input);

            Assert.AreEqual(nwOut, expOut);
        }

        [TestMethod]
        public void TestBias() {
            float input = 0.3f;
            float inpToOutWeight = 0.4f;
            float biasToOutWeight = 0.5f;

            var sigmoid = new SigmoidFunction();
            var nw = new Network(sigmoid, false);

            var inputNode = new Input(input);
            var bias = new Bias();
            var outputNode = new Perceptron(sigmoid, "Output");
            Connection.Create(inpToOutWeight, inputNode, outputNode);
            Connection.Create(biasToOutWeight, bias, outputNode);

            nw.Nodes = new Node[][] {
                new Node[] { inputNode },
                new Node[] { outputNode }
            };

            var nwOut = nw.Nodes[1][0].Output;

            //Output
            // = Sig(inpToOut.Output + biasToOut.Output)
            // = Sig((inpToOut.Weight * inpToOut.GetInput()) + (biasToOutWeight * 1))
            // = Sig((inpToOut.Weight * inputNode.Output) + (biasToOutWeight))
            // = Sig((this.weight * this.input) + (biasToOutWeight))
            var expOut = sigmoid.Calculate((inpToOutWeight * input) + biasToOutWeight);

            Assert.AreEqual(nwOut, expOut);
        }

        [TestMethod]
        public void TestDelConnection() {
            var inp = new Input(5);
            var outp = new Perceptron(new SigmoidFunction());

            var beforeConnect = outp.Output;

            var con = Connection.Create(1, inp, outp);

            outp.ResetCache();
            var afterConnect = outp.Output;

            Assert.AreNotEqual(beforeConnect, afterConnect);

            con.Delete();
            outp.ResetCache();

            Assert.AreEqual(beforeConnect, outp.Output);
        }
    }
}