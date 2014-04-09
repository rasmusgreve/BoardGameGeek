namespace DataMining.Neural_Networks
{
    public class Connection
    {
        public Node From { get; private set; }
        public Node To { get; private set; }
        public double Weight{ get; private set; }

        public Connection(double init, Node fromNode, Node toNode)
        {
            Weight = init;
            From = fromNode;
            To = toNode;
        }

        public void ChangeWeight(double change)
        {
            Weight += change;
            if (Weight < -1) Weight = -1;
            if (Weight > 1) Weight = 1;
        }

        public void UpdateWeight()
        {
            double d = Node.LearningRate*To.ErrorValue*From.OutputValue;
            ChangeWeight(d);
        }
    }
}