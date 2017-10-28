using NeuralNet.TransferFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Base {
    public interface INetwork : IGetOutput {
        TransferFunction TransferFunction { get; }

        float[][][] Weights { get; }

        int LayerCount { get; }
        int HiddenLayerCount { get; }

        void FillNetwork(int nrInputs, int nrOutputs, params int[] hiddenLayerHeights);

        float[][] GetValuesForInput(params float[] input);

        int GetLayerHeight(int layerNr);
    }
}
