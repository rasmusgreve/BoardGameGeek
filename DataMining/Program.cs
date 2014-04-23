using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiningIndividual
{
    class Program
    {

        /// <summary>
        /// The main method for running the analysis on the two datasets.
        /// </summary>
        /// <param name="args">Not used</param>
        static void Main(string[] args)
        {
            Console.WriteLine("Testing 123");
            PerformDM(args.Length > 0 ? args[0] : "data2014-04-03_03-35-14.csv");
            Console.ReadLine();
        }

        /// <summary>
        /// Performs various Data Mining routines on the data located in the given .csv
        /// file. The actions are as follows:
        /// 1. Load data as 2-dim string array.
        /// 2. Infer types and create DataLine objects.
        /// 3. Normalize numerical values to the range of 0-1 (MinMaxNormalization)
        /// 4. Test classification on first string parameter and see hit rate (kNN classification)
        /// 5. Test clustering using all parameters (kMeans clustering)
        /// </summary>
        /// <param name="file">The filename of the input (without ".csv"). The results will be
        /// saved under the same name with "-output.txt" at the end.</param>
        private static void PerformDM(string file)
        {
            Console.WriteLine("Parsing started.");

            string[][] data = CSVParser.ReadDataFile(file, ";", null);
            Console.WriteLine("Read datalines");

            DataLine.linkDictionary = CSVParser.ReadLinkFile("linkIdNames.txt");
            Console.WriteLine("Read link file");

            List<DataLine> answers = DataLine.ParseFixed(data);
            answers = answers.Take(5000).ToList();

            List<DataLine> historical = DataLine.ParseHistorical(CSVParser.ReadDataFile("data2014-04-09_09-11-52-historical.csv", ";", null))
                .Take(5000).ToList();
            Console.WriteLine("Historical Loaded");

            Console.WriteLine("Parsing Complete.\n");

            // create output after successful parsing
            TextWriter output = Console.Out;
            
            //DataMining.minMaxNormalize(answers);
            /*
            for (int i = 0; i < answers.Count; i++)
            {
                Print(output,answers[i]);
            }

            int correct = 0;
            for (int i = 0; i < answers.Count; i++)
            {
                string key = answers[i].hashStrings.Keys.First();
                string guessed = DataMining.kNN(answers.Where(a => !a.Equals(answers[i])).ToList(), answers[i], key, 3);
                Print(output,"os: real=" + answers[i].hashStrings[key] + " kNN: " + guessed);
                if (answers[i].hashStrings[key] != null && (answers[i].hashStrings[key].Contains(guessed) || guessed.Contains(answers[i].hashStrings[key])))
                    correct++;
            }
            Print(output,"= " + correct + "/" + answers.Count + " guessed right.");

            // KMeans
            List<KMeanCluster> clusters = DataMining.KMeansPartition(3, answers);
            Print(output,"\nkMeans clustering: ");
            for (int c = 0; c < clusters.Count; c++)
            {
                Print(output,"Cluster #" + c);
                Print(output,clusters[c] + "\n");
            }
            */
            // Apriori
            var aprioriLabels = new string[] { "mechanics", "categories", "min_players", "max_players", "playingtime", "average_rating" };
            int supportThreshold = answers.Count / 20;
            Console.WriteLine("Datalines: " + answers.Count);
            List<Tuple<List<string>,int>> patterns = DataMining.Apriori(answers, supportThreshold, aprioriLabels);
            foreach (Tuple<List<string>, int> list in patterns)
            {
                //TODO: Replace ints with names in list.Item1
                for (int i = 0; i < list.Item1.Count; i++)
                {
                    try
                    {
                        int v = int.Parse(list.Item1[i]);
                        list.Item1[i] = DataLine.IDtoLabel(v);
                    }
                    catch
                    {
                    }
                }
                Print(output, "["+string.Join(",", list.Item1)+"] = "+list.Item2);
            }

            Print(output,"");
            /*

            string aprioriLabel = "";
            // Assiciation Rules
            List<Tuple<List<string>, List<string>, double>> ass = DataMining.AprioriAssociationRules(answers, patterns.Select(p => p.Item1).ToList(), aprioriLabel, 0.7);
            foreach (Tuple<List<string>, List<string>, double> assiciation in ass)
            {
                Print(output, "[" + string.Join(",", assiciation.Item1) + "] => [" + string.Join(",", assiciation.Item2) + "] == " + assiciation.Item3);
            }
            */
            Console.WriteLine("DONE");
        }

        /// <summary>
        /// Prints the ToString() value of the given object to the Stream.
        /// </summary>
        /// <param name="target">The stream to write to.</param>
        /// <param name="o">The object that wants written.</param>
        private static void Print(TextWriter target, Object o) { Print(target,o.ToString()); }

        /// <summary>
        /// Prints the given string to the Stream.
        /// </summary>
        /// <param name="target">The stream to write to.</param>
        /// <param name="str">The string that wants written.</param>
        private static void Print(TextWriter target, String str)
        {
            target.WriteLine(str);
            target.Flush();
        }
    }
}
