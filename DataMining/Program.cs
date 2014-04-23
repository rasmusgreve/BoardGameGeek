using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMining.Neural_Networks;
using System.Threading;
using DataMiningIndividual;

namespace DataMining
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
            NeuralNetwork nn = new NeuralNetwork(2,1,1,1);
            double[][] input = { new[] { 0.0, 0.0 }, new[] { 1.0, 0.0 }, new[] { 0.0, 1.0 }, new[] { 1.0, 1.0 } };
            double[][] output = new[] {new[] {0.0}, new[] {1.0}, new[] {1.0}, new[] {0.0}};
            //double[][][] training = LoadTestData("XOR test data.txt");
            int result = 0;
            int iteration = 1;
            while (result < output.Length && iteration < 50000)
            {
                result = nn.RunSession(input, output);
                Console.WriteLine("iteration {0}, result {1} out of {2}",iteration,result,output.Length);
                iteration++;
            }
            //PerformDM(args.Length > 0 ? args[0] : "data2014-04-03_03-35-14.csv");
            Console.ReadLine();
        }

        private static double[][][] LoadTestData(string xorTestDataTxt)
        {
            double[][] input = new double[4][];
            double[][] output = new double[4][];
            StreamReader f = File.OpenText(xorTestDataTxt);
            int i = 0;
            while (!f.EndOfStream)
            {
                string l = f.ReadLine();
                string[] all = l.Split(':');
                string outp = all.Last();
                string[] inp = all.First().Split(' ');
                output[i] = new []{Convert.ToDouble(outp)};
                input[i] = new double[inp.Length];
                for (int j = 0; j < inp.Length; j++)
                {
                    input[i][j] = Convert.ToDouble(inp);
                }

                i++;
            }
            return new[] {input, output};
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
            answers = answers.Take(20000).ToList();

            List<DataLine> historical = DataLine.ParseHistorical(CSVParser.ReadDataFile("data2014-04-09_09-11-52-historical.csv", ";", null))
                .Take(5000).ToList();
            Console.WriteLine("Historical Loaded");

            Console.WriteLine("Parsing Complete.\n");

            // create output after successful parsing
            TextWriter output = Console.Out;

            DataMining.BackPropagation(historical);
            
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
            int supportThreshold = answers.Count / 4;
            Console.WriteLine("Datalines: " + answers.Count);
            var patterns = DataMining.Apriori(answers, supportThreshold, aprioriLabels);
            patterns.Sort((tuple, tuple1) => tuple.Item2 - tuple1.Item2);
            foreach (Tuple<List<string>, int> list in patterns)
            {
                Console.WriteLine("Support: " + list.Item2 + " / " + Math.Round((100d * list.Item2) / answers.Count, 1) + "%: [" + string.Join(",", list.Item1.Select(DataLine.IDtoLabel)) + "]");
            }
            
         
            //string aprioriLabel = "";

            // Assiciation Rules
            var ass = DataMining.AprioriAssociationRules(answers, patterns, 0.7);

            Console.WriteLine();
            ass.Sort((tuple, tuple1) => Math.Sign(tuple.Item4 - tuple1.Item4));
            
            foreach (var cheek in ass)

                Console.WriteLine("Conf=" + Math.Round(cheek.Item3*100, 1) + "% lift=" + Math.Round(cheek.Item4, 2)+": [" + string.Join(",", cheek.Item1.Select(DataLine.IDtoLabel)) + "] => \t[" + string.Join(",", cheek.Item2.Select(DataLine.IDtoLabel)) + "]");
            }
        }
}
