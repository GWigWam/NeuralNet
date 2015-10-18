using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNet.Connections {

    internal class SetValueConnection : Connection {

        public float Value {
            get; set;
        }

        public float Output => Value;

        public SetValueConnection(float value) {
            Value = value;
        }
    }
}