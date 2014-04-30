using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMiningIndividual;
using DataMining.Neural_Networks;

namespace DataMining
{
    /// <summary>
    /// A static library for performing Data Mining operations on Collections of
    /// parsed DataLines.
    /// </summary>
    class DataMining
    {
        private static readonly float KMEANS_MINIMUM = float.Epsilon;

        /// <summary>
        /// Normalizes all numeric values to a range between 0.0 and 1.0 depending on the
        /// maximum and minimum values in the list.
        /// </summary>
        /// <param name="data">A list of DataLine entries.</param>
        public static void minMaxNormalize(List<DataLine> data)
        {
            // normalize integers
            List<string> keys = data[0].hashDoubles.Keys.ToList();

            foreach (string attr in keys)
            {
                double min = data.Where(a => a.hashDoubles[attr] != null).Select(a => a.hashDoubles[attr].Value).Min();
                double max = data.Where(a => a.hashDoubles[attr] != null).Select(a => a.hashDoubles[attr].Value).Max();

                Console.WriteLine("Normalizing " + attr + ": min=" + min + " max=" + max);

                foreach (DataLine a in data.Where(a => a.hashDoubles[attr] != null))
                {
                    double oldVal = (double)a.hashDoubles[attr];
                    double newVal = ((oldVal - min) / (max - min));
                    a.hashDoubles[attr] = newVal;
                }
            }
        }

        /// <summary>
        /// Performs kNN classification on a new DataLine that wants to be classified.
        /// </summary>
        /// <param name="data">The entire dataSet that the classification will be based on.</param>
        /// <param name="newData">The new data entry that should be classfied.</param>
        /// <param name="stringLabel">The string parameter that should be labeled.</param>
        /// <param name="k">The amount of nearby entries to base the classification on.</param>
        /// <returns>The classified value for the newData entry. This entry must 
        /// be overwritten in the newData object if wanted.</returns>
        public static string kNN(List<DataLine> data, DataLine newData, string stringLabel, int k)
        {
            data = new List<DataLine>(data);

            // calc distances
            Dictionary<DataLine, double> distances = new Dictionary<DataLine, double>();
            foreach (DataLine a in data)
            {
                distances[a] = CalcDistance(a, newData, stringLabel);
                //Console.WriteLine("Distance: " + distances[a]);
            }

            // find k closest
            List<DataLine> kChosen = data.OrderBy(a => distances[a]).Take(k).ToList();

            // check what is most common
            string common = kChosen.GroupBy(a => a.hashStrings[stringLabel])
                .OrderByDescending(g => g.Count())
                .First().First()
                .hashStrings[stringLabel];


            // profit
            return common;
        }

        /// <summary>
        /// Performs KMeans partitioning (clustering) on the data. This is done on all
        /// parameters within the DataLines except for the dates, since they are not
        /// easily normalized (without loosing the meaning).
        /// </summary>
        /// <param name="k">The number of partitions to use.</param>
        /// <param name="data">The dataset that needs partitioning.</param>
        /// <returns>A list of KMeanCluster together containing all the entries from
        /// the original dataset.</returns>
        public static List<KMeanCluster> KMeansPartition(int k, List<DataLine> data)
        {
            // select k random
            KMeanCluster[] clusters = new KMeanCluster[k];

            data.Shuffle(1337);
            for (int i = 0; i < k; i++)
            {
                clusters[i] = new KMeanCluster(data[i]);
            }


            int iteration = 0;
            while (iteration < 10000)
            { // upper cutoff
                // beregn alle iris' til de k means
                data.ForEach(a => clusters.OrderBy(c => Dissimilarity(a, c)).First().AddMember(a));

                // beregn nye centroids
                KMeanCluster[] newClusters = new KMeanCluster[k];
                for (int c = 0; c < k; c++)
                {
                    newClusters[c] = clusters[c].CalcCentroid();
                }

                // stop hvis ingen ændringer
                if (Changed(clusters, newClusters))
                {
                    clusters = newClusters;
                }
                else
                {
                    break;
                }

                iteration++;
                Console.WriteLine("Iteration " + iteration + " done.");
            }

            return clusters.ToList();
        }

        /// <summary>
        /// Performs Apriori on the given dataset at a specific string array label in order
        /// to find frequent patterns of values.
        /// </summary>
        /// <param name="data">The dataset to look for frequent patterns in.</param>
        /// <param name="supportThreshold">The minimum number of occurences to call a pattern frequent.</param>
        /// <param name="stringArrayLabel">An array of labels specifying which parameters to look for patterns in. 
        /// MUST exist in either hashStringArrays or hashStrings on the DataLine.</param>
        /// <returns>A list of patterns (lists of strings) who comply with the support threshold.</returns>
        public static List<Tuple<List<string>, int>> Apriori(List<DataLine> data, int supportThreshold,
                                                             string[] stringArrayLabel)
        {
            const string aPrioriLabel = "aPrioriLabel";
            data.ForEach(line =>
                {
                    // Create a new array containing all values of the given labels 
                    var arr = new List<string>();
                    stringArrayLabel.ForEach(label =>
                        {
                            //Find the label in either hashStringArrays or hashStrings
                            if (line.hashStringArrays.ContainsKey(label))
                            {
                                if (line.hashStringArrays[label] != null)
                                    arr.AddRange(line.hashStringArrays[label]);
                            }
                            else
                            {
                                if (line.hashStrings[label] != null)
                                    arr.Add(line.hashStrings[label]);
                            }
                        });
                    line.hashStringArrays[aPrioriLabel] = arr.ToArray();
                } );
            
            // Run usual Apriori
            var ret = Apriori(data, supportThreshold, aPrioriLabel);

            // Cleanup: Delete the generated line
            //data.ForEach(line => line.hashStringArrays.Remove(aPrioriLabel));

            return ret;
        }

        /// <summary>
        /// Performs Apriori on the given dataset at a specific string array label in order
        /// to find frequent patterns of values.
        /// Runs in O(n^2 + k*n) time, n = <see cref="data"/>.Count, k = length of largest frequent item set.
        /// </summary>
        /// <param name="data">The dataset to look for frequent patterns in.</param>
        /// <param name="supportThreshold">The minimum number of occurences to call a pattern frequent.</param>
        /// <param name="stringArrayLabel">The parameter to look for patterns in.</param>
        /// <returns>A list of patterns (lists of strings) who comply with the support threshold.</returns>
        public static List<Tuple<List<string>, int>> Apriori(List<DataLine> data, int supportThreshold, string stringArrayLabel)
        {
            Console.WriteLine("Sorting items");
            data.ForEach(d => Array.Sort(d.hashStringArrays[stringArrayLabel]));

            int k;
            Console.WriteLine("Finding frequent itemsets of length 1");
            Dictionary<List<string>, int> frequentItemSets = GenerateFrequentItemSetsLevel1(data, stringArrayLabel, supportThreshold); // O(n^2)   <------------------
            List<Tuple<List<string>, int>> result = new List<Tuple<List<string>, int>>(frequentItemSets.Select(kv => new Tuple<List<string>, int>(kv.Key, kv.Value))); // O(f), f = frequentItemSet.Count
            for (k = 1; frequentItemSets.Count > 0; k++) // O(k)   <------------------
            {
                Console.WriteLine("Finding frequent itemsets of length " + (k + 1));
                frequentItemSets = GenerateFrequentItemSets(supportThreshold, data, stringArrayLabel, frequentItemSets); // O(n)   <------------------
                result.AddRange(frequentItemSets.Select(kv => new Tuple<List<string>, int>(kv.Key, kv.Value))); // O(f), f = frequentItemSet.Count

                Console.WriteLine(" found " + frequentItemSets.Count);
            }

            return result;
        }

        /// <summary>
        /// A separation of the Assocation Rule part of the Apriori algorithm.
        /// Association based on the frequent item sets of apriori will be found
        /// and only those with a confidence above the given threshold will be
        /// returned.
        /// </summary>
        /// <param name="data">The complete dataset, needed for calculating support
        /// since this method is separated from the apriori and therefore can't
        /// reuse it.</param>
        /// <param name="aprioriResult">The frequent itemsets from apriori, so all
        /// association rules also comply to the support threshold of the apriori.</param>
        /// <param name="stringArrayLabel">The parameter to look for patterns in, must
        /// be the same as used in apriori.</param>
        /// <param name="confidenceThreshold">The threshold (between 0.0 and 1.0)
        /// to only get grounded association rules.</param>
        /// <returns>A list of assoiciation rules containing the left term, right term
        /// and the calculated confidence of the association.</returns>
        public static List<Tuple<List<string>, List<string>, double, double>> AprioriAssociationRules
            (List<DataLine> data, List<Tuple<List<string>, int>> aprioriResult, double confidenceThreshold)
        {
            var result = new List<Tuple<List<string>, List<string>, double, double>>();
            foreach (var superSet in aprioriResult.Where(it => it.Item1.Count >= 2)) //No association rules for sets with only one item
            {
                foreach (var subSet in GetSubsetsDeep(superSet.Item1))
                {
                    double confidence = (1.0)*superSet.Item2/CountSupport(subSet, data, "aPrioriLabel"); //TODO: implement caching in stead of counting through entire data set again
                    if (confidence >= confidenceThreshold)
                    {
                        var subtractedSet = SubtractSet(superSet.Item1, subSet);
                        double baseSupport = CountSupport(subtractedSet, data, "aPrioriLabel")/(1.0*data.Count);
                        double lift = confidence/baseSupport;
                        result.Add(new Tuple<List<string>, List<string>, double, double>(subSet, subtractedSet, confidence, lift));
                    }

                }
            }
            return result;
        }

        public static void BackPropagation(List<DataLine> historicalData)
        {
            NeuralNetwork nn = new NeuralNetwork(2, 1, 1, 1);
            Console.WriteLine("Testing 123");

            double[][] input = { new[] { 0.0, 0.0 }, new[] { 1.0, 0.0 }, new[] { 0.0, 1.0 }, new[] { 1.0, 1.0 } };
            double[][] output = new[] { new[] { 0.0 }, new[] { 1.0 }, new[] { 1.0 }, new[] { 0.0 } };
            //double[][][] training = LoadTestData("XOR test data.txt");
            int result = 0;
            int iteration = 1;
            while (result < output.Length && iteration < 10000)
            {
                result = nn.RunSession(input, output);
                Console.WriteLine("iteration {0}, result {1} out of {2}", iteration, result, input.Length);
                iteration++;
            }

            /*// Normalization of historical data
            NormalizeHistorical(historicalData);

            // Training of NN

            // Verification*/
        }

        #region ------------- Private Helper Classes --------------

        private class ListComparer<T> : IEqualityComparer<List<T>>
        {
            /// <summary>
            /// Linear running time.
            /// Runs in O(n) time, n = <see cref="x"/>.Count
            /// </summary>
            public bool Equals(List<T> x, List<T> y)
            {
                if (x.Count != y.Count) return false;
                for (int i = 0; i < x.Count; i++)
                {
                    if (!x[i].Equals(y[i]))
                        return false;
                }
                return true;
            }

            public int GetHashCode(List<T> obj)
            {
                int hashcode = 0;
                foreach (T t in obj)
                {
                    hashcode ^= t.GetHashCode();
                }
                return hashcode;
            }
        }

        #endregion

        #region ------------- Private Helper Methods --------------

        /// <summary>
        /// Calculates the numerical distance between two object using their collected set of values.
        /// </summary>
        /// <param name="a">First DataLine</param>
        /// <param name="b">Second DataLine</param>
        /// <param name="ignoreLabel">This string label will be ignored in the calculation</param>
        /// <returns>The distance between the two objects, 0 = identical</returns>
        private static double CalcDistance(DataLine a, DataLine b, string ignoreLabel)
        {
            double strings = a.hashStrings.Keys.Where(k => !k.Equals(ignoreLabel)).Count(s => a.hashStrings[s] == null || !a.hashStrings[s].Equals(b.hashStrings[s]));
            double doubles = 0.0;
            double booleans = a.hashBooleans.Keys.Count(k => a.hashBooleans[k] != b.hashBooleans[k]);
            double stringArrays = 0.0;

            // doubles
            foreach (string key in a.hashDoubles.Keys)
            {
                if (a.hashDoubles[key] == null || b.hashDoubles[key] == null)
                {
                    doubles += 1.0;
                }
                else
                {
                    doubles += Math.Abs((double)a.hashDoubles[key] - (double)b.hashDoubles[key]);
                }
            }

            // string arrays
            foreach (string key in a.hashStringArrays.Keys)
            {
                if (a.hashStringArrays[key] == null || b.hashStringArrays[key] == null)
                {
                    stringArrays += 1.0;
                }
                else
                {
                    var union = a.hashStringArrays[key].Union(b.hashStringArrays[key]);
                    stringArrays += union.Count(s => !a.hashStringArrays[key].Contains(s) || !b.hashStringArrays[key].Contains(s)) / (1.0 * union.Count());
                }
            }

            return strings + doubles + booleans + stringArrays;
        }

        private static List<string> SubtractSet(IEnumerable<string> superSet, ICollection<string> subSet)
        {
            return superSet.Where(s => !subSet.Contains(s)).ToList();
        }

        private static double Dissimilarity(DataLine a, KMeanCluster c)
        {
            Dictionary<string, double> centroid = c.Centroid;

            return centroid.Sum(kv => Math.Abs(a.hashDoubles[kv.Key] ?? 0.0 - kv.Value)); // TODO: null == 0.0 might not be good
        }

        private static bool Changed(KMeanCluster[] oldClusters, KMeanCluster[] newClusters)
        {
            for (int c = 0; c < oldClusters.Length; c++)
            {
                foreach (string key in oldClusters[c].Centroid.Keys)
                {
                    double diff = Math.Abs(oldClusters[c].Centroid[key] - newClusters[c].Centroid[key]);
                    if (diff >= KMEANS_MINIMUM)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region ------------- Apriori Helper Methods --------------

        /// <summary>
        /// Linear running time (much more complex than just "linear", really).
        /// Runs in O(n) time, n = <see cref="data"/>.Count.
        /// Actually O(n + m^4), n = <see cref="data"/>.Count, m = <see cref="lowerLevelItemSets"/>.Count, but dominated by n (yeah, it's still more complicated than that, but don't blow your brains out).
        /// </summary>
        private static Dictionary<List<string>, int> GenerateFrequentItemSets
            (int supportThreshold, List<DataLine> data, string stringArrayLabel, Dictionary<List<string>, int> lowerLevelItemSets)
        {
            HashSet<List<string>> candidates = new HashSet<List<string>>(new ListComparer<string>());

            /*
             * first generate candidate itemsets from the lower level itemsets
             * Cubic running time (worst case quadroble time) in length of lowerLevelItems.
             * Runs in O(m^3) time (worst case O(m^4)), m = lowerLevelItemSets.Count 
             */
            foreach (List<string> first in lowerLevelItemSets.Keys) //O(m)   <------------------
            {
                foreach (List<string> second in lowerLevelItemSets.Keys) //O(m)   <------------------
                {
                    if (!new ListComparer<string>().Equals(first,second) //O(m)
                        && IdenticalButLast(first, second)) //O(m)
                    {
                        List<string> joined = first.Union(second).OrderBy(s => s).ToList(); //O(m) - worst case O(m^2) but should be close to O(m)   <------------------
                        if (!candidates.Contains(joined))
                        {
                            candidates.Add(joined);
                        }
                    }
                }
            }

            /*
             * Now check the support for all candidates and add only those
             * that have enough support to the set
             * Linear running time in data.
             * Runs in O(n) time, n = data.Count - actually O( c^3 + n) ) time, n = data.Count, c = candidates.Count, but dominated by n - (full O( c*(c^2 + s + n*c) ))
             */
            Dictionary<List<string>, int> result = new Dictionary<List<string>, int>(new ListComparer<string>());
            foreach (List<string> cand in candidates) // O(c)   <------------------
            {
                //Console.WriteLine("Candidate [" + string.Join(",", cand) + "]");
                List<List<string>> subsets = GetSubsets(cand); //O(c^2)   <------------------
                //subsets.ForEach(l => Console.WriteLine("\t["+string.Join(",",l)+"]"));
                if (subsets.All(t => lowerLevelItemSets.ContainsKey(t))) //O(s)
                {
                    int support = CountSupport(cand, data, stringArrayLabel); //O(n*c)   <------------------
                    if (support >= supportThreshold) result[cand] = support;
                }
            }

            return result;
        }

        /// <summary>
        /// Squared running time.
        /// Runs in O(n^2) time, n = <see cref="data"/>.Count
        /// </summary>
        private static Dictionary<List<string>, int> GenerateFrequentItemSetsLevel1(List<DataLine> data, string stringArrayLabel, int supportThreshold)
        {
            // Calculate support for all elements
            Dictionary<List<string>, int> all = new Dictionary<List<string>, int>(new ListComparer<string>());
            foreach (DataLine curArr in data) // O(n)
            {
                foreach (string i in curArr.hashStringArrays[stringArrayLabel])
                {
                    List<string> curSet = new List<string>();
                    curSet.Add(i);
                    if (!all.ContainsKey(curSet))
                    {
                        all[curSet] = CountSupport(curSet, data, stringArrayLabel); // Runs in O(n) - actually O(n*m) but m = 1
                    }
                }
            }

            // Return only elements with minimum support

            Dictionary<List<string>, int> result = new Dictionary<List<string>, int>(new ListComparer<string>());
            foreach (List<string> s in all.Keys) // O(n)
            {
                if (all[s] >= supportThreshold)
                    result[s] = all[s];
            }
            return result;
        }
        
        /// <summary>
        /// Squared running time.
        /// Runs in O(n^2) time, n = <see cref="cand"/>.Count
        /// </summary>
        private static List<List<string>> GetSubsets(List<string> cand)
        {
            List<List<string>> result = new List<List<string>>();
            for (int i = 0; i < cand.Count; i++)
            {
                List<string> copy = cand.ToList();
                copy.RemoveAt(i);
                result.Add(copy);
            }
            return result;
        }

        /// <summary>
        /// Cubic running time.
        /// Runs in O(n^3) time, n = <see cref="cand"/>.Count
        /// </summary>
        private static List<List<string>> GetSubsetsDeep(List<string> cand)
        {
            List<List<string>> result = new List<List<string>>();
            Stack<List<string>> deeper = new Stack<List<string>>(GetSubsets(cand));// Runs in O(n^2) time
            while (deeper.Count > 0) //O(n)
            {
                List<string> cur = deeper.Pop();
                if(!result.Any(r => new ListComparer<string>().Equals(cur))) // O(n^2) - because result.Any is O(n) and Equals is O(n)
                {
                    result.Add(cur);
                    GetSubsets(cur).Where(s => s.Count > 0).ForEach(s => deeper.Push(s)); //Runs in O(n^2) time
                }
            }
            return result;
        }

        /// <summary>
        /// Linear running time.
        /// Runs in O(n) time, n = <see cref="first"/>.Count
        /// </summary>
        private static bool IdenticalButLast(List<string> first, List<string> second)
        {
            for (int i = 0; i < first.Count - 1; i++)
            {
                if (first[i] != second[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// (Almost) Linear running time.
        /// (Probably) Runs in O(n*m) time, n = <see cref="data"/>.Count, m = <see cref="itemSet"/>.Count.
        /// </summary>
        private static int CountSupport(List<string> itemSet, List<DataLine> data, string stringArrayLabel)
        {

            return data.Count(d => itemSet.All(l => d.hashStringArrays[stringArrayLabel].Contains(l)));
            /*// Assumes that items in ItemSets and transactions are both unique
    	    int matches = 0;
    	    for(int[] other : transactions){
    		    if(asSet(other).containsAll(asSet(itemSet))) matches++;
    	    }

            return matches;*/
        }

        #endregion

        #region ------------- Apriori Helper Methods --------------

        private static void NormalizeHistorical(List<DataLine> data)
        {
            DataLine[][] groups = data.GroupBy(bg => bg.hashStrings["year_published"]).Select(g => g.ToArray()).ToArray();

            foreach (DataLine[] year in groups)
            {
                Console.WriteLine("Normalizing year " + year[0].hashStrings["year_published"]);
                for(int i = 0; i < year[0].hashDoubleArrays.First().Value.Length; i++) // each value in the historical data
                {
                    IEnumerable<double> values = year[0].hashDoubleArrays.Keys.SelectMany(k => year.Select(g => g.hashDoubleArrays[k][i]));
                    double min = values.Min();
                    double max = values.Max();
                    Console.WriteLine("\tHData " + i + ": min=" + min + " max=" + max);

                    year.ForEach(g => g.hashDoubleArrays.Keys.ForEach(k => g.hashDoubleArrays[k][i] = (g.hashDoubleArrays[k][i] - min) / (max - min)));
                }
            }
            Console.WriteLine("line");
        }

        #endregion
    }
}