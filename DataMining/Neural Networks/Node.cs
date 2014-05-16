using System;
using System.Collections.Generic;
using System.Linq;

namespace DataMining.Neural_Networks
{
    public class Node
    {
        public const double LearningRate = 0.5;

        public List<Connection> InCon, OutCon;

        public double InputValue { private get;  set; }
        public double OutputValue { get; private set; }
        public double ErrorValue { get; private set; }
        public double Bias { get; private set; }


        public Node(double initialBias)
        {
            InputValue = 0.0;
            OutputValue = 0.0;
            ErrorValue = 0.0;
            Bias = 1.0;
            
            Bias = initialBias;

            InCon = new List<Connection>();
            OutCon = new List<Connection>();
        }

        public double ForwardOperation()
        {
            if (InCon.Count == 0) // input node
            {
                OutputValue = InputValue;
                return InputValue;
            }

            //Console.WriteLine("Forward Op: " + this.GetHashCode());

            double S = Bias; //+ InCon.Sum(conn => conn.From.ForwardOperation()*conn.Weight);
            foreach (Connection c in InCon)
            {
                double value = c.From.ForwardOperation() * c.Weight;
                //Console.WriteLine("\t From: " + conn.From.GetHashCode() + " = "+value);
                S += value;
            }

            double A = 1.0/(1.0 + Math.Pow(Math.E, -1.0 * S));
            OutputValue = A;
            //Console.WriteLine("\t A = " + A);
            return A;
        }

        public void ComputeError(double T)
        {
            double O = OutputValue;

            if (OutCon.Count == 0) // output value
            {
                ErrorValue = O*(1.0 - O) * (T - O);
                return;
            }

            //layer mode
            double nextSum = OutCon.Sum(c => c.To.ErrorValue * c.Weight);
            ErrorValue = O * (1.0 - O) * nextSum;
        }

        public void UpdateBias()
        {
            Bias = Bias + (Node.LearningRate*ErrorValue);
        }
    }
}