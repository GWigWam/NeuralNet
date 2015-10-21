namespace NeuralNet.Nodes {

    public interface INode {

        string Name {
            get;
        }

        float Output {
            get;
        }
    }
}