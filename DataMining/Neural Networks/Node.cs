using System;
using System.Collections.Generic;
using System.Linq;

namespace DataMining.Neural_Networks
{
    public class Node
    {
        public const double LearningRate = 5.0;

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
            
            Bias = initialBias;

            InCon = new List<Connection>();
            OutCon = new List<Connection>();
        }

        public double ForwardOperation()
        {
            if (!InCon.Any())
            {
                OutputValue = InputValue;
                return InputValue;
            }

            double S = Bias + InCon.Sum(conn => conn.From.ForwardOperation()*conn.Weight);

            double A = 1.0/(1.0 + Math.Pow(Math.E, -1.0 * S));
            OutputValue = A;
            return A;
        }

        public void ComputeError(double T)
        {
            double O = OutputValue;

            if (!OutCon.Any())
            {
                ErrorValue = O*(1 - O)*(T - O);
                return;
            }

            //layer mode
            double nextSum = OutCon.Sum(conn => conn.To.ErrorValue*conn.Weight);
            ErrorValue = O*(1 - O)*nextSum;
        }

        public void UpdateBias()
        {
            Bias = Bias + (LearningRate*ErrorValue);
        }
    }
}