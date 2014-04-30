namespace DataMining.Neural_Networks
{
    public class Connection
    {
        public Node From { get; private set; }
        public Node To { get; private set; }
        public double Weight{ get; private set; }

        public Connection(double weight, Node fromNode, Node toNode)
        {
            Weight = weight;
            From = fromNode;
            To = toNode;

            From.OutCon.Add(this);
            To.InCon.Add(this);
        }

        public void ChangeWeight(double change)
        {
            Weight += change;
        }

        public void UpdateWeight()
        {
            double d = Node.LearningRate*To.ErrorValue*From.OutputValue;
            ChangeWeight(d);
        }
    }
}