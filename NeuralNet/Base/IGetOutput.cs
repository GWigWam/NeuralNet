using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Base {
    public interface IGetOutput {
        float[] GetOutputForInput(params float[] input);
    }
}
