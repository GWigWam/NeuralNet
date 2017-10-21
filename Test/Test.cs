using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNet;
using NeuralNet.BackpropagationTraining;
using NeuralNet.Connections;
using NeuralNet.Nodes;
using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            double weight = 2;
            double input = 5;
            var inp = new Input(input);
            var outp = new Perceptron(new SigmoidFunction(), "Output");
            var wc = Connection.Create(weight, inp, outp);

            Assert.AreEqual(wc.Output, weight * input);
            Assert.AreEqual(inp.GetOutgoingConnections()[0], outp.GetIncommingConnections()[0]);
        }

        [TestMethod]
        public void TestPerceptronCaching() {
            double local1 = -10;
            double local2 = -5;

            var in1 = new Input(local1, "Input1");
            var in2 = new Input(local2, "Input2");
            var p = new Perceptron(new SigmoidFunction());

            Connection.Create(0.5, in1, p);
            Connection.Create(1, in2, p);

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

            Assert.IsTrue(((Perceptron)nw.Nodes[3][0]).GetIncommingConnections().Count == layer2 + 1); // +1 for bias node
            Assert.IsTrue(((Perceptron)nw.Nodes[2][3]).GetIncommingConnections().Count == layer1 + 1); // +1 for bias node
        }

        [TestMethod]
        public void TestNetworkOutput() {
            var rand = new Random();
            double input = (double)(rand.NextDouble());
            double weight = (double)(rand.NextDouble());

            var sigmoid = new SigmoidFunction();
            var nw = new Network(sigmoid, false);

            var inputNode = new Input(input);
            var outputNode = new Perceptron(sigmoid, "Output");
            Connection.Create(weight, inputNode, outputNode);

            nw.Nodes = new Node[][] {
                new Node[] { inputNode },
                new Node[] { outputNode }
            };

            var nwOut = nw.GetOutput()[0];
            Assert.AreEqual(outputNode.Output, nwOut);

            var expOut = sigmoid.Calculate(input * weight);

            Assert.AreEqual(nwOut, expOut);
        }

        [TestMethod]
        public void TestGetInputResult() {
            var rand = new Random();
            double input = (double)(rand.NextDouble() * 100 + 1);
            double weight = (double)(rand.NextDouble() * 2);

            var sigmoid = new SigmoidFunction();
            var nw = new Network(sigmoid, false);

            var inputNode = new Input(input);
            var outputNode = new Perceptron(sigmoid, "Output");
            Connection.Create(weight, inputNode, outputNode);

            nw.Nodes = new Node[][] {
                new Node[] { inputNode },
                new Node[] { outputNode }
            };

            var nwOut = nw.GetOutputForInput(input)[0];

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
            double input = 0.3;
            double inpToOutWeight = 0.4;
            double biasToOutWeight = 0.5;

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

        [TestMethod]
        public void TestTraining() {
            var sigmoid = new SigmoidFunction();

            var net = new Network(sigmoid, true);
            net.FillNetwork(2, 2, 2);

            net.Nodes[0][0].GetOutgoingConnections()[0].Weight = .15;
            net.Nodes[0][0].GetOutgoingConnections()[1].Weight = .2;
            net.Nodes[0][1].GetOutgoingConnections()[0].Weight = .25;
            net.Nodes[0][1].GetOutgoingConnections()[1].Weight = .3;
            net.Nodes[1][0].GetOutgoingConnections()[0].Weight = .4;
            net.Nodes[1][0].GetOutgoingConnections()[1].Weight = .45;
            net.Nodes[1][1].GetOutgoingConnections()[0].Weight = .5;
            net.Nodes[1][1].GetOutgoingConnections()[1].Weight = .55;

            var expected = new InputExpectedResult(new double[] { .05, .1 }, new double[] { .01, .99 });

            var before = NetworkValidation.Validate(net, new InputExpectedResult[] { expected }, (a, b) => true);

            var bp = new Backpropagate(net, 0.5);
            bp.Train(new InputExpectedResult[] { expected });

            var after = NetworkValidation.Validate(net, new InputExpectedResult[] { expected }, (a, b) => true);

            Assert.IsTrue(before.AvgSSE > after.AvgSSE);
        }

        [TestMethod]
        public void TestTraining2() {
            var sigmoid = new SigmoidFunction();

            var net = new Network(sigmoid, true);
            net.FillNetwork(2, 2, 2);

            net.Nodes[0][0].GetOutgoingConnections()[0].Weight = .15;
            net.Nodes[0][0].GetOutgoingConnections()[1].Weight = .25;
            net.Nodes[0][1].GetOutgoingConnections()[0].Weight = .20;
            net.Nodes[0][1].GetOutgoingConnections()[1].Weight = .30;
            net.Nodes[1][0].GetOutgoingConnections()[0].Weight = .40;
            net.Nodes[1][0].GetOutgoingConnections()[1].Weight = .50;
            net.Nodes[1][1].GetOutgoingConnections()[0].Weight = .45;
            net.Nodes[1][1].GetOutgoingConnections()[1].Weight = .55;

            var biasOut = net.Bias.GetOutgoingConnections();
            biasOut[0].Weight = .35; // Bias --> H0.0
            biasOut[1].Weight = .35; // Bias --> H0.1
            biasOut[2].Weight = .60; // Bias --> O.0
            biasOut[3].Weight = .60; // Bias --> O.1

            var expected = new InputExpectedResult(new double[] { .05, .1 }, new double[] { .01, .99 });

            var before = NetworkValidation.Validate(net, new InputExpectedResult[] { expected }, (a, b) => true);

            var bp = new Backpropagate2(net, 0.5);
            bp.Train(new InputExpectedResult[] { expected });

            var after = NetworkValidation.Validate(net, new InputExpectedResult[] { expected }, (a, b) => true);

            Assert.IsTrue(before.AvgSSE > after.AvgSSE);
        }

        [TestMethod]
        public void TestParallelLogging() {
            Task t1 = new Task(() => {
                PerformanceLog.ResetCurTimer();
                Task.Delay(1000).Wait();
                PerformanceLog.LogProcess("1");
                Task.Delay(300).Wait();
                PerformanceLog.LogProcess("2");
            });

            Task t2 = new Task(() => {
                PerformanceLog.ResetCurTimer();
                Task.Delay(1100).Wait();
                PerformanceLog.LogProcess("1");
            });

            t1.Start();
            t2.Start();

            t1.Wait();
            t2.Wait();

            Assert.IsTrue(PerformanceLog.Processes["1"] >= 2100);
            Assert.IsTrue(PerformanceLog.Processes["2"] >= 300);
        }
    }
}