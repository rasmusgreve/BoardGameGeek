using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiningIndividual
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
        /// <param name="supportThreshold">The minimum number of occurences to call a pattern
        /// frequent.</param>
        /// <param name="stringArrayLabel">The parameter to look for patterns in.</param>
        /// <returns>A list of patterns (lists of strings) who comply with the 
        /// support threshold.</returns>
        public static List<Tuple<List<string>, int>> Apriori(List<DataLine> data, int supportThreshold, string stringArrayLabel)
        {
            data.ForEach(d => Array.Sort(d.hashStringArrays[stringArrayLabel]));

            int k;
            Dictionary<List<string>, int> frequentItemSets = GenerateFrequentItemSetsLevel1(data, stringArrayLabel, supportThreshold);
            List<Tuple<List<string>, int>> result = new List<Tuple<List<string>, int>>(frequentItemSets.Select(kv => new Tuple<List<string>, int>(kv.Key, kv.Value)));
            for (k = 1; frequentItemSets.Count > 0; k++)
            {
                Console.WriteLine("Finding frequent itemsets of length " + (k + 1));
                frequentItemSets = GenerateFrequentItemSets(supportThreshold, data, stringArrayLabel, frequentItemSets);
                result.AddRange(frequentItemSets.Select(kv => new Tuple<List<string>, int>(kv.Key, kv.Value)));

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
        public static List<Tuple<List<string>, List<string>, double>> AprioriAssociationRules(List<DataLine> data, List<List<string>> aprioriResult, 
            string stringArrayLabel, double confidenceThreshold)
        {
            List<Tuple<List<string>, List<string>, double>> result = new List<Tuple<List<string>, List<string>, double>>();
            foreach(List<string> l in aprioriResult){
                foreach (List<string> s in GetSubsetsDeep(l))
                {
                    List<string> lminusS = new List<string>(l);
                    s.ForEach(i => lminusS.Remove(i));

                    double confidence = (CountSupport(l, data, stringArrayLabel) * 1.0) / CountSupport(s, data, stringArrayLabel);

                    if (confidence > confidenceThreshold)
                    {
                        result.Add(new Tuple<List<string>, List<string>, double>(s, lminusS, confidence));
                    }
                }
            }

            return result;
        }

        // ------------- Private Helper Classes --------------

        private class ListComparer<T> : IEqualityComparer<List<T>>
        {
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

        // ------------- Private Helper Methods --------------

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

        // APRIORI

        private static Dictionary<List<string>, int> GenerateFrequentItemSets(int supportThreshold, List<DataLine> data,
                    string stringArrayLabel, Dictionary<List<string>, int> lowerLevelItemSets)
        {
            HashSet<List<string>> candidates = new HashSet<List<string>>(new ListComparer<string>());

            // first generate candidate itemsets from the lower level itemsets
            foreach (List<string> first in lowerLevelItemSets.Keys)
            {
                foreach (List<string> second in lowerLevelItemSets.Keys)
                {
                    if (!new ListComparer<string>().Equals(first,second) && IdenticalButLast(first, second))
                    {
                        List<string> joined = first.Union(second).OrderBy(s => s).ToList();
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
             */
            Dictionary<List<string>, int> result = new Dictionary<List<string>, int>(new ListComparer<string>());
            foreach (List<string> cand in candidates)
            {
                //Console.WriteLine("Candidate [" + string.Join(",", cand) + "]");
                List<List<string>> subsets = GetSubsets(cand);
                //subsets.ForEach(l => Console.WriteLine("\t["+string.Join(",",l)+"]"));
                if (subsets.All(t => lowerLevelItemSets.ContainsKey(t)))
                {
                    int support = CountSupport(cand, data, stringArrayLabel);
                    if (support >= supportThreshold) result[cand] = support;
                }
            }

            return result;
        }

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

        private static List<List<string>> GetSubsetsDeep(List<string> cand)
        {
            List<List<string>> result = new List<List<string>>();
            Stack<List<string>> deeper = new Stack<List<string>>(GetSubsets(cand));
            while (deeper.Count > 0)
            {
                List<string> cur = deeper.Pop();
                if(!result.Any(r => new ListComparer<string>().Equals(cur)))
                {
                    result.Add(cur);
                    GetSubsets(cur).Where(s => s.Count > 0).ForEach(s => deeper.Push(s));
                }
            }
            return result;
        }

        private static bool IdenticalButLast(List<string> first, List<string> second)
        {
            for (int i = 0; i < first.Count - 1; i++)
            {
                if (first[i] != second[i]) return false;
            }
            return true;
        }

        private static Dictionary<List<string>, int> GenerateFrequentItemSetsLevel1(List<DataLine> data, string stringArrayLabel, int supportThreshold)
        {
            Dictionary<List<string>, int> temp = new Dictionary<List<string>, int>(new ListComparer<string>());
            foreach (DataLine curArr in data)
            {
                foreach (string i in curArr.hashStringArrays[stringArrayLabel])
                {
                    List<string> curSet = new List<string>();
                    curSet.Add(i);
                    if (!temp.ContainsKey(curSet))
                    {
                        temp[curSet] = CountSupport(curSet, data, stringArrayLabel);
                    }
                }
            }

            // remove under Threshold
            Dictionary<List<string>, int> result = new Dictionary<List<string>, int>(new ListComparer<string>());
            foreach (List<string> s in temp.Keys)
            {
                if (temp[s] >= supportThreshold)
                    result[s] = temp[s];
            }
            return result;
        }

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
    }
}