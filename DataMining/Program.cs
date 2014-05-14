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
            //List<DataLine> historical = DataLine.ParseHistorical(CSVParser.ReadDataFile("data2014-04-09_09-11-52-historical.csv", ";", null)).ToList();
            //DataMining.BackPropagation(historical);

            DataMining.MissingValues();
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
            answers = answers.Take(20000).ToList();

            List<DataLine> historical = DataLine.ParseHistorical(CSVParser.ReadDataFile("data2014-04-09_09-11-52-historical.csv", ";", null)).ToList();
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
            }
        }
}
