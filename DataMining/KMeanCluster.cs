using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataMiningIndividual
{
    /// <summary>
    /// A single cluster for the KMeans Partitioning algorithm. It contains all
    /// DataLine members of the cluster and a centroid of the cluster.
    /// </summary>
    class KMeanCluster
    {
        public Dictionary<string, double> Centroid { get; private set; }
        private List<DataLine> members;

        // Makes a DataLine into a Centroid
        private static Dictionary<string, double> Convert(DataLine data)
        {
            Dictionary<string, double> centroid = new Dictionary<string, double>();
            foreach (string key in data.hashDoubles.Keys)
            {
                centroid[key] = data.hashDoubles[key] ?? 0.0; // Null == 0.0 (might not be good)
            }
            return centroid;
        }

        /// <summary>
        /// Creates a new KMeanCluster with the given centroid used for measuring
        /// distances to later DataLines
        /// </summary>
        /// <param name="centroid">The centroid that should be the center of the cluster.</param>
        public KMeanCluster(Dictionary<string,double> centroid)
        {
            this.Centroid = centroid;
            members = new List<DataLine>();
        }

        public KMeanCluster(DataLine answer) : this(Convert(answer)) {}

        /// <summary>
        /// Calculates a new centroid from the members of the current cluster
        /// and returns a new cluster with that centroid.
        /// </summary>
        /// <returns>The new empty cluster with the calculated centroid.</returns>
        public KMeanCluster CalcCentroid()
        {
            if (members.Count == 0)
            {
                Console.WriteLine("Warning: No members in cluster");
                return this;
            }

            Dictionary<string, double> newCentroid = new Dictionary<string, double>();

            members[0].hashDoubles.Keys.ForEach((k,i) => newCentroid[k] = 
                members.Where(a => a.hashDoubles[k] != null)
                .Sum(a => (double)a.hashDoubles[k]) / members.Count);

            return new KMeanCluster(newCentroid);
        }

        /// <summary>
        /// Adds the given DataLines as a member of this cluster.
        /// </summary>
        /// <param name="a">The DataLine to add.</param>
        public void AddMember(DataLine a)
        {
            this.members.Add(a);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (DataLine a in members) builder.Append(a.ToString() + "\n");
            return builder.ToString();
        }
    }
}
