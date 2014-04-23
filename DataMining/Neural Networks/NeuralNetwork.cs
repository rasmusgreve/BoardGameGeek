using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataMining.Neural_Networks
{
    public class NeuralNetwork
    {
        public const bool DEBUG = false;

        private Node[] inputNodes, outputNodes;
        private Node[][] hiddenLayers;
        private List<Connection> connections;

        private readonly Random rand;

        public NeuralNetwork(int inputNum, int outputNum, int hiddenNum, int layerNum)
        {
            rand = new Random();

            connections = new List<Connection>();
            hiddenLayers = new Node[layerNum][];
            outputNodes = new Node[outputNum];
            inputNodes = new Node[inputNum];

            for (int i = 0; i < inputNum; i++)
            {
                inputNodes[i] = new Node(RandStart());
            }

            for (int i = 0; i < outputNum; i++)
            {
                outputNodes[i] = new Node(RandStart());
            }
            for (int i = 0; i < layerNum; i++)
            {
                hiddenLayers[i] = new Node[hiddenNum];
                for (int j = 0; j < hiddenNum; j++)
                {
                    Node curNode = new Node(RandStart());
                    hiddenLayers[i][j] = curNode;

                    //hook up connection
                    if (i == 0) //input layer to first hiddden layer
                    {
                        for (int k = 0; k < inputNum; k++)
                        {
                            connections.Add(new Connection(RandStart(), inputNodes[k], curNode));

                        }
                    }
                    else //hidden layer to hidden layer
                    {
                        for (int k = 0; k < inputNum; k++)
                        {
                            connections.Add(new Connection(RandStart(), hiddenLayers[i - 1][k], curNode));

                        }
                    }

                    //last hidden layer to output layer
                    if (i == layerNum - 1)
                    {
                        for (int k = 0; k < outputNum; k++)
                        {
                            connections.Add(new Connection(RandStart(), curNode, outputNodes[k]));
                        }
                    }
                }
            }
        }

        public double RandStart()
        {
            return (rand.NextDouble()*2)-1.0;
        }

        public double[] CalculateOutput(double[] inputVariables)
        {
            double[] result = new double[outputNodes.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = outputNodes[i].ForwardOperation();
            }
            return result;
        }

        public bool RunTraining(double[] trainingInput, double[] trainingOutput)
        {
            //set inputs
            for (int i = 0; i < inputNodes.Length; i++)
            {
                inputNodes[i].InputValue = trainingInput[i];
            }

            if(DEBUG) Console.WriteLine("Output=");
            bool match = true;
            for (int i = 0; i < outputNodes.Length; i++)
            {
                //forward operation
                double result = outputNodes[i].ForwardOperation();
                int rounded = (int) Math.Round(result);
                int target = (int) Math.Round(trainingOutput[i]);

                if (DEBUG) Console.WriteLine("\t{0}: {1} - Target={2}", i, result, trainingOutput[i]);
                if (DEBUG) Console.WriteLine("\t{0}(int): {1} - Target(int)={2}", i, rounded, target);

                match = match && (target == rounded);
            }

            if (DEBUG) Console.WriteLine("Result Match: {0}", match);

            // output error
            for (int i = 0; i < outputNodes.Length; i++)
            {
                double T = trainingOutput[i];
                outputNodes[i].ComputeError(T);
            }
            for (int i = hiddenLayers.Length-1; i >= 0; i--)
            {
                foreach (Node hiddenNode in hiddenLayers[i])
                {
                    hiddenNode.ComputeError(0);
                }
            }

            //update weights
            connections.ToList().ForEach(e => e.UpdateWeight());

            //update bias
            foreach (Node hNode in hiddenLayers.SelectMany(hLayer => hLayer))
            {
                hNode.UpdateBias();
            }
            outputNodes.ToList().ForEach(e => e.UpdateBias());

            if(DEBUG) Console.WriteLine(this);

            return match;
        }

        public int runSession(double[][] trainingInput, double[][] trainingOutput)
        {
            int numCorrect = 0;

            for (int i = 0; i < trainingInput.Length; i++)
            {
                bool match = RunTraining(trainingInput[i], trainingOutput[i]);
                if (match) numCorrect++;
                
            }
            if(DEBUG) Console.WriteLine("{0} out of {1} correct", numCorrect, trainingInput.Length);
            return numCorrect;
        }

        public override string ToString()
        {
            StringBuilder strB = new StringBuilder();
            strB.Append("Input Nodes:\n");
            inputNodes.ToList().ForEach(e => NodeToBuilder(e, strB));
        
            for (int i = 0; i < hiddenLayers.Length; i++)
            {
                strB.AppendFormat("\nLayer {0}:\n", i + 1);
                hiddenLayers[i].ToList().ForEach(e => NodeToBuilder(e,strB));
            }

            strB.Append("\nOutput Nodes\n");
            outputNodes.ToList().ForEach(e => NodeToBuilder(e,strB));

            strB.Append("\n");
            return strB.ToString();
        }

        private void NodeToBuilder(Node node, StringBuilder strB)
        {
            strB.AppendFormat("{0}: bias={1}\n",node, node.Bias);
            foreach (Connection conn in node.OutCon)
            {
                strB.AppendFormat("\t-> {0} = {1}\n", conn.To, conn.Weight);
            }
        }
    }
}
